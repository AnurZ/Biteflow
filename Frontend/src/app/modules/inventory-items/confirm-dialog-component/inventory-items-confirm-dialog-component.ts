import {Component, Inject} from '@angular/core';
import {
  MAT_DIALOG_DATA,
  MatDialogActions,
  MatDialogContent,
  MatDialogRef,
  MatDialogTitle
} from '@angular/material/dialog';
import {MatButton} from '@angular/material/button';

@Component({
  imports: [
    MatDialogContent,
    MatDialogTitle,
    MatDialogActions,
    MatButton
  ],
  template:`
    <h2 mat-dialog-title>{{ data.title }}</h2>
    <mat-dialog-content>{{ data.message }}</mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button (click)="ref.close(false)">Cancel</button>
      <button mat-flat-button color="warn" (click)="ref.close(true)">Delete</button>
    </mat-dialog-actions>
  `
})
export class InventoryItemsConfirmDialogComponent {
  constructor(
    public ref : MatDialogRef<InventoryItemsConfirmDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data : {title: string, message: string}
  ) {}
}
