import { Component, input, forwardRef } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR, FormsModule } from '@angular/forms';

@Component({
  selector: 'cq-checkbox',
  standalone: true,
  imports: [FormsModule],
  template: `
    <div class="cq-flex cq-items-center cq-gap-sm">
      <input
        type="checkbox"
        class="cq-checkbox"
        [disabled]="disabled()"
        [ngModel]="value"
        (ngModelChange)="onValueChange($event)"
      />
      @if (label()) {
        <span class="cq-body-text">{{ label() }}</span>
      }
    </div>
  `,
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => CqCheckbox),
      multi: true,
    },
  ],
})
export class CqCheckbox implements ControlValueAccessor {
  label = input<string>('');
  disabled = input<boolean>(false);

  value = false;
  onChange: (value: boolean) => void = () => {};
  onTouched: () => void = () => {};

  onValueChange(val: boolean) {
    this.value = val;
    this.onChange(val);
  }

  writeValue(value: boolean): void {
    this.value = value || false;
  }

  registerOnChange(fn: (value: boolean) => void): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: () => void): void {
    this.onTouched = fn;
  }
}
