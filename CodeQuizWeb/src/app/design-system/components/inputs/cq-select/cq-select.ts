import { Component, input, forwardRef } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR, FormsModule } from '@angular/forms';

export interface CqSelectOption {
  value: string;
  label: string;
}

@Component({
  selector: 'cq-select',
  standalone: true,
  imports: [FormsModule],
  template: `
    <div class="cq-form-group">
      @if (label()) {
        <label class="cq-label">{{ label() }}</label>
      }
      <div class="cq-select-wrapper" [class.cq-input-error]="error()">
        <select
          class="cq-select"
          [disabled]="disabled()"
          [ngModel]="value"
          (ngModelChange)="onValueChange($event)"
          (blur)="onTouched()"
        >
          @for (opt of options(); track opt.value) {
            <option [value]="opt.value">{{ opt.label }}</option>
          }
        </select>
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
      useExisting: forwardRef(() => CqSelect),
      multi: true,
    },
  ],
})
export class CqSelect implements ControlValueAccessor {
  label = input<string>('');
  options = input<CqSelectOption[]>([]);
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
