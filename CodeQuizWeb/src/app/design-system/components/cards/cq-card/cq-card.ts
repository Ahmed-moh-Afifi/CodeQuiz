import { Component, computed, input } from '@angular/core';

type CardType = 'standard' | 'elevated' | 'inner' | 'highlighted';

@Component({
  selector: 'cq-card',
  imports: [],
  templateUrl: './cq-card.html',
  styleUrl: './cq-card.scss',
})
export class CqCard {
  type = input<CardType>('standard');
  className = computed(() => this.getCardClass);

  get getCardClass(): string {
    if (this.type() === 'standard') {
      return 'cq-card';
    } else {
      return `cq-card-${this.type()}`;
    }
  }
}
