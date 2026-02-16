import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import {
  CqPage,
  CqButton,
  // CqCard,
  // CqInput,
  // CqSelect,
  // CqTextarea,
  // CqCheckbox,
  // CqSwitch,
  // CqBadge,
  // CqTable,
  // CqDialogService,
  // CqDialogRef,
} from '../../design-system';
import { CqCard } from '../../design-system/components/cards/cq-card/cq-card';

@Component({
  selector: 'testing-page',
  standalone: true,
  imports: [FormsModule, CqPage, CqButton, CqCard],
  templateUrl: './testing-page.html',
  styleUrl: './testing-page.scss',
})
export class TestingPage {
  // dialogService = inject(CqDialogService);

  // Data props
  inputValue = '';
  inputCodeValue = '';
  textareaValue = '';
  selectValue = '1';
  switchValue = true;
  checkboxValue = false;

  // Dialog Openers
  // openDialog(type: DialogType, title: string, message: string, showCancel: boolean = false) {
  //   this.dialogService.open(TestAlertDialog, {
  //     size: 'sm',
  //     data: {
  //       type,
  //       title,
  //       message,
  //       confirmText: showCancel
  //         ? type === 'danger'
  //           ? 'Delete'
  //           : 'Yes, Proceed'
  //         : type === 'success'
  //           ? 'Continue'
  //           : 'OK',
  //       cancelText: showCancel ? 'Cancel' : '',
  //     },
  //   });
  // }

  // openQuestionDialog() {
  //   this.openDialog(
  //     'question',
  //     'Confirm Action',
  //     'Do you want to proceed with this operation?',
  //     true,
  //   );
  // }

  // openDangerDialog() {
  //   this.openDialog(
  //     'danger',
  //     'Delete Item?',
  //     'Are you sure you want to delete this item? This action cannot be undone.',
  //     true,
  //   );
  // }

  // openSuccessDialog() {
  //   this.openDialog('success', 'Success!', 'Your changes have been saved successfully.');
  // }

  // openWarningDialog() {
  //   this.openDialog(
  //     'warning',
  //     'Warning',
  //     'This action may have unintended consequences. Please review before proceeding.',
  //     true,
  //   );
  // }

  // openInfoDialog() {
  //   this.openDialog('info', 'Information', 'Here is some useful information.');
  // }
}
