import { Component, Input, Output, EventEmitter } from '@angular/core';

// ========== Breadcrumb Item ==========
@Component({
  selector: 'cq-breadcrumb-item',
  standalone: true,
  template: `
    @if (link) {
      <a class="cq-breadcrumb-link" (click)="selected.emit()">{{ label }}</a>
    } @else {
      <span class="cq-breadcrumb-current">{{ label }}</span>
    }
  `,
  styles: [
    `
      :host {
        display: inline;
      }
      .cq-breadcrumb-link {
        color: var(--cq-text-muted);
        font-size: 13px;
        text-decoration: none;
        cursor: pointer;
        transition: color 0.15s;
      }
      .cq-breadcrumb-link:hover {
        color: var(--cq-text-primary);
      }
      .cq-breadcrumb-current {
        color: var(--cq-text-primary);
        font-size: 13px;
        font-weight: 500;
      }
    `,
  ],
})
export class CqBreadcrumbItem {
  @Input() label = '';
  @Input() link = true;
  @Output() selected = new EventEmitter<void>();
}

// ========== Breadcrumb Container ==========
@Component({
  selector: 'cq-breadcrumb',
  standalone: true,
  template: `
    <nav class="cq-breadcrumb" aria-label="Breadcrumb">
      <ng-content></ng-content>
    </nav>
  `,
  styles: [
    `
      .cq-breadcrumb {
        display: flex;
        align-items: center;
        gap: 8px;
        flex-wrap: wrap;
      }
      :host ::ng-deep cq-breadcrumb-item:not(:last-child)::after {
        content: '/';
        color: var(--cq-text-disabled, #5a5a5a);
        font-size: 13px;
        margin-left: 8px;
      }
    `,
  ],
})
export class CqBreadcrumb {
  @Input() separator = '/';
}
