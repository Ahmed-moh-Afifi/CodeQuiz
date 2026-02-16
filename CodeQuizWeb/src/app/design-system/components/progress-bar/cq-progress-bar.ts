import { Component, input, computed } from '@angular/core';

export type ProgressBarType = 'primary' | 'success' | 'warning' | 'error';

@Component({
  selector: 'cq-progress-bar',
  standalone: true,
  template: `
    <div [class]="progressClass()">
      <div
        class="cq-progress-bar"
        [style.width.%]="clampedValue()"
        [class.cq-progress-bar-animated]="animated()"
      ></div>
    </div>
    @if (showLabel()) {
      <span class="cq-progress-label">{{ clampedValue() }}%</span>
    }
  `,
  styles: [
    `
      :host {
        display: flex;
        align-items: center;
        gap: 12px;
      }
      .cq-progress,
      :host > div {
        flex: 1;
      }
      .cq-progress-label {
        font-size: 13px;
        font-weight: 600;
        color: var(--cq-text-secondary);
        white-space: nowrap;
      }
      .cq-progress-bar-animated {
        background-image: linear-gradient(
          -45deg,
          rgba(255, 255, 255, 0.15) 25%,
          transparent 25%,
          transparent 50%,
          rgba(255, 255, 255, 0.15) 50%,
          rgba(255, 255, 255, 0.15) 75%,
          transparent 75%,
          transparent
        );
        background-size: 20px 20px;
        animation: cq-progress-stripe 0.8s linear infinite;
      }

      @keyframes cq-progress-stripe {
        from {
          background-position: 20px 0;
        }
        to {
          background-position: 0 0;
        }
      }
    `,
  ],
})
export class CqProgressBar {
  value = input<number>(0);
  type = input<ProgressBarType>('primary');
  showLabel = input<boolean>(false);
  animated = input<boolean>(false);

  clampedValue = computed(() => Math.max(0, Math.min(100, this.value())));

  progressClass = computed(() => {
    const t = this.type();
    let cls = 'cq-progress';
    if (t !== 'primary') cls += ` cq-progress-${t}`;
    return cls;
  });
}
