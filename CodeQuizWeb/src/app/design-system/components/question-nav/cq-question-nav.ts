import { Component, input, output, computed } from '@angular/core';
import { CqProgressBar } from '../progress-bar/cq-progress-bar';
import { CqButton } from '../buttons/cq-button/cq-button';

export interface QuestionNavItem {
  id: number;
  order: number;
  answered?: boolean;
  graded?: boolean;
}

@Component({
  selector: 'cq-question-nav',
  standalone: true,
  imports: [CqProgressBar, CqButton],
  template: `
    <div class="cq-question-nav">
      <!-- Questions Grid -->
      <div class="cq-question-nav-section">
        <span class="cq-question-nav-label">Questions</span>
        <div class="cq-question-nav-grid">
          @for (q of questions(); track q.id) {
            <button
              class="cq-question-nav-btn"
              [class.cq-question-nav-btn-active]="q.id === selectedId()"
              [class.cq-question-nav-btn-answered]="q.answered && q.id !== selectedId()"
              [class.cq-question-nav-btn-graded]="q.graded && q.id !== selectedId()"
              (click)="questionSelected.emit(q)"
            >
              {{ q.order }}
            </button>
          }
        </div>
      </div>

      <!-- Progress -->
      @if (showProgress()) {
        <div class="cq-question-nav-progress">
          <div class="cq-question-nav-progress-header">
            <span class="cq-question-nav-caption">Progress</span>
            <span class="cq-question-nav-caption"
              >{{ selectedOrder() }} / {{ questions().length }}</span
            >
          </div>
          <cq-progress-bar [value]="progressValue()" />
        </div>
      }

      <!-- Extra content (e.g. save status) -->
      <div class="cq-question-nav-extra">
        <ng-content></ng-content>
      </div>

      <!-- Navigation Buttons -->
      <div class="cq-question-nav-buttons">
        <cq-button type="secondary" [disabled]="!canGoPrevious()" (clicked)="previous.emit()">
          ← Previous
        </cq-button>
        <cq-button type="primary" [disabled]="!canGoNext()" (clicked)="next.emit()">
          Next →
        </cq-button>
      </div>
    </div>
  `,
  styles: [
    `
      :host {
        display: flex;
        flex-direction: column;
        height: 100%;
      }

      .cq-question-nav {
        display: flex;
        flex-direction: column;
        height: 100%;
        padding: 24px;
        gap: 20px;
      }

      .cq-question-nav-section {
        display: flex;
        flex-direction: column;
        gap: 12px;
        flex: 1;
      }

      .cq-question-nav-label {
        font-size: 16px;
        font-weight: 700;
        color: var(--cq-text-primary);
      }

      .cq-question-nav-grid {
        display: flex;
        flex-wrap: wrap;
        gap: 6px;
        align-items: flex-start;
        align-content: flex-start;
      }

      .cq-question-nav-btn {
        display: flex;
        align-items: center;
        justify-content: center;
        width: 48px;
        height: 48px;
        border: 1px solid var(--cq-border-default);
        border-radius: 8px;
        background: var(--cq-surface-dark);
        color: var(--cq-text-primary);
        font-size: 16px;
        font-weight: 700;
        cursor: pointer;
        transition: all 0.15s ease;
      }

      .cq-question-nav-btn:hover {
        background: var(--cq-primary-hover, rgba(var(--cq-primary-rgb), 0.15));
      }

      .cq-question-nav-btn-active {
        background: var(--cq-primary) !important;
        border-color: var(--cq-primary) !important;
        color: var(--cq-text-on-primary, #fff) !important;
      }

      .cq-question-nav-btn-answered {
        background: rgba(var(--cq-success-rgb, 31, 122, 77), 0.15);
        border-color: var(--cq-success);
        color: var(--cq-success);
      }

      .cq-question-nav-btn-graded {
        background: rgba(var(--cq-success-rgb, 31, 122, 77), 0.15);
        border-color: var(--cq-success);
        color: var(--cq-success);
      }

      .cq-question-nav-progress {
        display: flex;
        flex-direction: column;
        gap: 8px;
      }

      .cq-question-nav-progress-header {
        display: flex;
        justify-content: space-between;
        align-items: center;
      }

      .cq-question-nav-caption {
        font-size: 13px;
        color: var(--cq-text-muted);
      }

      .cq-question-nav-extra:empty {
        display: none;
      }

      .cq-question-nav-buttons {
        display: grid;
        grid-template-columns: 1fr 1fr;
        gap: 12px;
        margin-top: auto;
        padding-top: 16px;
      }
    `,
  ],
})
export class CqQuestionNav {
  questions = input.required<QuestionNavItem[]>();
  selectedId = input<number>(0);
  showProgress = input<boolean>(true);

  questionSelected = output<QuestionNavItem>();
  previous = output<void>();
  next = output<void>();

  selectedOrder = computed(() => {
    const q = this.questions().find((q) => q.id === this.selectedId());
    return q ? q.order : 1;
  });

  progressValue = computed(() => {
    const total = this.questions().length;
    if (total === 0) return 0;
    return Math.round((this.selectedOrder() / total) * 100);
  });

  canGoPrevious = computed(() => this.selectedOrder() > 1);
  canGoNext = computed(() => this.selectedOrder() < this.questions().length);
}
