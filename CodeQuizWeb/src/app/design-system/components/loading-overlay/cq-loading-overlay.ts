import { Component, ChangeDetectorRef } from '@angular/core';

export interface LoadingState {
  visible: boolean;
  message: string;
}

@Component({
  selector: 'cq-loading-overlay',
  standalone: true,
  template: `
    @if (state.visible) {
      <div class="cq-loading-overlay" (click)="$event.stopPropagation()">
        <div class="cq-loading-content">
          <div class="cq-spinner cq-spinner-lg"></div>
          @if (state.message) {
            <p class="cq-loading-message">{{ state.message }}</p>
          }
        </div>
      </div>
    }
  `,
  styles: [
    `
      .cq-loading-overlay {
        position: fixed;
        inset: 0;
        z-index: 10100;
        display: flex;
        align-items: center;
        justify-content: center;
        background: rgba(0, 0, 0, 0.55);
        backdrop-filter: blur(4px);
        animation: cq-loading-fade-in 0.2s ease-out;
      }
      .cq-loading-content {
        display: flex;
        flex-direction: column;
        align-items: center;
        gap: 20px;
      }
      .cq-loading-message {
        color: var(--cq-text-primary, #ededed);
        font-size: 15px;
        font-weight: 500;
        text-align: center;
        margin: 0;
        max-width: 320px;
      }
      .cq-spinner-lg {
        width: 48px !important;
        height: 48px !important;
      }
      @keyframes cq-loading-fade-in {
        from {
          opacity: 0;
        }
        to {
          opacity: 1;
        }
      }
    `,
  ],
})
export class CqLoadingOverlay {
  state: LoadingState = { visible: false, message: '' };

  constructor(public cdr: ChangeDetectorRef) {}

  show(message: string) {
    this.state = { visible: true, message };
    this.cdr.detectChanges();
  }

  hide() {
    this.state = { visible: false, message: '' };
    this.cdr.detectChanges();
  }
}
