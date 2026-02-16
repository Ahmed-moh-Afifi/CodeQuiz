import { Component, input, output, model } from '@angular/core';

type DialogSize = 'sm' | 'md' | 'lg';

@Component({
  selector: 'cq-dialog',
  standalone: true,
  templateUrl: './cq-dialog.html',
  styleUrl: './cq-dialog.scss',
})
export class CqDialog {
  visible = model<boolean>(false);
  size = input<DialogSize>('md');
  closeOnOverlay = input<boolean>(true);

  closed = output<void>();

  get sizeClass(): string {
    if (this.size() === 'md') return '';
    return `cq-dialog-${this.size()}`;
  }

  onOverlayClick() {
    if (this.closeOnOverlay()) {
      this.close();
    }
  }

  close() {
    this.visible.set(false);
    this.closed.emit();
  }
}
