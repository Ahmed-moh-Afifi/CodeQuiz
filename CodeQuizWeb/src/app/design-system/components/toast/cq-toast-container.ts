import { Component, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';

export interface ToastConfig {
  id: number;
  type: 'success' | 'error' | 'warning' | 'info';
  message: string;
  duration: number;
  action?: string;
  removing?: boolean;
}

const ICON_MAP: Record<string, string> = {
  success: 'assets/icons/success.svg',
  error: 'assets/icons/error.svg',
  warning: 'assets/icons/warning.svg',
  info: 'assets/icons/lightbulb.svg',
};

@Component({
  selector: 'cq-toast-container',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="cq-toast-container">
      @for (toast of toasts; track toast.id) {
        <div [class]="'cq-toast cq-toast-' + toast.type + (toast.removing ? ' cq-toast-exit' : '')">
          <div
            class="cq-mask-icon"
            [class]="'cq-mask-icon cq-mask-icon-' + toast.type"
            [style.--mask-url]="'url(' + getIcon(toast.type) + ')'"
            style="width: 20px; height: 20px; flex-shrink: 0"
          ></div>
          <span class="cq-toast-message">{{ toast.message }}</span>
          @if (toast.action) {
            <button class="cq-toast-action" (click)="dismiss(toast)">
              {{ toast.action }}
            </button>
          }
          <button class="cq-toast-close" (click)="dismiss(toast)">✕</button>
          <div class="cq-toast-timer" [style.animation-duration.ms]="toast.duration"></div>
        </div>
      }
    </div>
  `,
})
export class CqToastContainer {
  toasts: ToastConfig[] = [];

  getIcon(type: string): string {
    return ICON_MAP[type] || ICON_MAP['info'];
  }

  addToast(toast: ToastConfig) {
    this.toasts.push(toast);

    if (toast.duration > 0) {
      setTimeout(() => this.dismiss(toast), toast.duration);
    }
  }

  dismiss(toast: ToastConfig) {
    const t = this.toasts.find((x) => x.id === toast.id);
    if (t && !t.removing) {
      t.removing = true;
      setTimeout(() => {
        this.toasts = this.toasts.filter((x) => x.id !== toast.id);
      }, 300); // match exit animation duration
    }
  }
}
