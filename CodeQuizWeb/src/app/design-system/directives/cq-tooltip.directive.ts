import { Directive, ElementRef, HostListener, input, Renderer2, OnDestroy } from '@angular/core';

@Directive({
  selector: '[cqTooltip]',
  standalone: true,
})
export class CqTooltipDirective implements OnDestroy {
  cqTooltip = input.required<string>();
  cqTooltipPosition = input<'top' | 'bottom' | 'left' | 'right'>('top');

  private tooltipEl: HTMLElement | null = null;

  constructor(
    private el: ElementRef<HTMLElement>,
    private renderer: Renderer2,
  ) {}

  @HostListener('mouseenter')
  onMouseEnter() {
    if (this.tooltipEl) return;
    this.show();
  }

  @HostListener('mouseleave')
  onMouseLeave() {
    this.hide();
  }

  @HostListener('focus')
  onFocus() {
    this.show();
  }

  @HostListener('blur')
  onBlur() {
    this.hide();
  }

  ngOnDestroy() {
    this.hide();
  }

  private show() {
    const text = this.cqTooltip();
    if (!text || this.tooltipEl) return;

    this.tooltipEl = this.renderer.createElement('div');
    this.renderer.addClass(this.tooltipEl, 'cq-tooltip');
    this.renderer.addClass(this.tooltipEl, `cq-tooltip-${this.cqTooltipPosition()}`);
    this.tooltipEl!.textContent = text;

    this.renderer.appendChild(document.body, this.tooltipEl);

    // Position relative to the host element
    requestAnimationFrame(() => this.positionTooltip());
  }

  private positionTooltip() {
    if (!this.tooltipEl) return;

    const hostRect = this.el.nativeElement.getBoundingClientRect();
    const tipRect = this.tooltipEl.getBoundingClientRect();
    const pos = this.cqTooltipPosition();
    const gap = 8;

    let top = 0;
    let left = 0;

    switch (pos) {
      case 'top':
        top = hostRect.top - tipRect.height - gap + window.scrollY;
        left = hostRect.left + (hostRect.width - tipRect.width) / 2 + window.scrollX;
        break;
      case 'bottom':
        top = hostRect.bottom + gap + window.scrollY;
        left = hostRect.left + (hostRect.width - tipRect.width) / 2 + window.scrollX;
        break;
      case 'left':
        top = hostRect.top + (hostRect.height - tipRect.height) / 2 + window.scrollY;
        left = hostRect.left - tipRect.width - gap + window.scrollX;
        break;
      case 'right':
        top = hostRect.top + (hostRect.height - tipRect.height) / 2 + window.scrollY;
        left = hostRect.right + gap + window.scrollX;
        break;
    }

    this.renderer.setStyle(this.tooltipEl, 'top', `${top}px`);
    this.renderer.setStyle(this.tooltipEl, 'left', `${left}px`);
  }

  private hide() {
    if (this.tooltipEl) {
      this.renderer.removeChild(document.body, this.tooltipEl);
      this.tooltipEl = null;
    }
  }
}
