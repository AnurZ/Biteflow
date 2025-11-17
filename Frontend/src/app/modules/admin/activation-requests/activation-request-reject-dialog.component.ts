import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';

@Component({
  selector: 'app-activation-request-reject-dialog',
  standalone: true,
  templateUrl: './activation-request-reject-dialog.component.html',
  styleUrl: './activation-request-reject-dialog.component.css',
  imports: [
    CommonModule,
    FormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
  ],
})
export class ActivationRequestRejectDialogComponent {
  private readonly dialogRef = inject(MatDialogRef<ActivationRequestRejectDialogComponent>);

  reason = '';

  cancel(): void {
    this.dialogRef.close();
  }

  submit(): void {
    const value = this.reason.trim();
    if (!value) return;
    this.dialogRef.close(value);
  }
}
