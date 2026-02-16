import { Component, input } from '@angular/core';

@Component({
  selector: 'cq-tab',
  standalone: true,
  template: `
    <div class="cq-tab-panel" [style.display]="active ? 'block' : 'none'">
      <ng-content></ng-content>
    </div>
  `,
})
export class CqTab {
  label = input.required<string>();
  active = false;
}
