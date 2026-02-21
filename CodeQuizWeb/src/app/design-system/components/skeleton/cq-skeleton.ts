import { Component, Input } from '@angular/core';

@Component({
  selector: 'cq-skeleton',
  standalone: true,
  template: `
    <div
      class="cq-skeleton"
      [class.cq-skeleton-text]="variant === 'text'"
      [class.cq-skeleton-circular]="variant === 'circular'"
      [class.cq-skeleton-rectangular]="variant === 'rectangular'"
      [style.width]="width"
      [style.height]="height"
    ></div>
  `,
  styles: [
    `
      .cq-skeleton {
        background: linear-gradient(
          90deg,
          var(--cq-secondary, #2a2a2a) 25%,
          var(--cq-border-subtle, #3b3b3b) 50%,
          var(--cq-secondary, #2a2a2a) 75%
        );
        background-size: 200% 100%;
        animation: cq-skeleton-shimmer 1.5s ease-in-out infinite;
      }
      .cq-skeleton-text {
        height: 14px;
        border-radius: 4px;
        width: 100%;
      }
      .cq-skeleton-circular {
        width: 40px;
        height: 40px;
        border-radius: 50%;
      }
      .cq-skeleton-rectangular {
        width: 100%;
        height: 120px;
        border-radius: var(--cq-radius-md, 8px);
      }
      @keyframes cq-skeleton-shimmer {
        0% {
          background-position: -200% 0;
        }
        100% {
          background-position: 200% 0;
        }
      }
    `,
  ],
})
export class CqSkeleton {
  @Input() variant: 'text' | 'circular' | 'rectangular' = 'text';
  @Input() width: string = '';
  @Input() height: string = '';
}
