import { Component } from '@angular/core';
import { CqDialogRef } from '../../design-system/services/cq-dialog-ref';
import { CqButton } from '../../design-system/components/buttons/cq-button/cq-button';
import { CqInput } from '../../design-system/components/inputs/cq-input/cq-input';
import { CqTextarea } from '../../design-system/components/inputs/cq-textarea/cq-textarea';
import {
  CqSelect,
  CqSelectOption,
} from '../../design-system/components/inputs/cq-select/cq-select';
import { FormsModule } from '@angular/forms';

/**
 * Sample form dialog content, opened via `CqDialogService.open(CreateQuizDialog)`.
 * Demonstrates how to inject CqDialogRef and close with a result.
 */
@Component({
  selector: 'create-quiz-dialog',
  standalone: true,
  imports: [CqButton, CqInput, CqTextarea, CqSelect, FormsModule],
  template: `
    <div class="cq-dialog-header">
      <h3 class="cq-dialog-title">Create New Quiz</h3>
      <button class="cq-dialog-close" (click)="dialogRef.close()">
        <div
          class="cq-mask-icon"
          style="--mask-url: url('assets/icons/close.svg'); background-color: var(--cq-text-secondary); width: 18px; height: 18px;"
        ></div>
      </button>
    </div>
    <div class="cq-dialog-content">
      <cq-input label="Quiz Title" placeholder="Enter quiz title..." [(ngModel)]="title"></cq-input>
      <cq-textarea
        label="Description"
        placeholder="Describe this quiz..."
        [(ngModel)]="description"
      ></cq-textarea>
      <cq-select label="Category" [options]="categories" [(ngModel)]="category"></cq-select>
    </div>
    <div class="cq-dialog-footer">
      <cq-button type="secondary" (clicked)="dialogRef.close()">Cancel</cq-button>
      <cq-button type="primary" (clicked)="onSubmit()">Create Quiz</cq-button>
    </div>
  `,
})
export class CreateQuizDialog {
  title = '';
  description = '';
  category = 'javascript';

  categories: CqSelectOption[] = [
    { value: 'javascript', label: 'JavaScript' },
    { value: 'python', label: 'Python' },
    { value: 'cpp', label: 'C++' },
    { value: 'csharp', label: 'C#' },
  ];

  constructor(public dialogRef: CqDialogRef) {}

  onSubmit() {
    this.dialogRef.close({
      title: this.title,
      description: this.description,
      category: this.category,
    });
  }
}
