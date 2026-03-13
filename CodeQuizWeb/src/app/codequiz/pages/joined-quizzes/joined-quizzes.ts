import { Component, signal, inject, OnInit, OnDestroy } from '@angular/core';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { Subscription } from 'rxjs';
import { DatePipe, DecimalPipe } from '@angular/common';
import { CqButton, CqBadge, CqInput, CqEmptyState, CqSkeleton } from '../../../design-system';
import { AttemptsService } from '../../services/attempts.service';
import type { ExamineeAttempt } from '../../api/models/attempt.models';

@Component({
  selector: 'app-joined-quizzes',
  standalone: true,
  imports: [
    CqButton,
    CqBadge,
    CqInput,
    CqEmptyState,
    CqSkeleton,
    FormsModule,
    DatePipe,
    DecimalPipe,
  ],
  templateUrl: './joined-quizzes.html',
  styleUrl: './joined-quizzes.scss',
})
export class JoinedQuizzes implements OnInit, OnDestroy {
  private readonly attemptsService = inject(AttemptsService);
  private readonly router = inject(Router);
  private subs: Subscription[] = [];

  loading = signal(true);
  attempts = signal<ExamineeAttempt[]>([]);
  quizCode = '';
  joining = signal(false);

  async ngOnInit() {
    await this.loadAttempts();
    this.subs.push(
      this.attemptsService.examineeAttemptChanges$.subscribe((change) => {
        const current = [...this.attempts()];
        switch (change.action) {
          case 'created':
            if (!current.find((a) => a.id === change.attempt.id)) {
              this.attempts.set([change.attempt as ExamineeAttempt, ...current]);
            }
            break;
          case 'updated': {
            const idx = current.findIndex((a) => a.id === change.attempt.id);
            if (idx >= 0) {
              current[idx] = change.attempt as ExamineeAttempt;
              this.attempts.set([...current]);
            }
            break;
          }
          case 'deleted':
            this.attempts.set(current.filter((a) => a.id !== change.attempt.id));
            break;
        }
      }),
    );
  }

  ngOnDestroy() {
    this.subs.forEach((s) => s.unsubscribe());
  }

  private async loadAttempts() {
    this.loading.set(true);
    try {
      const attempts = await this.attemptsService.getMyAttempts();
      this.attempts.set(attempts);
    } finally {
      this.loading.set(false);
    }
  }

  async joinQuiz() {
    if (!this.quizCode.trim()) return;
    this.joining.set(true);
    try {
      const attempt = await this.attemptsService.beginAttempt({ quizCode: this.quizCode.trim() });
      this.router.navigate(['/app/quiz/take'], {
        queryParams: { attemptId: attempt.id, quizCode: this.quizCode.trim() },
      });
    } finally {
      this.joining.set(false);
    }
  }

  async continueAttempt(attempt: ExamineeAttempt) {
    const result = await this.attemptsService.beginAttempt({ quizCode: attempt.quiz.code });
    this.router.navigate(['/app/quiz/take'], {
      queryParams: { attemptId: result.id, quizCode: attempt.quiz.code },
    });
  }

  reviewAttempt(attempt: ExamineeAttempt) {
    this.router.navigate(['/app/quiz/review'], { queryParams: { attemptId: attempt.id } });
  }

  getStatusText(attempt: ExamineeAttempt): string {
    return attempt.endTime ? 'Completed' : 'Running';
  }

  getStatusBadgeType(attempt: ExamineeAttempt): 'live' | 'success' {
    return attempt.endTime ? 'success' : 'live';
  }

  formatDuration(attempt: ExamineeAttempt): string {
    if (!attempt.startTime) return '—';
    const start = new Date(attempt.startTime);
    const end = attempt.endTime ? new Date(attempt.endTime) : new Date();
    const diffMs = end.getTime() - start.getTime();
    const mins = Math.floor(diffMs / 60000);
    const hrs = Math.floor(mins / 60);
    const remainMins = mins % 60;
    if (hrs > 0) return `${hrs}h ${remainMins}m`;
    return `${remainMins}m`;
  }

  formatScore(attempt: ExamineeAttempt): string {
    if (attempt.gradePercentage != null) return `${Math.round(attempt.gradePercentage)}%`;
    return '—';
  }
}
