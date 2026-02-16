import { Component, input } from '@angular/core';

export type BadgeType = 'status' | 'live' | 'success' | 'warning' | 'error' | 'info' | 'action';
export type BadgeSize = 'default' | 'sm';

@Component({
  selector: 'cq-badge',
  standalone: true,
  template: `
    @if (dot()) {
      <span [class]="dotClass"></span>
    } @else {
      <span [class]="badgeClass">
        <ng-content></ng-content>
      </span>
    }
  `,
})
export class CqBadge {
  type = input<BadgeType>('status');
  size = input<BadgeSize>('default');
  /** When true, renders as a small colored dot instead of a pill badge */
  dot = input<boolean>(false);

  get badgeClass(): string {
    let cls = `cq-badge-${this.type()}`;
    if (this.size() === 'sm') {
      cls += ' cq-badge-sm';
    }
    return cls;
  }

  get dotClass(): string {
    const type = this.type();
    let cls = 'cq-badge-dot';
    if (type !== 'status') {
      cls += ` cq-badge-dot-${type}`;
    }
    return cls;
  }
}
