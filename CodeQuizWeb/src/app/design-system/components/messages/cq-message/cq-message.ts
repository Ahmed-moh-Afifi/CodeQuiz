import { Component, input } from '@angular/core';

type MessageType = 'info' | 'warning' | 'error';

@Component({
  selector: 'cq-message',
  standalone: true,
  template: `
    <div [class]="'cq-message cq-message-' + type()">
      @if (icon()) {
        <div class="cq-message-icon">
          <div
            [class]="'cq-mask-icon cq-mask-icon-' + type()"
            [style.--mask-url]="'url(' + icon() + ')'"
          ></div>
        </div>
      }
      <div class="cq-message-content">
        @if (title()) {
          <h4 class="cq-message-title">{{ title() }}</h4>
        }
        <p class="cq-message-text">
          <ng-content></ng-content>
        </p>
      </div>
    </div>
  `,
})
export class CqMessage {
  type = input<MessageType>('info');
  title = input<string>('');
  icon = input<string>('');
}
