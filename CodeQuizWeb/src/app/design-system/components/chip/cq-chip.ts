import { Component, Input, Output, EventEmitter } from '@angular/core';

@Component({
  selector: 'cq-chip',
  standalone: true,
  template: `
    <span
      class="cq-chip"
      [class.cq-chip-primary]="type === 'primary'"
      [class.cq-chip-success]="type === 'success'"
      [class.cq-chip-warning]="type === 'warning'"
      [class.cq-chip-error]="type === 'error'"
      [class.cq-chip-selected]="selected"
      [class.cq-chip-selectable]="selectable"
      (click)="onChipClick()"
    >
      <ng-content></ng-content>
      @if (removable) {
        <button class="cq-chip-remove" (click)="$event.stopPropagation(); removed.emit()">✕</button>
      }
    </span>
  `,
  styles: [
    `
      .cq-chip {
        display: inline-flex;
        align-items: center;
        gap: 6px;
        padding: 4px 12px;
        border-radius: 20px;
        font-size: 12px;
        font-weight: 500;
        border: 1px solid var(--cq-border-default);
        background: var(--cq-secondary);
        color: var(--cq-text-secondary);
        transition: all 0.15s ease;
        user-select: none;
      }
      .cq-chip.cq-chip-selectable {
        cursor: pointer;
      }
      .cq-chip:hover {
        filter: brightness(1.1);
      }
      .cq-chip-primary {
        background: var(--cq-primary);
        color: var(--cq-text-on-primary);
        border-color: var(--cq-primary);
      }
      .cq-chip-primary:hover {
        background: var(--cq-primary-hover);
        border-color: var(--cq-primary-hover);
      }
      .cq-chip-success {
        background: var(--cq-success-transparent, rgba(31, 122, 77, 0.15));
        color: var(--cq-success-light);
        border-color: var(--cq-success);
      }
      .cq-chip-success:hover {
        background: var(--cq-success-transparent, rgba(31, 122, 77, 0.25));
      }
      .cq-chip-warning {
        background: var(--cq-warning-transparent, rgba(201, 162, 39, 0.15));
        color: var(--cq-warning-light);
        border-color: var(--cq-warning);
      }
      .cq-chip-warning:hover {
        background: var(--cq-warning-transparent, rgba(201, 162, 39, 0.25));
      }
      .cq-chip-error {
        background: var(--cq-error-transparent, rgba(139, 30, 30, 0.15));
        color: var(--cq-error-light);
        border-color: var(--cq-error);
      }
      .cq-chip-error:hover {
        background: var(--cq-error-transparent, rgba(139, 30, 30, 0.25));
      }
      .cq-chip-selected {
        outline: 2px solid var(--cq-primary);
        outline-offset: 1px;
      }
      .cq-chip-remove {
        display: inline-flex;
        align-items: center;
        justify-content: center;
        width: 16px;
        height: 16px;
        border: none;
        border-radius: 50%;
        background: rgba(255, 255, 255, 0.15);
        color: inherit;
        font-size: 10px;
        cursor: pointer;
        padding: 0;
        line-height: 1;
        transition: background 0.15s;
      }
      .cq-chip-remove:hover {
        background: rgba(255, 255, 255, 0.3);
      }
    `,
  ],
})
export class CqChip {
  @Input() type: 'default' | 'primary' | 'success' | 'warning' | 'error' = 'default';
  @Input() removable = false;
  @Input() selectable = false;
  @Input() selected = false;
  @Output() removed = new EventEmitter<void>();
  @Output() selectedChange = new EventEmitter<boolean>();

  onChipClick() {
    if (this.selectable) {
      this.selected = !this.selected;
      this.selectedChange.emit(this.selected);
    }
  }
}
