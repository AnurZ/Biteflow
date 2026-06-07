import { Component } from '@angular/core';
import {MatDialogActions, MatDialogContent, MatDialogRef} from '@angular/material/dialog';
import {MatRadioButton, MatRadioGroup} from '@angular/material/radio';
import {FormsModule} from '@angular/forms';

@Component({
  selector: 'app-export-format-dialog',
  templateUrl: './export-format-dialog.html',
  imports: [
    MatDialogActions,
    MatRadioButton,
    MatRadioGroup,
    MatDialogContent,
    FormsModule
  ]
})
export class ExportFormatDialogComponent {

  selectedFormat: 'xlsx' | 'csv' = 'xlsx';

  constructor(
    private dialogRef: MatDialogRef<ExportFormatDialogComponent>
  ) {}

  confirm() {
    this.dialogRef.close(this.selectedFormat);
  }

  cancel() {
    this.dialogRef.close(null);
  }
}
