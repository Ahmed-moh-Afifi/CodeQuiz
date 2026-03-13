import {
  Component,
  Input,
  Output,
  EventEmitter,
  ViewEncapsulation,
  HostBinding,
  ContentChildren,
  QueryList,
  AfterContentInit,
  OnDestroy,
  OnInit,
  ViewChild,
  TemplateRef,
  ChangeDetectorRef,
  NgZone,
} from '@angular/core';
import { NgTemplateOutlet } from '@angular/common';
import { Subscription, fromEvent } from 'rxjs';
import { debounceTime } from 'rxjs/operators';

const MOBILE_BREAKPOINT = 768;

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
      (click)="handleClick()"
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
    <ng-template #contentTpl>
      <ng-content></ng-content>
    </ng-template>
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

  @ViewChild('contentTpl', { static: true }) contentTemplate!: TemplateRef<any>;

  handleClick() {
    if (!this.disabled) {
      this.selected.emit();
    }
  }
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
  imports: [NgTemplateOutlet],
  encapsulation: ViewEncapsulation.None,
  template: `
    <!-- Mobile top bar (hamburger + active page label) -->
    @if (isMobile) {
      <div class="cq-side-nav-mobile-bar">
        <button class="cq-side-nav-hamburger" (click)="mobileOpen = true" aria-label="Open menu">
          <span class="cq-hamburger-line"></span>
          <span class="cq-hamburger-line"></span>
          <span class="cq-hamburger-line"></span>
        </button>
        <span class="cq-side-nav-mobile-title">{{ activeItem?.label }}</span>
      </div>
    }

    <!-- Backdrop for mobile drawer -->
    @if (isMobile && mobileOpen) {
      <div class="cq-side-nav-backdrop" (click)="mobileOpen = false"></div>
    }

    <!-- Rail / Drawer -->
    <div
      class="cq-side-nav-rail"
      [class.cq-side-nav-rail-mobile]="isMobile"
      [class.cq-side-nav-rail-open]="isMobile && mobileOpen"
    >
      <div class="cq-side-nav-header">
        <div class="cq-side-nav-header-content">
          <ng-content select="[slot=header]"></ng-content>
        </div>
        @if (isMobile) {
          <button class="cq-side-nav-toggle" (click)="mobileOpen = false" aria-label="Close menu">
            <span class="cq-side-nav-toggle-icon">✕</span>
          </button>
        } @else {
          <button
            class="cq-side-nav-toggle"
            (click)="toggleCollapse()"
            [attr.aria-label]="collapsed ? 'Expand sidebar' : 'Collapse sidebar'"
          >
            <span class="cq-side-nav-toggle-icon">{{ collapsed ? '›' : '‹' }}</span>
          </button>
        }
      </div>
      <div class="cq-side-nav-body">
        <ng-content></ng-content>
      </div>
      <div class="cq-side-nav-footer">
        <ng-content select="[slot=footer]"></ng-content>
      </div>
    </div>

    @if (activeItem?.contentTemplate) {
      <div class="cq-side-nav-content">
        <ng-container [ngTemplateOutlet]="activeItem!.contentTemplate"></ng-container>
      </div>
    }
  `,
  styles: [],
})
export class CqSideNav implements OnInit, AfterContentInit, OnDestroy {
  @ContentChildren(CqSideNavItem, { descendants: true })
  items!: QueryList<CqSideNavItem>;

  @Input() collapsed = false;
  @Output() collapsedChange = new EventEmitter<boolean>();

  /** Emits whenever the active item changes. */
  @Output() activeItemChange = new EventEmitter<CqSideNavItem>();

  /** The currently active nav item (read-only for external access). */
  activeItem: CqSideNavItem | null = null;

  /** Whether the viewport is at mobile size. */
  isMobile = false;
  /** Whether the mobile drawer is open. */
  mobileOpen = false;

  private itemSubs: Subscription[] = [];
  private itemsChangeSub: Subscription | null = null;
  private resizeSub: Subscription | null = null;

  constructor(
    private cdr: ChangeDetectorRef,
    private ngZone: NgZone,
  ) {}

  @HostBinding('class.cq-side-nav-collapsed') get isCollapsed() {
    return this.collapsed && !this.isMobile;
  }

  @HostBinding('class.cq-side-nav-mobile') get isMobileHost() {
    return this.isMobile;
  }

  ngOnInit() {
    this.checkMobile();
    this.ngZone.runOutsideAngular(() => {
      this.resizeSub = fromEvent(window, 'resize')
        .pipe(debounceTime(100))
        .subscribe(() => {
          this.ngZone.run(() => this.checkMobile());
        });
    });
  }

  ngAfterContentInit() {
    this.setupItems();
    this.itemsChangeSub = this.items.changes.subscribe(() => this.setupItems());
  }

  ngOnDestroy() {
    this.cleanupSubs();
    this.itemsChangeSub?.unsubscribe();
    this.resizeSub?.unsubscribe();
  }

  /** Programmatically select an item by its label or reference. */
  selectItem(item: CqSideNavItem) {
    this.items.forEach((i) => (i.active = false));
    item.active = true;
    this.activeItem = item;
    this.activeItemChange.emit(item);
    // Auto-close drawer on mobile after selection
    if (this.isMobile) {
      this.mobileOpen = false;
    }
  }

  toggleCollapse() {
    this.collapsed = !this.collapsed;
    this.collapsedChange.emit(this.collapsed);
  }

  private checkMobile() {
    const wasMobile = this.isMobile;
    this.isMobile = window.innerWidth < MOBILE_BREAKPOINT;
    if (!this.isMobile) {
      this.mobileOpen = false;
    }
    if (wasMobile !== this.isMobile) {
      this.cdr.markForCheck();
    }
  }

  private cleanupSubs() {
    this.itemSubs.forEach((s) => s.unsubscribe());
    this.itemSubs = [];
  }

  private setupItems() {
    this.cleanupSubs();

    // Subscribe to each item's selected event
    this.items.forEach((item) => {
      this.itemSubs.push(item.selected.subscribe(() => this.selectItem(item)));
    });

    // Activate the first [active]="true" item, or the first non-disabled item
    if (!this.activeItem || !this.items.find((i) => i === this.activeItem)) {
      const activeItem = this.items.find((i) => i.active);
      if (activeItem) {
        this.selectItem(activeItem);
      } else {
        const firstEnabled = this.items.find((i) => !i.disabled);
        if (firstEnabled) {
          this.selectItem(firstEnabled);
        }
      }
    }
  }
}
