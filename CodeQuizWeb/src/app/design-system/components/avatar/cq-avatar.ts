import { Component, input, computed } from '@angular/core';

export type AvatarSize = 'sm' | 'md' | 'lg';

@Component({
  selector: 'cq-avatar',
  standalone: true,
  template: `
    @if (src() && !imgError) {
      <img
        [src]="src()"
        [alt]="alt() || initials()"
        [class]="avatarClass()"
        (error)="onImgError()"
      />
    } @else {
      <div [class]="avatarClass()">{{ initials() }}</div>
    }
  `,
  styles: [
    `
      :host {
        display: inline-block;
      }
      img {
        object-fit: cover;
        border-radius: 50%;
      }
    `,
  ],
})
export class CqAvatar {
  size = input<AvatarSize>('md');
  initials = input<string>('');
  src = input<string>('');
  alt = input<string>('');

  imgError = false;

  avatarClass = computed(() => {
    const s = this.size();
    let cls = 'cq-avatar';
    if (s !== 'md') cls += ` cq-avatar-${s}`;
    return cls;
  });

  onImgError() {
    this.imgError = true;
  }
}
