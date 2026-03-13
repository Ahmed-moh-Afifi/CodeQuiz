import { Component, signal, computed, inject, OnInit, OnDestroy } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { Subscription } from 'rxjs';
import { DecimalPipe } from '@angular/common';
import {
  CqButton,
  CqBadge,
  CqTextarea,
  CqSkeleton,
  CqDialogService,
  CqProgressBar,
  CqQuestionNav,
  CqIcon,
  CqCodeEditor,
} from '../../../design-system';
import type { QuestionNavItem } from '../../../design-system';
import { AttemptsService } from '../../services/attempts.service';
import { QuizzesService } from '../../services/quizzes.service';
import { ExecutionService } from '../../services/execution.service';
import { AuthStore } from '../../api/auth.store';
import type {
  ExaminerAttempt,
  SolutionDTO,
  SolutionGradeUpdate,
} from '../../api/models/attempt.models';
import type { ExaminerQuiz, QuestionDTO } from '../../api/models/quiz.models';
import type { RunCodeRequest } from '../../api/models/execution.models';

@Component({
  selector: 'app-grade-attempt',
  standalone: true,
  imports: [
    CqButton,
    CqBadge,
    CqTextarea,
    CqSkeleton,
    FormsModule,
    DecimalPipe,
    CqProgressBar,
    CqQuestionNav,
    CqIcon,
    CqCodeEditor,
  ],
  templateUrl: './grade-attempt.html',
  styleUrl: './grade-attempt.scss',
})
export class GradeAttempt implements OnInit, OnDestroy {
  private readonly attemptsService = inject(AttemptsService);
  private readonly quizzesService = inject(QuizzesService);
  private readonly executionService = inject(ExecutionService);
  private readonly authStore = inject(AuthStore);
  private readonly dialogService = inject(CqDialogService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private subs: Subscription[] = [];

  loading = signal(true);
  saving = signal(false);

  attempt = signal<ExaminerAttempt | null>(null);
  quiz = signal<ExaminerQuiz | null>(null);
  selectedQuestion = signal<QuestionDTO | null>(null);

  // Code display
  codeInEditor = signal('');
  output = signal('');
  input = signal('');
  isRunningCode = signal(false);

  // Grading
  grade = signal<string>('');
  feedback = signal<string>('');
  hasUnsavedChanges = signal(false);

  // AI Assessment
  evaluationStatus = signal<string>('');
  isRerunningAi = signal(false);

  // Track original + current grades per solution
  private originalGrades = new Map<number, number | undefined>();
  private originalFeedback = new Map<number, string | undefined>();
  private currentGrades = new Map<number, string>();
  private currentFeedbackMap = new Map<number, string>();

  questions = computed(() => this.quiz()?.questions ?? []);

  navItems = computed<QuestionNavItem[]>(() => {
    return this.questions().map((q) => ({
      id: q.id,
      order: q.order,
      graded: this.isSolutionGraded(q.id),
    }));
  });

  currentSolution = computed(() => {
    const att = this.attempt();
    const sel = this.selectedQuestion();
    if (!att || !sel) return null;
    return att.solutions.find((s) => s.questionId === sel.id) ?? null;
  });

  currentAiAssessment = computed(() => this.currentSolution()?.aiAssessment ?? null);

  isSolutionGraded(questionId: number): boolean {
    const att = this.attempt();
    if (!att) return false;
    const sol = att.solutions.find((s) => s.questionId === questionId);
    if (!sol) return false;
    // Check tracking map first, then original
    const tracked = this.currentGrades.get(sol.id);
    if (tracked != null && tracked !== '') return true;
    return sol.receivedGrade != null;
  }

  async ngOnInit() {
    const attemptId = parseInt(this.route.snapshot.queryParams['attemptId'], 10);
    const quizId = parseInt(this.route.snapshot.queryParams['quizId'], 10);

    if (!attemptId || !quizId) {
      this.router.navigate(['/app']);
      return;
    }

    this.loading.set(true);
    try {
      const [attempt, allQuizzes] = await Promise.all([
        this.attemptsService.getAttempt(attemptId),
        this.quizzesService.getMyQuizzes(),
      ]);

      this.attempt.set(attempt);

      const quiz = allQuizzes.find((q) => q.id === quizId);
      if (quiz) {
        this.quiz.set(quiz);
        this.initializeTracking(attempt);

        // Select first question
        const first = quiz.questions.find((q) => q.order === 1);
        if (first) this.selectQuestion(first, false);
      }

      // SignalR subscriptions
      this.subs.push(
        this.attemptsService.examinerAttemptChanges$.subscribe((change) => {
          if (change.attempt.id === attemptId && change.action === 'updated') {
            this.attempt.set(change.attempt);
          }
        }),
        this.attemptsService.evaluationStatus$.subscribe((evt) => {
          if (evt.attemptId !== attemptId) return;
          switch (evt.type) {
            case 'started':
              this.evaluationStatus.set('Evaluating...');
              break;
            case 'system-grading-complete':
              this.evaluationStatus.set('System grading complete. AI assessment in progress...');
              if (evt.attempt) this.attempt.set(evt.attempt);
              break;
            case 'ai-assessment-complete':
              this.evaluationStatus.set('AI assessment complete');
              if (evt.attempt) this.attempt.set(evt.attempt);
              setTimeout(() => {
                if (this.evaluationStatus() === 'AI assessment complete')
                  this.evaluationStatus.set('');
              }, 3000);
              break;
            case 'failed':
              this.evaluationStatus.set(`Evaluation failed: ${evt.message}`);
              break;
          }
        }),
      );
    } finally {
      this.loading.set(false);
    }
  }

  ngOnDestroy() {
    this.subs.forEach((s) => s.unsubscribe());
  }

  private initializeTracking(attempt: ExaminerAttempt) {
    for (const sol of attempt.solutions) {
      this.originalGrades.set(sol.id, sol.receivedGrade);
      this.originalFeedback.set(sol.id, sol.feedback);
    }
  }

  // --- Question navigation ---
  selectQuestion(question: QuestionDTO, saveFirst = true) {
    // Save current tracking
    if (saveFirst) this.saveCurrentToTracking();

    this.selectedQuestion.set(question);
    const att = this.attempt();
    if (!att) return;

    const sol = att.solutions.find((s) => s.questionId === question.id);
    this.codeInEditor.set(sol?.code ?? '');

    // Restore from tracking or from solution
    const tracked = sol ? this.currentGrades.get(sol.id) : undefined;
    this.grade.set(tracked ?? sol?.receivedGrade?.toString() ?? '');

    const trackedFb = sol ? this.currentFeedbackMap.get(sol.id) : undefined;
    this.feedback.set(trackedFb ?? sol?.feedback ?? '');

    this.output.set('');
  }

  private saveCurrentToTracking() {
    const sol = this.currentSolution();
    if (sol) {
      this.currentGrades.set(sol.id, this.grade());
      this.currentFeedbackMap.set(sol.id, this.feedback());
      this.checkUnsavedChanges();
    }
  }

  private checkUnsavedChanges() {
    for (const sol of this.attempt()?.solutions ?? []) {
      const curr = this.currentGrades.get(sol.id) ?? sol.receivedGrade?.toString() ?? '';
      const orig = this.originalGrades.get(sol.id)?.toString() ?? '';
      if (curr !== orig) {
        this.hasUnsavedChanges.set(true);
        return;
      }
      const currFb = this.currentFeedbackMap.get(sol.id) ?? sol.feedback ?? '';
      const origFb = this.originalFeedback.get(sol.id) ?? '';
      if (currFb !== origFb) {
        this.hasUnsavedChanges.set(true);
        return;
      }
    }
    this.hasUnsavedChanges.set(false);
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

  // --- Run code ---
  async onRunRequested(event: { code: string; input: string }) {
    if (this.isRunningCode()) return;
    const q = this.selectedQuestion();
    if (!q) return;

    this.isRunningCode.set(true);
    this.output.set('Running...');
    try {
      const config = q.questionConfiguration;
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

  // --- Run code (legacy) ---
  async runCode() {
    if (this.isRunningCode()) return;
    const q = this.selectedQuestion();
    if (!q) return;

    this.isRunningCode.set(true);
    this.output.set('Running...');
    try {
      const config = q.questionConfiguration;
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

  // --- AI re-assessment ---
  async rerunAi() {
    const sol = this.currentSolution();
    if (!sol) return;

    this.isRerunningAi.set(true);
    this.evaluationStatus.set('Queuing AI reassessment...');
    try {
      await this.attemptsService.aiReassessSolution(sol.id);
      this.evaluationStatus.set('AI reassessment in progress...');
    } catch {
      this.evaluationStatus.set('AI reassessment failed');
    } finally {
      this.isRerunningAi.set(false);
    }
  }

  // --- Save & return ---
  async saveAllGrades() {
    this.saveCurrentToTracking();

    const att = this.attempt();
    const quiz = this.quiz();
    if (!att || !quiz) return;

    const updates: SolutionGradeUpdate[] = [];
    for (const sol of att.solutions) {
      const currGrade = parseFloat(
        this.currentGrades.get(sol.id) ?? sol.receivedGrade?.toString() ?? '',
      );
      const origGrade = this.originalGrades.get(sol.id);
      const currFb = this.currentFeedbackMap.get(sol.id) ?? sol.feedback ?? '';
      const origFb = this.originalFeedback.get(sol.id) ?? '';

      const gradeChanged = !isNaN(currGrade) && currGrade !== origGrade;
      const fbChanged = currFb !== origFb;

      if (gradeChanged || fbChanged) {
        const question = quiz.questions.find((q) => q.id === sol.questionId);
        updates.push({
          solutionId: sol.id,
          questionNumber: question?.order ?? 0,
          totalPoints: question?.points ?? 0,
          receivedGrade: isNaN(currGrade) ? undefined : currGrade,
          oldGrade: origGrade,
          feedback: currFb || undefined,
          oldFeedback: origFb || undefined,
          evaluatedBy: this.authStore.user()?.firstName + ' ' + this.authStore.user()?.lastName,
        });
      }
    }

    if (updates.length > 0) {
      this.saving.set(true);
      try {
        await this.attemptsService.batchUpdateSolutionGrades({
          attemptId: att.id,
          updates,
        });
        this.hasUnsavedChanges.set(false);
      } finally {
        this.saving.set(false);
      }
    }

    this.router.navigate(['/app/quiz/view'], { queryParams: { quizId: quiz.id } });
  }

  goBack() {
    this.saveCurrentToTracking();
    this.checkUnsavedChanges();

    if (this.hasUnsavedChanges()) {
      const ref = this.dialogService.confirm(
        'warning',
        'Unsaved Changes',
        'You have unsaved grade changes. Discard them?',
        'Discard',
        'Cancel',
      );
      ref.afterClosed$.subscribe((confirmed) => {
        if (confirmed)
          this.router.navigate(['/app/quiz/view'], { queryParams: { quizId: this.quiz()!.id } });
      });
    } else {
      this.router.navigate(['/app/quiz/view'], { queryParams: { quizId: this.quiz()!.id } });
    }
  }

  onGradeChange(value: string) {
    this.grade.set(value);
    this.checkUnsavedChanges();
  }

  onFeedbackChange(value: string) {
    this.feedback.set(value);
    this.checkUnsavedChanges();
  }

  applySuggestedGrade() {
    const ai = this.currentAiAssessment();
    const q = this.selectedQuestion();
    if (ai?.suggestedGrade != null && q) {
      const actual = ai.suggestedGrade * q.points;
      this.grade.set(actual.toFixed(1));
      this.checkUnsavedChanges();
    }
  }

  onNavSelect(item: QuestionNavItem) {
    const q = this.questions().find((x) => x.id === item.id);
    if (q) this.selectQuestion(q);
  }

  formatTestInput(tc: { input: string[] }): string {
    return tc.input?.join('\n') ?? '';
  }
}
