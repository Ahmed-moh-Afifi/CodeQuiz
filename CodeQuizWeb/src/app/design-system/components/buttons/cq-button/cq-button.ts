import { Component, input, output } from '@angular/core';

export type ButtonType =
  | 'primary'
  | 'secondary'
  | 'outlined'
  | 'danger'
  | 'ghost'
  | 'success'
  | 'warning'
  | 'danger-filled'
  | 'add-item';

@Component({
  selector: 'cq-button',
  standalone: true,
  imports: [],
  templateUrl: './cq-button.html',
  styleUrl: './cq-button.scss',
})
export class CqButton {
  type = input<ButtonType>('primary');
  disabled = input<boolean>(false);
  loading = input<boolean>(false);
  loadingText = input<string>('');
  icon = input<string>('');

  clicked = output<MouseEvent>();

  get buttonClass(): string {
    const type = this.type();
    if (type === 'danger-filled') return 'cq-btn-danger-filled';
    if (type === 'add-item') return 'cq-add-item-button';
    return `cq-btn-${type}`;
  }

  onClick(event: MouseEvent) {
    if (!this.disabled() && !this.loading()) {
      this.clicked.emit(event);
    }
  }
}
