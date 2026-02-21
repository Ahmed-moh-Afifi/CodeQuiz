import { Component, Input, Output, EventEmitter, ViewEncapsulation } from '@angular/core';

// ========== Accordion Item ==========
@Component({
  selector: 'cq-accordion-item',
  standalone: true,
  encapsulation: ViewEncapsulation.None,
  template: `
    <div class="cq-accordion-item" [class.expanded]="expanded" [class.disabled]="disabled">
      <button
        class="cq-accordion-header"
        (click)="!disabled && toggle.emit(); !disabled && (expanded = !expanded)"
        [attr.disabled]="disabled ? '' : null"
      >
        <span class="cq-accordion-title">{{ title }}</span>
        <span class="cq-accordion-chevron">›</span>
      </button>
      <div class="cq-accordion-body" [class.cq-accordion-open]="expanded">
        <div class="cq-accordion-inner">
          <ng-content></ng-content>
        </div>
      </div>
    </div>
  `,
  styles: [
    `
      cq-accordion-item {
        display: block;
      }
      .cq-accordion-item {
        border: 1px solid var(--cq-border-default);
        border-radius: var(--cq-radius-md, 8px);
        overflow: hidden;
        margin-bottom: 8px;
        background: var(--cq-card-background);
        transition:
          box-shadow 0.2s ease,
          border-color 0.2s ease;
      }
      .cq-accordion-item:hover:not(.disabled) {
        border-color: var(--cq-border-subtle);
      }
      .cq-accordion-item.expanded {
        box-shadow: 0 2px 8px rgba(0, 0, 0, 0.08);
      }
      .cq-accordion-header {
        display: flex;
        align-items: center;
        justify-content: space-between;
        width: 100%;
        padding: 14px 16px;
        border: none;
        background: transparent;
        color: var(--cq-text-primary);
        font-size: 14px;
        font-weight: 500;
        cursor: pointer;
        text-align: left;
        transition: background-color 0.15s ease;
      }
      .cq-accordion-header:hover {
        background-color: var(--cq-secondary-hover);
      }
      .cq-accordion-chevron {
        font-size: 18px;
        font-weight: 700;
        color: var(--cq-text-muted);
        transform: rotate(0deg);
        transition: transform 0.3s cubic-bezier(0.4, 0, 0.2, 1);
        flex-shrink: 0;
      }
      .cq-accordion-item.expanded .cq-accordion-chevron {
        transform: rotate(90deg);
      }
      .cq-accordion-body {
        max-height: 0;
        overflow: hidden;
        transition:
          max-height 0.35s cubic-bezier(0.4, 0, 0.2, 1),
          opacity 0.3s ease;
        opacity: 0;
      }
      .cq-accordion-body.cq-accordion-open {
        max-height: 500px;
        opacity: 1;
      }
      .cq-accordion-inner {
        padding: 0 16px 16px;
        color: var(--cq-text-secondary);
        font-size: 14px;
        line-height: 1.6;
      }
      .cq-accordion-item.disabled .cq-accordion-header {
        color: var(--cq-text-disabled);
        cursor: not-allowed;
      }
    `,
  ],
})
export class CqAccordionItem {
  @Input() title = '';
  @Input() expanded = false;
  @Input() disabled = false;
  @Output() toggle = new EventEmitter<void>();
}
