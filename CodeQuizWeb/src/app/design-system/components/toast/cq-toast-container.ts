import { Component, ChangeDetectorRef } from '@angular/core';

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
  template: `
    <div class="cq-toast-container">
      @for (toast of toasts; track toast.id) {
        <div
          class="cq-toast"
          [class.cq-toast-success]="toast.type === 'success'"
          [class.cq-toast-error]="toast.type === 'error'"
          [class.cq-toast-warning]="toast.type === 'warning'"
          [class.cq-toast-info]="toast.type === 'info'"
          [class.cq-toast-exit]="toast.removing"
        >
          <div
            class="cq-mask-icon"
            [class.cq-mask-icon-success]="toast.type === 'success'"
            [class.cq-mask-icon-error]="toast.type === 'error'"
            [class.cq-mask-icon-warning]="toast.type === 'warning'"
            [class.cq-mask-icon-info]="toast.type === 'info'"
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

  constructor(private cdr: ChangeDetectorRef) {}

  getIcon(type: string): string {
    return ICON_MAP[type] || ICON_MAP['info'];
  }

  addToast(toast: ToastConfig) {
    this.toasts = [...this.toasts, toast];
    this.cdr.detectChanges();

    if (toast.duration > 0) {
      setTimeout(() => this.dismiss(toast), toast.duration);
    }
  }

  dismiss(toast: ToastConfig) {
    const t = this.toasts.find((x) => x.id === toast.id);
    if (t && !t.removing) {
      t.removing = true;
      this.cdr.detectChanges();

      setTimeout(() => {
        this.toasts = this.toasts.filter((x) => x.id !== toast.id);
        this.cdr.detectChanges();
      }, 300);
    }
  }
}
