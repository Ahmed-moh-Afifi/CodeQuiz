import { Component, signal, computed, inject, OnInit, OnDestroy } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { Subscription } from 'rxjs';
import {
  CqButton,
  CqBadge,
  CqTextarea,
  CqDialogService,
  CqProgressBar,
  CqSkeleton,
  CqQuestionNav,
  CqIcon,
  CqCodeEditor,
} from '../../../design-system';
import type { QuestionNavItem } from '../../../design-system';
import { AttemptsService } from '../../services/attempts.service';
import { ExecutionService } from '../../services/execution.service';
import type { ExamineeAttempt, SolutionDTO } from '../../api/models/attempt.models';
import type { QuestionDTO } from '../../api/models/quiz.models';
import type { RunCodeRequest } from '../../api/models/execution.models';

@Component({
  selector: 'app-take-quiz',
  standalone: true,
  imports: [
    CqButton,
    CqBadge,
    CqTextarea,
    FormsModule,
    CqProgressBar,
    CqSkeleton,
    CqQuestionNav,
    CqIcon,
    CqCodeEditor,
  ],
  templateUrl: './take-quiz.html',
  styleUrl: './take-quiz.scss',
})
export class TakeQuiz implements OnInit, OnDestroy {
  private readonly attemptsService = inject(AttemptsService);
  private readonly executionService = inject(ExecutionService);
  private readonly dialogService = inject(CqDialogService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private subs: Subscription[] = [];

  loading = signal(true);
  submitting = signal(false);
  isAutoSaving = signal(false);
  isRunningCode = signal(false);
  lastSavedText = signal('Not saved yet');

  attempt = signal<ExamineeAttempt | null>(null);
  selectedQuestion = signal<QuestionDTO | null>(null);
  codeInEditor = signal('');
  input = signal('');
  output = signal('');

  remainingTime = signal('--:--:--');
  private timerInterval: ReturnType<typeof setInterval> | null = null;
  private autoSaveTimer: ReturnType<typeof setTimeout> | null = null;

  questions = computed(() => this.attempt()?.quiz?.questions ?? []);

  navItems = computed<QuestionNavItem[]>(() =>
    this.questions().map((q) => ({
      id: q.id,
      order: q.order,
      answered: this.isQuestionAnswered(q),
    })),
  );

  quizProgress = computed(() => {
    const qs = this.questions();
    const sel = this.selectedQuestion();
    if (!qs.length || !sel) return 0;
    return sel.order / qs.length;
  });

  selectedQuestionConfig = computed(() => this.selectedQuestion()?.questionConfiguration);

  isTimerDanger = computed(() => this.remainingTime().startsWith('00:0'));

  async ngOnInit() {
    const attemptId = this.route.snapshot.queryParams['attemptId'];
    const quizCode = this.route.snapshot.queryParams['quizCode'];

    if (!attemptId && !quizCode) {
      this.router.navigate(['/app/joined']);
      return;
    }

    this.loading.set(true);
    try {
      let attempt: ExamineeAttempt;
      if (quizCode) {
        attempt = await this.attemptsService.beginAttempt({ quizCode });
      } else {
        // Re-begin to get the latest attempt state
        attempt = await this.attemptsService.beginAttempt({ quizCode: '' });
      }
      this.attempt.set(attempt);

      // Select first question
      const firstQ = attempt.quiz.questions.find((q) => q.order === 1);
      if (firstQ) {
        this.selectQuestion(firstQ, false);
      }

      this.startTimer();

      // Subscribe to auto-submit
      this.subs.push(
        this.attemptsService.examineeAttemptChanges$.subscribe((change) => {
          if (
            change.attempt.id === this.attempt()?.id &&
            (change.attempt as ExamineeAttempt).endTime
          ) {
            this.handleAutoSubmitted();
          }
        }),
      );
    } finally {
      this.loading.set(false);
    }
  }

  ngOnDestroy() {
    this.subs.forEach((s) => s.unsubscribe());
    if (this.timerInterval) clearInterval(this.timerInterval);
    if (this.autoSaveTimer) clearTimeout(this.autoSaveTimer);
  }

  // --- Timer ---
  private startTimer() {
    this.updateRemainingTime();
    this.timerInterval = setInterval(() => this.updateRemainingTime(), 1000);
  }

  private updateRemainingTime() {
    const att = this.attempt();
    if (!att) return;

    const maxEnd = new Date(att.maxEndTime).getTime();
    const now = Date.now();
    const diff = maxEnd - now;

    if (diff <= 0) {
      this.remainingTime.set('00:00:00');
      if (this.timerInterval) clearInterval(this.timerInterval);
      this.handleTimeExpired();
      return;
    }

    const h = Math.floor(diff / 3600000);
    const m = Math.floor((diff % 3600000) / 60000);
    const s = Math.floor((diff % 60000) / 1000);
    this.remainingTime.set(
      `${h.toString().padStart(2, '0')}:${m.toString().padStart(2, '0')}:${s.toString().padStart(2, '0')}`,
    );
  }

  // --- Navigation ---
  goBack() {
    this.router.navigate(['/app']);
  }

  onNavSelect(item: QuestionNavItem) {
    const q = this.questions().find((q) => q.id === item.id);
    if (q) this.selectQuestion(q);
  }

  // --- Question Navigation ---
  selectQuestion(question: QuestionDTO, save = true) {
    if (save) this.saveSolution();

    this.selectedQuestion.set(question);
    const att = this.attempt();
    if (att) {
      const solution = att.solutions.find((s) => s.questionId === question.id);
      this.codeInEditor.set(solution?.code ?? question.editorCode ?? '');
    }
    this.output.set('');
  }

  nextQuestion() {
    const sel = this.selectedQuestion();
    const qs = this.questions();
    if (!sel || sel.order >= qs.length) return;
    const next = qs.find((q) => q.order === sel.order + 1);
    if (next) this.selectQuestion(next);
  }

  previousQuestion() {
    const sel = this.selectedQuestion();
    if (!sel || sel.order <= 1) return;
    const prev = this.questions().find((q) => q.order === sel.order - 1);
    if (prev) this.selectQuestion(prev);
  }

  // --- Code changes + auto-save ---
  onCodeChange(value: string) {
    this.codeInEditor.set(value);
    // Debounced auto-save
    if (this.autoSaveTimer) clearTimeout(this.autoSaveTimer);
    this.autoSaveTimer = setTimeout(() => this.autoSave(), 2000);
  }

  private async autoSave() {
    if (this.isAutoSaving() || this.submitting()) return;
    this.isAutoSaving.set(true);
    try {
      await this.saveSolution();
    } finally {
      this.isAutoSaving.set(false);
    }
  }

  private async saveSolution(): Promise<boolean> {
    const att = this.attempt();
    const sel = this.selectedQuestion();
    if (!att || !sel) return false;

    const solution = att.solutions.find((s) => s.questionId === sel.id);
    if (!solution) return false;

    try {
      solution.code = this.codeInEditor();
      await this.attemptsService.updateSolution(solution);
      this.lastSavedText.set('Just now');
      setTimeout(() => {
        const now = this.lastSavedText();
        if (now === 'Just now') this.lastSavedText.set('Saved');
      }, 5000);
      return true;
    } catch {
      return false;
    }
  }

  // --- Run Code ---
  async onRunRequested(event: { code: string; input: string }) {
    if (this.isRunningCode()) return;
    const config = this.selectedQuestionConfig();
    if (!config?.allowExecution) return;

    this.isRunningCode.set(true);
    this.output.set('Running...');
    try {
      const request: RunCodeRequest = {
        language: config.language,
        code: event.code,
        input: event.input.split('\n').filter((l) => l.length > 0),
        containOutput: config.showOutput,
        containError: config.showError,
      };

      const result = await this.executionService.runCode(request);
      if (result.success) {
        this.output.set(
          config.showOutput ? (result.output ?? 'Success') : 'Code executed successfully',
        );
      } else {
        this.output.set(config.showError ? (result.error ?? 'Error') : 'Code execution failed');
      }
    } catch {
      this.output.set('Failed to run code');
    } finally {
      this.isRunningCode.set(false);
    }
  }

  // --- Run Code (legacy) ---
  async runCode() {
    if (this.isRunningCode()) return;
    const config = this.selectedQuestionConfig();
    if (!config?.allowExecution) return;

    this.isRunningCode.set(true);
    this.output.set('Running...');
    try {
      const request: RunCodeRequest = {
        language: config.language,
        code: this.codeInEditor(),
        input: this.input()
          .split('\n')
          .filter((l) => l.length > 0),
        containOutput: config.showOutput,
        containError: config.showError,
      };

      const result = await this.executionService.runCode(request);
      if (result.success) {
        this.output.set(
          config.showOutput ? (result.output ?? 'Success') : 'Code executed successfully',
        );
      } else {
        this.output.set(config.showError ? (result.error ?? 'Error') : 'Code execution failed');
      }
    } catch {
      this.output.set('Failed to run code');
    } finally {
      this.isRunningCode.set(false);
    }
  }

  // --- Submit ---
  async submitQuiz() {
    if (this.submitting()) return;

    const ref = this.dialogService.confirm(
      'warning',
      'Submit Quiz',
      "Are you sure you want to submit? You won't be able to make changes after.",
      'Submit',
      'Cancel',
    );

    ref.afterClosed$.subscribe(async (confirmed) => {
      if (!confirmed) return;

      this.submitting.set(true);
      if (this.timerInterval) clearInterval(this.timerInterval);

      try {
        await this.saveSolution();
        await this.attemptsService.submitAttempt(this.attempt()!.id);
        this.router.navigate(['/app/joined']);
      } catch {
        this.submitting.set(false);
      }
    });
  }

  private async handleTimeExpired() {
    if (this.submitting()) return;
    this.submitting.set(true);

    try {
      await this.saveSolution();
    } catch {
      /* best effort */
    }

    try {
      await this.attemptsService.submitAttempt(this.attempt()!.id);
    } catch {
      /* backend will auto-submit */
    }

    this.router.navigate(['/app/joined']);
  }

  private handleAutoSubmitted() {
    if (this.timerInterval) clearInterval(this.timerInterval);
    this.router.navigate(['/app/joined']);
  }

  // --- Helpers ---
  isQuestionAnswered(question: QuestionDTO): boolean {
    const att = this.attempt();
    if (!att) return false;
    const sol = att.solutions.find((s) => s.questionId === question.id);
    return !!sol?.code?.trim();
  }

  formatTestInput(tc: { input?: string[] }): string {
    return tc.input?.join('\n') ?? '';
  }
}
