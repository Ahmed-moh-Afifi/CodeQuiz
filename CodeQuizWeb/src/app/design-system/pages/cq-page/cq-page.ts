import { Component, input, output } from '@angular/core';

@Component({
  selector: 'cq-page',
  imports: [],
  templateUrl: './cq-page.html',
  styleUrl: './cq-page.scss',
})
export class CqPage {
  title = input<string>();
  subtitle = input<string>();
  hasBackButton = input<boolean>(true);
  back = output<void>();

  backClick() {
    this.back.emit();
  }
}
