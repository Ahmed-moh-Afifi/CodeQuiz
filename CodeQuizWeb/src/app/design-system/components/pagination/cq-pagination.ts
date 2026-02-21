import { Component, Input, Output, EventEmitter, computed, signal } from '@angular/core';

@Component({
  selector: 'cq-pagination',
  standalone: true,
  template: `
    <nav class="cq-pagination" aria-label="Pagination">
      <button
        class="cq-pagination-btn"
        [disabled]="currentPage <= 1"
        (click)="goTo(currentPage - 1)"
      >
        ‹ Prev
      </button>
      @for (page of visiblePages(); track page) {
        @if (page === -1) {
          <span class="cq-pagination-ellipsis">…</span>
        } @else {
          <button
            class="cq-pagination-btn"
            [class.active]="page === currentPage"
            (click)="goTo(page)"
          >
            {{ page }}
          </button>
        }
      }
      <button
        class="cq-pagination-btn"
        [disabled]="currentPage >= totalPagesCount()"
        (click)="goTo(currentPage + 1)"
      >
        Next ›
      </button>
    </nav>
  `,
  styles: [
    `
      .cq-pagination {
        display: flex;
        align-items: center;
        gap: 4px;
        flex-wrap: wrap;
      }
      .cq-pagination-btn {
        display: inline-flex;
        align-items: center;
        justify-content: center;
        min-width: 36px;
        height: 36px;
        padding: 0 10px;
        border: 1px solid var(--cq-border-default);
        border-radius: var(--cq-radius-md, 8px);
        background: transparent;
        color: var(--cq-text-secondary);
        font-size: 13px;
        cursor: pointer;
        transition: all 0.15s;
      }
      .cq-pagination-btn:hover:not(:disabled):not(.active) {
        background: var(--cq-secondary-hover);
        color: var(--cq-text-primary);
      }
      .cq-pagination-btn.active {
        background: var(--cq-primary);
        color: var(--cq-text-on-primary);
        border-color: var(--cq-primary);
        font-weight: 600;
      }
      .cq-pagination-btn:disabled {
        opacity: 0.4;
        cursor: not-allowed;
      }
      .cq-pagination-ellipsis {
        display: inline-flex;
        align-items: center;
        justify-content: center;
        width: 36px;
        height: 36px;
        color: var(--cq-text-muted);
        font-size: 14px;
      }
    `,
  ],
})
export class CqPagination {
  @Input() totalItems = 0;
  @Input() pageSize = 10;
  @Input() currentPage = 1;
  @Output() pageChange = new EventEmitter<number>();

  totalPagesCount = computed(() => {
    return Math.max(1, Math.ceil(this.totalItems / this.pageSize));
  });

  visiblePages = computed(() => {
    const total = this.totalPagesCount();
    const current = this.currentPage;
    const pages: number[] = [];

    if (total <= 7) {
      for (let i = 1; i <= total; i++) pages.push(i);
      return pages;
    }

    // Always show first page
    pages.push(1);

    if (current > 3) pages.push(-1); // ellipsis

    const start = Math.max(2, current - 1);
    const end = Math.min(total - 1, current + 1);

    for (let i = start; i <= end; i++) pages.push(i);

    if (current < total - 2) pages.push(-1); // ellipsis

    // Always show last page
    pages.push(total);

    return pages;
  });

  goTo(page: number) {
    const total = this.totalPagesCount();
    if (page >= 1 && page <= total && page !== this.currentPage) {
      this.currentPage = page;
      this.pageChange.emit(page);
    }
  }
}
