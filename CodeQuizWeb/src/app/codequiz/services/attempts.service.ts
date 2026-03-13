import { Injectable, inject, OnDestroy } from '@angular/core';
import { Subject } from 'rxjs';
import { firstValueFrom } from 'rxjs';
import * as signalR from '@microsoft/signalr';
import { AttemptsApiService } from '../api/services/attempts-api.service';
import { AuthStore } from '../api/auth.store';
import { HUB_BASE_URL } from '../api/api.config';
import type {
  BeginAttemptRequest,
  ExamineeAttempt,
  ExaminerAttempt,
  SolutionDTO,
  UpdateSolutionGradeRequest,
  BatchUpdateSolutionGradesRequest,
} from '../api/models/attempt.models';

export type ChangeAction = 'created' | 'updated' | 'deleted';

export interface AttemptChange<T = ExaminerAttempt | ExamineeAttempt> {
  action: ChangeAction;
  attempt: T;
}

export type EvaluationStatusType =
  | 'started'
  | 'system-grading-complete'
  | 'ai-assessment-complete'
  | 'failed';

export interface EvaluationStatusEvent {
  type: EvaluationStatusType;
  attemptId: number;
  message?: string;
  attempt?: ExaminerAttempt;
}

@Injectable({ providedIn: 'root' })
export class AttemptsService implements OnDestroy {
  private readonly attemptsApi = inject(AttemptsApiService);
  private readonly authStore = inject(AuthStore);
  private readonly hubBaseUrl = inject(HUB_BASE_URL);

  private connection: signalR.HubConnection | null = null;
  private connectedUserId: string | null = null;
  private readonly joinedQuizGroups = new Set<number>();
  private initPromise: Promise<void> | null = null;

  /** Emits examiner-facing attempt changes (created/updated/deleted) */
  private readonly _examinerAttemptChanges = new Subject<AttemptChange<ExaminerAttempt>>();
  readonly examinerAttemptChanges$ = this._examinerAttemptChanges.asObservable();

  /** Emits examinee-facing attempt changes (auto-submit, created, updated, deleted) */
  private readonly _examineeAttemptChanges = new Subject<AttemptChange<ExamineeAttempt>>();
  readonly examineeAttemptChanges$ = this._examineeAttemptChanges.asObservable();

  /** Emits evaluation status events (grading progress, AI assessment, failures) */
  private readonly _evaluationStatus = new Subject<EvaluationStatusEvent>();
  readonly evaluationStatus$ = this._evaluationStatus.asObservable();

  // ── API methods ──────────────────────────────────────────────────────

  async beginAttempt(request: BeginAttemptRequest): Promise<ExamineeAttempt> {
    const response = await firstValueFrom(this.attemptsApi.beginAttempt(request));
    return response.data!;
  }

  async submitAttempt(attemptId: number): Promise<ExamineeAttempt> {
    const response = await firstValueFrom(this.attemptsApi.submitAttempt(attemptId));
    return response.data!;
  }

  async updateSolution(solution: SolutionDTO): Promise<SolutionDTO> {
    const response = await firstValueFrom(this.attemptsApi.updateSolution(solution));
    return response.data!;
  }

  async updateSolutionGrade(request: UpdateSolutionGradeRequest): Promise<SolutionDTO> {
    const response = await firstValueFrom(this.attemptsApi.updateSolutionGrade(request));
    return response.data!;
  }

  async batchUpdateSolutionGrades(
    request: BatchUpdateSolutionGradesRequest,
  ): Promise<SolutionDTO[]> {
    const response = await firstValueFrom(this.attemptsApi.batchUpdateSolutionGrades(request));
    return response.data!;
  }

  async aiReassessSolution(solutionId: number): Promise<SolutionDTO> {
    const response = await firstValueFrom(this.attemptsApi.aiReassessSolution(solutionId));
    return response.data!;
  }

  async getMyAttempts(): Promise<ExamineeAttempt[]> {
    const response = await firstValueFrom(this.attemptsApi.getMyAttempts());
    return response.data!;
  }

  async getAttempt(attemptId: number): Promise<ExaminerAttempt> {
    const response = await firstValueFrom(this.attemptsApi.getAttempt(attemptId));
    return response.data!;
  }

  // ── SignalR ──────────────────────────────────────────────────────────

  async ensureConnected(): Promise<void> {
    if (!this.initPromise) {
      this.initPromise = this.initializeConnection();
    }
    return this.initPromise;
  }

  private async initializeConnection(): Promise<void> {
    const userId = this.authStore.user()?.id;
    if (!userId) return;

    // Dispose if user changed
    if (this.connection && this.connectedUserId !== userId) {
      await this.disposeConnection();
    }

    if (this.connection) return;

    this.connection = new signalR.HubConnectionBuilder()
      .withUrl(`${this.hubBaseUrl}/Attempts`, {
        accessTokenFactory: () => this.authStore.accessToken() ?? '',
      })
      .withAutomaticReconnect()
      .build();

    this.registerHandlers();

    this.connection.onreconnected(async () => {
      await this.rejoinGroups();
    });

    await this.connection.start();
    this.connectedUserId = userId;

    // Join user + examiner groups
    await this.connection.invoke('JoinUserGroup', userId);
    await this.connection.invoke('JoinExaminerGroup', userId);
  }

  private registerHandlers(): void {
    if (!this.connection) return;

    // Examinee-specific: auto-submitted attempts
    this.connection.on('AttemptAutoSubmitted', (attempt: ExamineeAttempt) => {
      this._examineeAttemptChanges.next({ action: 'updated', attempt });
    });

    // Dual-type attempt lifecycle events
    this.connection.on('AttemptCreated', (examinerAttempt: ExaminerAttempt) => {
      this._examinerAttemptChanges.next({ action: 'created', attempt: examinerAttempt });
    });

    this.connection.on('AttemptUpdated', (examinerAttempt: ExaminerAttempt) => {
      this._examinerAttemptChanges.next({ action: 'updated', attempt: examinerAttempt });
    });

    this.connection.on('AttemptDeleted', (examinerAttempt: ExaminerAttempt) => {
      this._examinerAttemptChanges.next({ action: 'deleted', attempt: examinerAttempt });
    });

    // Evaluation lifecycle events
    this.connection.on('EvaluationStarted', (attemptId: number) => {
      this._evaluationStatus.next({ type: 'started', attemptId });
    });

    this.connection.on('SystemGradingComplete', (attemptId: number, attempt: ExaminerAttempt) => {
      this._evaluationStatus.next({
        type: 'system-grading-complete',
        attemptId,
        attempt,
      });
      this._examinerAttemptChanges.next({ action: 'updated', attempt });
    });

    this.connection.on('AiAssessmentComplete', (attemptId: number, attempt: ExaminerAttempt) => {
      this._evaluationStatus.next({
        type: 'ai-assessment-complete',
        attemptId,
        attempt,
      });
      this._examinerAttemptChanges.next({ action: 'updated', attempt });
    });

    this.connection.on('EvaluationFailed', (attemptId: number, message: string) => {
      this._evaluationStatus.next({ type: 'failed', attemptId, message });
    });
  }

  private async rejoinGroups(): Promise<void> {
    if (!this.connection || !this.connectedUserId) return;

    await this.connection.invoke('JoinUserGroup', this.connectedUserId);
    await this.connection.invoke('JoinExaminerGroup', this.connectedUserId);
    for (const quizId of this.joinedQuizGroups) {
      await this.connection.invoke('JoinQuizGroup', quizId);
    }
  }

  async joinQuizGroup(quizId: number): Promise<void> {
    await this.ensureConnected();
    if (this.connection && !this.joinedQuizGroups.has(quizId)) {
      await this.connection.invoke('JoinQuizGroup', quizId);
      this.joinedQuizGroups.add(quizId);
    }
  }

  async leaveQuizGroup(quizId: number): Promise<void> {
    if (this.connection && this.joinedQuizGroups.has(quizId)) {
      await this.connection.invoke('LeaveQuizGroup', quizId);
      this.joinedQuizGroups.delete(quizId);
    }
  }

  private async disposeConnection(): Promise<void> {
    if (this.connection) {
      try {
        await this.connection.stop();
      } catch {
        // Connection may already be stopped
      }
      this.connection = null;
      this.connectedUserId = null;
      this.joinedQuizGroups.clear();
      this.initPromise = null;
    }
  }

  ngOnDestroy(): void {
    this.disposeConnection();
    this._examinerAttemptChanges.complete();
    this._examineeAttemptChanges.complete();
    this._evaluationStatus.complete();
  }
}
