import { Component, Input } from '@angular/core';

@Component({
  selector: 'cq-empty-state',
  standalone: true,
  template: `
    <div class="cq-empty-state">
      @if (icon) {
        <div
          class="cq-mask-icon cq-empty-state-icon"
          [style.--mask-url]="'url(' + icon + ')'"
        ></div>
      }
      @if (title) {
        <h3 class="cq-empty-state-title">{{ title }}</h3>
      }
      @if (message) {
        <p class="cq-empty-state-message">{{ message }}</p>
      }
      <div class="cq-empty-state-actions">
        <ng-content></ng-content>
      </div>
    </div>
  `,
  styles: [
    `
      .cq-empty-state {
        display: flex;
        flex-direction: column;
        align-items: center;
        justify-content: center;
        padding: 48px 24px;
        text-align: center;
      }
      .cq-empty-state-icon {
        width: 56px;
        height: 56px;
        background-color: var(--cq-text-disabled);
        margin-bottom: 16px;
      }
      .cq-empty-state-title {
        color: var(--cq-text-primary);
        font-size: 18px;
        font-weight: 600;
        margin: 0 0 8px;
      }
      .cq-empty-state-message {
        color: var(--cq-text-muted);
        font-size: 14px;
        margin: 0 0 20px;
        max-width: 400px;
        line-height: 1.5;
      }
      .cq-empty-state-actions:empty {
        display: none;
      }
      .cq-empty-state-actions {
        display: flex;
        gap: 8px;
      }
    `,
  ],
})
export class CqEmptyState {
  @Input() icon = '';
  @Input() title = '';
  @Input() message = '';
}
