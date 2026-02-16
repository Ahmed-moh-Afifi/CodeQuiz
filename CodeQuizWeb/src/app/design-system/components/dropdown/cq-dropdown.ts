import {
  Component,
  input,
  output,
  ElementRef,
  HostListener,
  signal,
  contentChildren,
} from '@angular/core';

@Component({
  selector: 'cq-dropdown-item',
  standalone: true,
  template: `
    <button
      class="cq-dropdown-menu-item"
      [class.cq-dropdown-menu-item-disabled]="disabled()"
      [disabled]="disabled()"
      (click)="onSelect()"
    >
      <ng-content></ng-content>
    </button>
  `,
})
export class CqDropdownItem {
  value = input<any>();
  disabled = input<boolean>(false);
  selected = output<any>();

  onSelect() {
    if (!this.disabled()) {
      this.selected.emit(this.value());
    }
  }
}

@Component({
  selector: 'cq-dropdown-divider',
  standalone: true,
  template: `<div class="cq-dropdown-divider"></div>`,
})
export class CqDropdownDivider {}

@Component({
  selector: 'cq-dropdown',
  standalone: true,
  imports: [],
  template: `
    <div class="cq-dropdown" [class.cq-dropdown-open]="isOpen()">
      <div class="cq-dropdown-trigger" (click)="toggle()">
        <ng-content select="[slot=trigger]"></ng-content>
      </div>
      @if (isOpen()) {
        <div class="cq-dropdown-menu" [class]="'cq-dropdown-menu cq-dropdown-menu-' + align()">
          <ng-content></ng-content>
        </div>
      }
    </div>
  `,
})
export class CqDropdown {
  align = input<'left' | 'right'>('left');
  isOpen = signal(false);

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent) {
    if (this.isOpen() && !(this.el.nativeElement as HTMLElement).contains(event.target as Node)) {
      this.isOpen.set(false);
    }
  }

  constructor(private el: ElementRef) {}

  toggle() {
    this.isOpen.update((v) => !v);
  }

  close() {
    this.isOpen.set(false);
  }
}
