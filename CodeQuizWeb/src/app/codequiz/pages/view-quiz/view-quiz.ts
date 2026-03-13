import { Component, signal, computed, inject, OnInit, OnDestroy } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { Subscription } from 'rxjs';
import { DatePipe, DecimalPipe } from '@angular/common';
import { CqButton, CqBadge, CqEmptyState, CqSkeleton, CqProgressBar } from '../../../design-system';
import { QuizzesService } from '../../services/quizzes.service';
import { AttemptsService } from '../../services/attempts.service';
import type { ExaminerQuiz } from '../../api/models/quiz.models';
import type { ExaminerAttempt } from '../../api/models/attempt.models';

@Component({
  selector: 'app-view-quiz',
  standalone: true,
  imports: [CqButton, CqBadge, CqEmptyState, CqSkeleton, CqProgressBar, DatePipe, DecimalPipe],
  templateUrl: './view-quiz.html',
  styleUrl: './view-quiz.scss',
})
export class ViewQuiz implements OnInit, OnDestroy {
  private readonly quizzesService = inject(QuizzesService);
  private readonly attemptsService = inject(AttemptsService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private subs: Subscription[] = [];

  loading = signal(true);
  quiz = signal<ExaminerQuiz | null>(null);
  attempts = signal<ExaminerAttempt[]>([]);
  showStats = signal(true);
  activeFilter = signal('');

  // Statistics
  totalAttempts = computed(() => this.attempts().length);
  submittedAttempts = computed(() => this.attempts().filter((a) => a.endTime).length);
  averageScore = computed(() => {
    const submitted = this.attempts().filter((a) => a.gradePercentage != null);
    if (submitted.length === 0) return 0;
    return submitted.reduce((sum, a) => sum + (a.gradePercentage ?? 0), 0) / submitted.length;
  });
  highestScore = computed(() => {
    const submitted = this.attempts().filter((a) => a.gradePercentage != null);
    if (submitted.length === 0) return 0;
    return Math.max(...submitted.map((a) => a.gradePercentage ?? 0));
  });
  lowestScore = computed(() => {
    const submitted = this.attempts().filter((a) => a.gradePercentage != null);
    if (submitted.length === 0) return 0;
    return Math.min(...submitted.map((a) => a.gradePercentage ?? 0));
  });
  medianScore = computed(() => {
    const sorted = this.attempts()
      .filter((a) => a.gradePercentage != null)
      .map((a) => a.gradePercentage!)
      .sort((a, b) => a - b);
    if (sorted.length === 0) return 0;
    const mid = Math.floor(sorted.length / 2);
    return sorted.length % 2 ? sorted[mid] : (sorted[mid - 1] + sorted[mid]) / 2;
  });
  passRate = computed(() => {
    const submitted = this.attempts().filter((a) => a.gradePercentage != null);
    if (submitted.length === 0) return 0;
    const passing = submitted.filter((a) => (a.gradePercentage ?? 0) >= 50).length;
    return (passing / submitted.length) * 100;
  });
  aiAssessedCount = computed(
    () => this.attempts().filter((a) => a.solutions?.some((s) => s.aiAssessment)).length,
  );
  flaggedCount = computed(
    () =>
      this.attempts().filter((a) =>
        a.solutions?.some((s) => s.aiAssessment && !s.aiAssessment.isValid),
      ).length,
  );

  filteredAttempts = computed(() => {
    const filter = this.activeFilter();
    const all = this.attempts();
    if (!filter) return all;
    if (filter === 'AI Assessed')
      return all.filter((a) => a.solutions?.some((s) => s.aiAssessment));
    if (filter === 'Flagged')
      return all.filter((a) => a.solutions?.some((s) => s.aiAssessment && !s.aiAssessment.isValid));
    return all;
  });

  async ngOnInit() {
    const quizId = parseInt(this.route.snapshot.queryParams['quizId'], 10);
    if (!quizId) {
      this.router.navigate(['/app']);
      return;
    }

    this.loading.set(true);
    try {
      // Load quiz info from user's quizzes
      const quizzes = await this.quizzesService.getMyQuizzes();
      const quiz = quizzes.find((q) => q.id === quizId);
      if (!quiz) {
        this.router.navigate(['/app']);
        return;
      }
      this.quiz.set(quiz);

      // Load attempts
      const attempts = await this.quizzesService.getQuizAttempts(quizId);
      this.attempts.set(attempts);

      // Join SignalR group for real-time updates
      await this.attemptsService.ensureConnected();
      await this.attemptsService.joinQuizGroup(quizId);

      this.subs.push(
        this.attemptsService.examinerAttemptChanges$.subscribe((change) => {
          const current = [...this.attempts()];
          if (change.attempt.quizId !== quizId) return;

          switch (change.action) {
            case 'created':
              if (!current.find((a) => a.id === change.attempt.id)) {
                this.attempts.set([change.attempt, ...current]);
              }
              break;
            case 'updated': {
              const idx = current.findIndex((a) => a.id === change.attempt.id);
              if (idx >= 0) {
                current[idx] = change.attempt;
                this.attempts.set([...current]);
              }
              break;
            }
          }
        }),
      );
    } finally {
      this.loading.set(false);
    }
  }

  ngOnDestroy() {
    this.subs.forEach((s) => s.unsubscribe());
    const quizId = this.quiz()?.id;
    if (quizId) this.attemptsService.leaveQuizGroup(quizId);
  }

  goBack() {
    this.router.navigate(['/app']);
  }

  toggleStatsPanel() {
    this.showStats.update((v) => !v);
  }

  filterByAiAssessed() {
    this.activeFilter.set(this.activeFilter() === 'AI Assessed' ? '' : 'AI Assessed');
  }

  filterByFlagged() {
    this.activeFilter.set(this.activeFilter() === 'Flagged' ? '' : 'Flagged');
  }

  clearFilter() {
    this.activeFilter.set('');
  }

  gradeAttempt(attempt: ExaminerAttempt) {
    this.router.navigate(['/app/quiz/grade'], {
      queryParams: { attemptId: attempt.id, quizId: this.quiz()!.id },
    });
  }

  getAttemptStatus(attempt: ExaminerAttempt): string {
    return attempt.endTime ? 'Submitted' : 'In Progress';
  }

  getAttemptBadgeType(attempt: ExaminerAttempt): 'success' | 'live' {
    return attempt.endTime ? 'success' : 'live';
  }

  isFullyGraded(attempt: ExaminerAttempt): boolean {
    if (!attempt.solutions?.length) return false;
    return attempt.solutions.every((s) => s.receivedGrade != null);
  }

  isPartiallyGraded(attempt: ExaminerAttempt): boolean {
    if (!attempt.solutions?.length) return false;
    const hasGraded = attempt.solutions.some((s) => s.receivedGrade != null);
    const hasUngraded = attempt.solutions.some((s) => s.receivedGrade == null);
    return hasGraded && hasUngraded;
  }

  hasAiFlags(attempt: ExaminerAttempt): boolean {
    return attempt.solutions?.some((s) => s.aiAssessment && !s.aiAssessment.isValid) ?? false;
  }

  hasAiAssessment(attempt: ExaminerAttempt): boolean {
    return attempt.solutions?.some((s) => !!s.aiAssessment) ?? false;
  }
}
