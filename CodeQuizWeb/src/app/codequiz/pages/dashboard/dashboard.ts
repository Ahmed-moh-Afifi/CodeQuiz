import { Component, signal, computed, OnInit, OnDestroy, inject } from '@angular/core';
import { Router } from '@angular/router';
import { DecimalPipe, DatePipe, UpperCasePipe } from '@angular/common';
import { Subscription } from 'rxjs';
import { CqBadge, CqButton, CqSkeleton, CqIcon } from '../../../design-system';
import { QuizzesService } from '../../services/quizzes.service';
import { AttemptsService } from '../../services/attempts.service';
import { AuthStore } from '../../api/auth.store';
import type { ExaminerQuiz } from '../../api/models/quiz.models';
import type { ExamineeAttempt } from '../../api/models/attempt.models';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CqBadge, CqButton, CqSkeleton, CqIcon, DecimalPipe, DatePipe, UpperCasePipe],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.scss',
})
export class Dashboard implements OnInit, OnDestroy {
  private readonly quizzesService = inject(QuizzesService);
  private readonly attemptsService = inject(AttemptsService);
  private readonly authStore = inject(AuthStore);
  private readonly router = inject(Router);
  private subs: Subscription[] = [];

  loading = signal(true);
  createdQuizzes = signal<ExaminerQuiz[]>([]);
  joinedAttempts = signal<ExamineeAttempt[]>([]);

  runningAttempts = computed(() => this.joinedAttempts().filter((a) => !a.endTime));

  completedAttempts = computed(() => this.joinedAttempts().filter((a) => !!a.endTime));

  hasRunningAttempts = computed(() => this.runningAttempts().length > 0);

  userName = computed(() => {
    const user = this.authStore.user();
    return user ? `${user.firstName}` : '';
  });

  statsJson = computed(() => {
    return {
      joined: this.joinedAttempts().length,
      created: this.createdQuizzes().length,
      active: this.runningAttempts().length,
    };
  });

  async ngOnInit() {
    await this.loadData();

    this.subs.push(
      this.quizzesService.examinerQuizChanges$.subscribe(() => this.loadCreatedQuizzes()),
      this.attemptsService.examineeAttemptChanges$.subscribe(() => this.loadJoinedAttempts()),
    );
  }

  ngOnDestroy() {
    this.subs.forEach((s) => s.unsubscribe());
  }

  private async loadData() {
    this.loading.set(true);
    try {
      await Promise.all([this.loadCreatedQuizzes(), this.loadJoinedAttempts()]);
    } finally {
      this.loading.set(false);
    }
  }

  private async loadCreatedQuizzes() {
    try {
      const quizzes = await this.quizzesService.getMyQuizzes();
      this.createdQuizzes.set(quizzes);
    } catch {
      // Error handled globally
    }
  }

  private async loadJoinedAttempts() {
    try {
      const attempts = await this.attemptsService.getMyAttempts();
      this.joinedAttempts.set(attempts);
    } catch {
      // Error handled globally
    }
  }

  continueAttempt(attempt: ExamineeAttempt) {
    this.router.navigate(['/app/quiz/take'], {
      queryParams: { attemptId: attempt.id, quizCode: attempt.quiz.code },
    });
  }

  viewResults(attempt: ExamineeAttempt) {
    this.router.navigate(['/app/quiz/review'], {
      queryParams: { attemptId: attempt.id },
    });
  }

  viewCreatedQuiz(quiz: ExaminerQuiz) {
    this.router.navigate(['/app/quiz/view'], {
      queryParams: { quizId: quiz.id },
    });
  }

  deleteCreatedQuiz(quiz: ExaminerQuiz) {
    // Confirmation is handled at the quiz service level
    this.quizzesService.deleteQuiz(quiz.id).then(() => this.loadCreatedQuizzes());
  }

  navigateCreateQuiz() {
    this.router.navigate(['/app/quiz/create']);
  }

  navigateJoinQuiz() {
    this.router.navigate(['/app/joined']);
  }

  copyQuizCode(quiz: ExaminerQuiz) {
    navigator.clipboard.writeText(quiz.code);
  }

  getQuizStatus(quiz: ExaminerQuiz): string {
    const now = new Date();
    const start = new Date(quiz.startDate);
    const end = new Date(quiz.endDate);
    if (now < start) return 'Upcoming';
    if (now > end) return 'Ended';
    return 'Running';
  }

  getQuizStatusBadgeType(quiz: ExaminerQuiz): 'live' | 'success' | 'warning' {
    const status = this.getQuizStatus(quiz);
    if (status === 'Running') return 'live';
    if (status === 'Ended') return 'success';
    return 'warning';
  }

  getAttemptStatusBadgeType(attempt: ExamineeAttempt): 'live' | 'success' {
    return attempt.endTime ? 'success' : 'live';
  }

  formatScore(attempt: ExamineeAttempt): string {
    if (attempt.gradePercentage != null) {
      return `${Math.round(attempt.gradePercentage)}%`;
    }
    return '—';
  }
}
