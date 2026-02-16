import { Component, input, output, forwardRef } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR, FormsModule } from '@angular/forms';

@Component({
  selector: 'cq-input',
  standalone: true,
  imports: [FormsModule],
  template: `
    <div class="cq-form-group">
      @if (label()) {
        <label class="cq-label">{{ label() }}</label>
      }
      <div class="cq-input-wrapper" [class.cq-input-error]="error()">
        <input
          [type]="type()"
          [class]="inputClass()"
          [placeholder]="placeholder()"
          [disabled]="disabled()"
          [ngModel]="value"
          (ngModelChange)="onValueChange($event)"
          (blur)="onTouched()"
        />
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
      useExisting: forwardRef(() => CqInput),
      multi: true,
    },
  ],
})
export class CqInput implements ControlValueAccessor {
  label = input<string>('');
  placeholder = input<string>('');
  type = input<'text' | 'email' | 'password' | 'number'>('text');
  error = input<string>('');
  helper = input<string>('');
  disabled = input<boolean>(false);
  inputClass = input<string>('cq-input');

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
