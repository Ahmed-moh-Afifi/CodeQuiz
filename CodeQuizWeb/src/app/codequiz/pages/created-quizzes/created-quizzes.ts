import { Component, signal, inject, OnInit, OnDestroy } from '@angular/core';
import { Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { DatePipe, DecimalPipe } from '@angular/common';
import {
  CqButton,
  CqBadge,
  CqEmptyState,
  CqSkeleton,
  CqDialogService,
} from '../../../design-system';
import { QuizzesService } from '../../services/quizzes.service';
import type { ExaminerQuiz } from '../../api/models/quiz.models';

@Component({
  selector: 'app-created-quizzes',
  standalone: true,
  imports: [CqButton, CqBadge, CqEmptyState, CqSkeleton, DatePipe, DecimalPipe],
  templateUrl: './created-quizzes.html',
  styleUrl: './created-quizzes.scss',
})
export class CreatedQuizzes implements OnInit, OnDestroy {
  private readonly quizzesService = inject(QuizzesService);
  private readonly router = inject(Router);
  private readonly dialogService = inject(CqDialogService);
  private subs: Subscription[] = [];

  loading = signal(true);
  quizzes = signal<ExaminerQuiz[]>([]);

  async ngOnInit() {
    await this.loadQuizzes();
    this.subs.push(
      this.quizzesService.examinerQuizChanges$.subscribe((change) => {
        const current = [...this.quizzes()];
        switch (change.action) {
          case 'created':
            if (!current.find((q) => q.id === change.quiz.id)) {
              this.quizzes.set([change.quiz, ...current]);
            }
            break;
          case 'updated': {
            const idx = current.findIndex((q) => q.id === change.quiz.id);
            if (idx >= 0) {
              current[idx] = change.quiz;
              this.quizzes.set([...current]);
            }
            break;
          }
          case 'deleted':
            this.quizzes.set(current.filter((q) => q.id !== change.quiz.id));
            break;
        }
      }),
    );
  }

  ngOnDestroy() {
    this.subs.forEach((s) => s.unsubscribe());
  }

  private async loadQuizzes() {
    this.loading.set(true);
    try {
      const quizzes = await this.quizzesService.getMyQuizzes();
      this.quizzes.set(quizzes);
    } finally {
      this.loading.set(false);
    }
  }

  navigateCreate() {
    this.router.navigate(['/app/quiz/create']);
  }

  editQuiz(quiz: ExaminerQuiz) {
    this.router.navigate(['/app/quiz/create'], { queryParams: { quizId: quiz.id } });
  }

  viewQuiz(quiz: ExaminerQuiz) {
    this.router.navigate(['/app/quiz/view'], { queryParams: { quizId: quiz.id } });
  }

  async deleteQuiz(quiz: ExaminerQuiz) {
    const ref = this.dialogService.confirm(
      'danger',
      'Delete Quiz',
      `Are you sure you want to delete "${quiz.title}"? This action cannot be undone and will remove all associated attempts and grades.`,
      'Delete',
      'Cancel',
    );
    ref.afterClosed$.subscribe(async (confirmed) => {
      if (confirmed) {
        await this.quizzesService.deleteQuiz(quiz.id);
        this.quizzes.set(this.quizzes().filter((q) => q.id !== quiz.id));
      }
    });
  }

  copyCode(quiz: ExaminerQuiz) {
    navigator.clipboard.writeText(quiz.code);
  }

  getStatus(quiz: ExaminerQuiz): string {
    const now = new Date();
    const start = new Date(quiz.startDate);
    const end = new Date(quiz.endDate);
    if (now < start) return 'Upcoming';
    if (now > end) return 'Ended';
    return 'Running';
  }

  getStatusBadgeType(quiz: ExaminerQuiz): 'live' | 'success' | 'warning' {
    const status = this.getStatus(quiz);
    if (status === 'Running') return 'live';
    if (status === 'Ended') return 'success';
    return 'warning';
  }

  formatDuration(duration: string): string {
    // duration is HH:mm:ss
    const parts = duration.split(':');
    const h = parseInt(parts[0], 10);
    const m = parseInt(parts[1], 10);
    if (h > 0 && m > 0) return `${h}h ${m}m`;
    if (h > 0) return `${h}h`;
    return `${m}m`;
  }
}
