import { Component, ViewChild, ViewContainerRef, NgZone } from '@angular/core';
import { CqDialogRef } from '../services/cq-dialog-ref';
import { CqButton } from '../components/buttons/cq-button/cq-button';

/**
 * Types supported by the alert/confirm shorthand methods.
 */
export type AlertDialogType = 'question' | 'danger' | 'success' | 'warning' | 'info';

type ButtonType =
  | 'primary'
  | 'secondary'
  | 'outlined'
  | 'danger'
  | 'ghost'
  | 'success'
  | 'warning'
  | 'danger-filled';

export interface AlertDialogConfig {
  type: AlertDialogType;
  title: string;
  message: string;
  confirmText?: string;
  cancelText?: string;
}

const ICON_MAP: Record<AlertDialogType, { icon: string; iconClass: string; bgClass: string }> = {
  question: {
    icon: 'assets/icons/question.svg',
    iconClass: 'cq-mask-icon-primary',
    bgClass: 'cq-dialog-icon-question',
  },
  danger: {
    icon: 'assets/icons/question.svg',
    iconClass: 'cq-mask-icon-error',
    bgClass: 'cq-dialog-icon-danger',
  },
  success: {
    icon: 'assets/icons/success.svg',
    iconClass: 'cq-mask-icon-success',
    bgClass: 'cq-dialog-icon-success',
  },
  warning: {
    icon: 'assets/icons/warning.svg',
    iconClass: 'cq-mask-icon-warning',
    bgClass: 'cq-dialog-icon-warning',
  },
  info: {
    icon: 'assets/icons/lightbulb.svg',
    iconClass: 'cq-mask-icon-info',
    bgClass: 'cq-dialog-icon-info',
  },
};

const CONFIRM_BUTTON_TYPE: Record<AlertDialogType, ButtonType> = {
  question: 'primary',
  danger: 'danger-filled',
  success: 'success',
  warning: 'warning',
  info: 'primary',
};

/**
 * The host component that gets dynamically created and appended to the DOM.
 * It renders the overlay + dialog container and supports two modes:
 * 1. Alert/Confirm mode (built-in template)
 * 2. Custom component mode (dynamic component projected into the container)
 */
@Component({
  selector: 'cq-dialog-host',
  standalone: true,
  imports: [CqButton],
  template: `
    <div class="cq-dialog-overlay" (click)="onOverlayClick()">
      <div [class]="dialogContainerClass" (click)="$event.stopPropagation()">
        @if (alertConfig) {
          <!-- Alert / Confirm mode -->
          <div [class]="alertConfig.cancelText ? 'cq-confirm-dialog' : 'cq-alert-dialog'">
            <div [class]="alertConfig.cancelText ? 'cq-confirm-dialog-content' : ''">
              <div [class]="'cq-dialog-icon ' + iconBgClass">
                <div
                  [class]="'cq-mask-icon ' + iconMaskClass"
                  [style.--mask-url]="'url(' + iconUrl + ')'"
                ></div>
              </div>
              <h3 class="cq-alert-dialog-title">{{ alertConfig.title }}</h3>
              <p class="cq-alert-dialog-message">{{ alertConfig.message }}</p>
              <div class="cq-alert-dialog-actions">
                @if (alertConfig.cancelText) {
                  <cq-button type="secondary" (clicked)="onCancel()">{{
                    alertConfig.cancelText
                  }}</cq-button>
                }
                <cq-button [type]="confirmBtnType" (clicked)="onConfirm()">
                  {{ alertConfig.confirmText || 'OK' }}
                </cq-button>
              </div>
            </div>
          </div>
        } @else {
          <!-- Custom component mode -->
          <ng-template #contentHost></ng-template>
        }
      </div>
    </div>
  `,
})
export class CqDialogHost {
  @ViewChild('contentHost', { read: ViewContainerRef })
  contentContainer!: ViewContainerRef;

  dialogRef!: CqDialogRef;
  ngZone!: NgZone;
  alertConfig: AlertDialogConfig | null = null;
  dialogContainerClass = 'cq-dialog';
  closeOnOverlayClick = true;

  get iconUrl(): string {
    return this.alertConfig ? ICON_MAP[this.alertConfig.type].icon : '';
  }

  get iconMaskClass(): string {
    return this.alertConfig ? ICON_MAP[this.alertConfig.type].iconClass : '';
  }

  get iconBgClass(): string {
    return this.alertConfig ? ICON_MAP[this.alertConfig.type].bgClass : '';
  }

  get confirmBtnType(): ButtonType {
    return this.alertConfig ? CONFIRM_BUTTON_TYPE[this.alertConfig.type] : 'primary';
  }

  onOverlayClick() {
    if (this.closeOnOverlayClick) {
      this.closeInZone(undefined);
    }
  }

  onConfirm() {
    this.closeInZone(true);
  }

  onCancel() {
    this.closeInZone(false);
  }

  /** Close inside Angular's NgZone so subscribers trigger change detection */
  private closeInZone(result: any) {
    if (this.ngZone) {
      this.ngZone.run(() => this.dialogRef.close(result));
    } else {
      this.dialogRef.close(result);
    }
  }
}
