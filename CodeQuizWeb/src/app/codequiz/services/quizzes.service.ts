import { Injectable, inject, OnDestroy } from '@angular/core';
import { Subject } from 'rxjs';
import { firstValueFrom } from 'rxjs';
import * as signalR from '@microsoft/signalr';
import { QuizzesApiService } from '../api/services/quizzes-api.service';
import { AuthStore } from '../api/auth.store';
import { HUB_BASE_URL } from '../api/api.config';
import type { NewQuizModel, ExaminerQuiz, ExamineeQuiz } from '../api/models/quiz.models';
import type { ExaminerAttempt } from '../api/models/attempt.models';

export type ChangeAction = 'created' | 'updated' | 'deleted';

export interface QuizChange<T = ExaminerQuiz | ExamineeQuiz> {
  action: ChangeAction;
  quiz: T;
}

@Injectable({ providedIn: 'root' })
export class QuizzesService implements OnDestroy {
  private readonly quizzesApi = inject(QuizzesApiService);
  private readonly authStore = inject(AuthStore);
  private readonly hubBaseUrl = inject(HUB_BASE_URL);

  private connection: signalR.HubConnection | null = null;
  private connectedUserId: string | null = null;
  private readonly joinedQuizGroups = new Set<number>();
  private initPromise: Promise<void> | null = null;

  /** Emits whenever a quiz the current user owns is created/updated/deleted via SignalR */
  private readonly _examinerQuizChanges = new Subject<QuizChange<ExaminerQuiz>>();
  readonly examinerQuizChanges$ = this._examinerQuizChanges.asObservable();

  /** Emits whenever an examinee-facing quiz change is received via SignalR */
  private readonly _examineeQuizChanges = new Subject<QuizChange<ExamineeQuiz>>();
  readonly examineeQuizChanges$ = this._examineeQuizChanges.asObservable();

  // ── API methods ──────────────────────────────────────────────────────

  async createQuiz(model: NewQuizModel): Promise<ExaminerQuiz> {
    const response = await firstValueFrom(this.quizzesApi.createQuiz(model));
    return response.data!;
  }

  async updateQuiz(id: number, model: NewQuizModel): Promise<ExaminerQuiz> {
    const response = await firstValueFrom(this.quizzesApi.updateQuiz(id, model));
    return response.data!;
  }

  async getMyQuizzes(): Promise<ExaminerQuiz[]> {
    const response = await firstValueFrom(this.quizzesApi.getMyQuizzes());
    return response.data!;
  }

  async getUserQuizzes(userId: string): Promise<ExaminerQuiz[]> {
    const response = await firstValueFrom(this.quizzesApi.getUserQuizzes(userId));
    return response.data!;
  }

  async deleteQuiz(id: number): Promise<void> {
    await firstValueFrom(this.quizzesApi.deleteQuiz(id));
  }

  async getQuizByCode(code: string): Promise<ExamineeQuiz> {
    const response = await firstValueFrom(this.quizzesApi.getQuizByCode(code));
    return response.data!;
  }

  async getQuizAttempts(quizId: number): Promise<ExaminerAttempt[]> {
    const response = await firstValueFrom(this.quizzesApi.getQuizAttempts(quizId));
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
      .withUrl(`${this.hubBaseUrl}/Quizzes`, {
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

    // Join examiner group to receive quiz ownership events
    await this.connection.invoke('JoinExaminerGroup', userId);
  }

  private registerHandlers(): void {
    if (!this.connection) return;

    this.connection.on('QuizCreated', (quiz: ExaminerQuiz) => {
      this._examinerQuizChanges.next({ action: 'created', quiz });
    });

    this.connection.on('QuizUpdated', (quiz: ExaminerQuiz) => {
      this._examinerQuizChanges.next({ action: 'updated', quiz });
    });

    this.connection.on('QuizDeleted', (quiz: ExaminerQuiz) => {
      this._examinerQuizChanges.next({ action: 'deleted', quiz });
    });
  }

  private async rejoinGroups(): Promise<void> {
    if (!this.connection || !this.connectedUserId) return;

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
    this._examinerQuizChanges.complete();
    this._examineeQuizChanges.complete();
  }
}
