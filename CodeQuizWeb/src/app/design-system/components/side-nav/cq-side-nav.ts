import {
  Component,
  Input,
  Output,
  EventEmitter,
  ViewEncapsulation,
  HostBinding,
} from '@angular/core';

// ========== Side Nav Item ==========
@Component({
  selector: 'cq-side-nav-item',
  standalone: true,
  encapsulation: ViewEncapsulation.None,
  template: `
    <button
      class="cq-side-nav-item"
      [class.active]="active"
      [class.disabled]="disabled"
      [attr.disabled]="disabled ? '' : null"
      (click)="!disabled && selected.emit()"
    >
      @if (icon) {
        <div
          class="cq-mask-icon"
          [style.--mask-url]="'url(' + icon + ')'"
          style="width: 20px; height: 20px; flex-shrink: 0"
        ></div>
      }
      <span class="cq-side-nav-item-label">{{ label }}</span>
    </button>
  `,
  styles: [
    `
      cq-side-nav-item {
        display: block;
      }
    `,
  ],
})
export class CqSideNavItem {
  @Input() label = '';
  @Input() icon = '';
  @Input() active = false;
  @Input() disabled = false;
  @Output() selected = new EventEmitter<void>();
}

// ========== Side Nav Group ==========
@Component({
  selector: 'cq-side-nav-group',
  standalone: true,
  encapsulation: ViewEncapsulation.None,
  template: `
    <div class="cq-side-nav-group" [class.expanded]="expanded">
      <button class="cq-side-nav-group-header" (click)="expanded = !expanded">
        <span class="cq-side-nav-group-label">{{ label }}</span>
        <span class="cq-side-nav-group-chevron">›</span>
      </button>
      <div class="cq-side-nav-group-items" [class.cq-group-collapsed]="!expanded">
        <ng-content></ng-content>
      </div>
    </div>
  `,
  styles: [
    `
      cq-side-nav-group {
        display: block;
      }
    `,
  ],
})
export class CqSideNavGroup {
  @Input() label = '';
  @Input() expanded = true;
}

// ========== Side Nav Container ==========
@Component({
  selector: 'cq-side-nav',
  standalone: true,
  encapsulation: ViewEncapsulation.None,
  template: `
    <div class="cq-side-nav-header">
      <ng-content select="[slot=header]"></ng-content>
      <button
        class="cq-side-nav-toggle"
        (click)="toggleCollapse()"
        [attr.aria-label]="collapsed ? 'Expand sidebar' : 'Collapse sidebar'"
      >
        <span class="cq-side-nav-toggle-icon">{{ collapsed ? '›' : '‹' }}</span>
      </button>
    </div>
    <div class="cq-side-nav-body">
      <ng-content></ng-content>
    </div>
    <div class="cq-side-nav-footer">
      <ng-content select="[slot=footer]"></ng-content>
    </div>
  `,
  styles: [
    `
      cq-side-nav:not(._) {
        display: flex;
        flex-direction: column;
        flex-shrink: 0;
        min-height: 100%;
        background-color: var(--cq-surface-dark);
        border-right: 1px solid var(--cq-border-default);
        overflow: hidden;
        width: 240px;
        transition: width 0.35s cubic-bezier(0.4, 0, 0.2, 1);
      }
    `,
  ],
})
export class CqSideNav {
  @Input() collapsed = false;
  @Output() collapsedChange = new EventEmitter<boolean>();

  @HostBinding('class.cq-side-nav-collapsed') get isCollapsed() {
    return this.collapsed;
  }

  @HostBinding('style.width.px') get hostWidth() {
    return this.collapsed ? 56 : 240;
  }

  toggleCollapse() {
    this.collapsed = !this.collapsed;
    this.collapsedChange.emit(this.collapsed);
  }
}
