import { Component, input, output, model } from '@angular/core';
import { CqDialog } from '../cq-dialog/cq-dialog';
import { CqButton } from '../../buttons/cq-button/cq-button';

type AlertDialogType = 'question' | 'danger' | 'success' | 'warning' | 'info';
type ButtonType =
  | 'primary'
  | 'secondary'
  | 'outlined'
  | 'danger'
  | 'ghost'
  | 'success'
  | 'warning'
  | 'danger-filled';

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

@Component({
  selector: 'cq-alert-dialog',
  standalone: true,
  imports: [CqDialog, CqButton],
  template: `
    <cq-dialog [(visible)]="visible" size="sm" (closed)="cancelled.emit()">
      <div [class]="cancelText() ? 'cq-confirm-dialog' : 'cq-alert-dialog'">
        <div [class]="cancelText() ? 'cq-confirm-dialog-content' : ''">
          <div [class]="'cq-dialog-icon ' + iconConfig.bgClass">
            <div
              [class]="'cq-mask-icon ' + iconConfig.iconClass"
              [style.--mask-url]="'url(' + iconConfig.icon + ')'"
            ></div>
          </div>
          <h3 class="cq-alert-dialog-title">{{ title() }}</h3>
          <p class="cq-alert-dialog-message">{{ message() }}</p>
          <div class="cq-alert-dialog-actions">
            @if (cancelText()) {
              <cq-button type="secondary" (clicked)="onCancel()">{{ cancelText() }}</cq-button>
            }
            <cq-button [type]="confirmBtnType" (clicked)="onConfirm()">
              {{ confirmText() }}
            </cq-button>
          </div>
        </div>
      </div>
    </cq-dialog>
  `,
})
export class CqAlertDialog {
  visible = model<boolean>(false);
  type = input<AlertDialogType>('info');
  title = input<string>('');
  message = input<string>('');
  confirmText = input<string>('OK');
  cancelText = input<string>('');

  confirmed = output<void>();
  cancelled = output<void>();

  get iconConfig() {
    return ICON_MAP[this.type()];
  }

  get confirmBtnType(): ButtonType {
    return CONFIRM_BUTTON_TYPE[this.type()];
  }

  onConfirm() {
    this.confirmed.emit();
  }

  onCancel() {
    this.cancelled.emit();
  }
}
