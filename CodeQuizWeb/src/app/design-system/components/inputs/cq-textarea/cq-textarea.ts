import { Component, input, forwardRef } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR, FormsModule } from '@angular/forms';

@Component({
  selector: 'cq-textarea',
  standalone: true,
  imports: [FormsModule],
  template: `
    <div class="cq-form-group">
      @if (label()) {
        <label class="cq-label">{{ label() }}</label>
      }
      <div class="cq-textarea-wrapper" [class.cq-input-error]="error()">
        <textarea
          class="cq-textarea"
          [placeholder]="placeholder()"
          [rows]="rows()"
          [disabled]="disabled()"
          [ngModel]="value"
          (ngModelChange)="onValueChange($event)"
          (blur)="onTouched()"
        ></textarea>
      </div>
      @if (error()) {
        <span class="cq-error-text">{{ error() }}</span>
      } @else if (helper()) {
        <span class="cq-helper-text">{{ helper() }}</span>
      }
    </div>
  `,
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => CqTextarea),
      multi: true,
    },
  ],
})
export class CqTextarea implements ControlValueAccessor {
  label = input<string>('');
  placeholder = input<string>('');
  rows = input<number>(3);
  error = input<string>('');
  helper = input<string>('');
  disabled = input<boolean>(false);

  value = '';
  onChange: (value: string) => void = () => {};
  onTouched: () => void = () => {};

  onValueChange(val: string) {
    this.value = val;
    this.onChange(val);
  }

  writeValue(value: string): void {
    this.value = value || '';
  }

  registerOnChange(fn: (value: string) => void): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: () => void): void {
    this.onTouched = fn;
  }
}
