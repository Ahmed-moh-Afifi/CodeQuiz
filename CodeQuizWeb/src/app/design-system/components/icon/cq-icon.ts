import { Component, input } from '@angular/core';

export type IconColor = 'current' | 'primary' | 'success' | 'warning' | 'error' | 'muted';

@Component({
  selector: 'cq-icon',
  standalone: true,
  template: `
    <div
      class="cq-icon-el"
      [style.--mask-url]="'url(' + src() + ')'"
      [style.width.px]="size()"
      [style.height.px]="size()"
      [class]="'cq-icon-el cq-icon-' + color()"
    ></div>
  `,
  styles: `
    :host {
      display: inline-flex;
      align-items: center;
      justify-content: center;
      line-height: 0;
    }

    .cq-icon-el {
      display: inline-block;
      mask-repeat: no-repeat;
      mask-position: center;
      mask-size: contain;
      -webkit-mask-repeat: no-repeat;
      -webkit-mask-position: center;
      -webkit-mask-size: contain;
      mask-image: var(--mask-url);
      -webkit-mask-image: var(--mask-url);
    }

    .cq-icon-current {
      background-color: currentColor;
    }
    .cq-icon-primary {
      background-color: var(--cq-primary);
    }
    .cq-icon-success {
      background-color: var(--cq-success);
    }
    .cq-icon-warning {
      background-color: var(--cq-warning);
    }
    .cq-icon-error {
      background-color: var(--cq-error);
    }
    .cq-icon-muted {
      background-color: var(--cq-text-muted);
    }
  `,
})
export class CqIcon {
  src = input.required<string>();
  size = input<number>(20);
  color = input<IconColor>('current');
}
