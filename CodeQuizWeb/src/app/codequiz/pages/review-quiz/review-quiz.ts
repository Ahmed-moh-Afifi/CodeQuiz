import { Component, signal, computed, inject, OnInit } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { DecimalPipe } from '@angular/common';
import {
  CqButton,
  CqBadge,
  CqSkeleton,
  CqProgressBar,
  CqCard,
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
  selector: 'app-review-quiz',
  standalone: true,
  imports: [
    CqButton,
    CqBadge,
    CqSkeleton,
    CqProgressBar,
    CqCard,
    CqQuestionNav,
    CqIcon,
    CqCodeEditor,
    FormsModule,
    DecimalPipe,
  ],
  templateUrl: './review-quiz.html',
  styleUrl: './review-quiz.scss',
})
export class ReviewQuiz implements OnInit {
  private readonly attemptsService = inject(AttemptsService);
  private readonly executionService = inject(ExecutionService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  loading = signal(true);
  attempt = signal<ExamineeAttempt | null>(null);
  selectedQuestion = signal<QuestionDTO | null>(null);

  // Code execution
  input = signal('');
  output = signal('');
  isRunningCode = signal(false);

  questions = computed(() => {
    const att = this.attempt();
    return att?.quiz?.questions ?? [];
  });

  navItems = computed<QuestionNavItem[]>(() => {
    return this.questions().map((q) => ({
      id: q.id,
      order: q.order,
    }));
  });

  currentSolution = computed((): SolutionDTO | null => {
    const att = this.attempt();
    const sel = this.selectedQuestion();
    if (!att || !sel) return null;
    return att.solutions.find((s) => s.questionId === sel.id) ?? null;
  });

  currentAiAssessment = computed(() => this.currentSolution()?.aiAssessment ?? null);

  showAiFeedback = computed(() => {
    const att = this.attempt();
    return att?.quiz?.showAiFeedbackToStudents ?? false;
  });

  gradePercentage = computed(() => {
    const att = this.attempt();
    return att?.gradePercentage ?? 0;
  });

  passedTestsText = computed(() => {
    const sol = this.currentSolution();
    if (!sol?.evaluationResults?.length) return '0 / 0';
    const passed = sol.evaluationResults.filter((r) => r.isSuccessful).length;
    return `${passed} / ${sol.evaluationResults.length}`;
  });

  async ngOnInit() {
    const attemptId = parseInt(this.route.snapshot.queryParams['attemptId'], 10);
    if (!attemptId) {
      this.router.navigate(['/app']);
      return;
    }

    this.loading.set(true);
    try {
      const attempts = await this.attemptsService.getMyAttempts();
      const attempt = attempts.find((a) => a.id === attemptId);
      if (attempt) {
        this.attempt.set(attempt);
        const first = attempt.quiz?.questions?.find((q) => q.order === 1);
        if (first) this.selectQuestion(first);
      }
    } finally {
      this.loading.set(false);
    }
  }

  selectQuestion(question: QuestionDTO) {
    this.selectedQuestion.set(question);
    this.output.set('');
  }

  onNavSelect(item: QuestionNavItem) {
    const q = this.questions().find((x) => x.id === item.id);
    if (q) this.selectQuestion(q);
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

  formatTestInput(tc: { input: string[] }): string {
    return tc.input?.join('\n') ?? '';
  }

  async onRunRequested(event: { code: string; input: string }) {
    if (this.isRunningCode()) return;
    const q = this.selectedQuestion();
    const sol = this.currentSolution();
    if (!q || !sol) return;

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

  async runCode() {
    if (this.isRunningCode()) return;
    const q = this.selectedQuestion();
    const sol = this.currentSolution();
    if (!q || !sol) return;

    this.isRunningCode.set(true);
    this.output.set('Running...');
    try {
      const config = q.questionConfiguration;
      const request: RunCodeRequest = {
        language: config.language,
        code: sol.code,
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

  goBack() {
    this.router.navigate(['/app/joined']);
  }
}
