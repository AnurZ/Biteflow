import { CommonModule } from '@angular/common';
import { Component, Inject } from '@angular/core';
import {
  MAT_DIALOG_DATA,
  MatDialogActions,
  MatDialogContent,
  MatDialogRef
} from '@angular/material/dialog';
import { MatFormField, MatLabel } from '@angular/material/form-field';
import { MatSelect, MatOption } from '@angular/material/select';
import { FormsModule } from '@angular/forms';
import { OrderStatus } from '../../../../services/orders/orders.service';
import { MatButton } from '@angular/material/button';

export interface ChangeOrderStatusDialogData {
  orderId: number;
  currentStatus: OrderStatus;
}

@Component({
  selector: 'app-change-order-status-dialog',
  standalone: true,
  templateUrl: './change-order-status-dialog.html',
  styleUrl: './change-order-status-dialog.css',
  imports: [
    CommonModule,
    MatFormField,
    MatLabel,
    MatSelect,
    MatOption,
    FormsModule,
    MatDialogActions,
    MatDialogContent,
    MatButton
  ]
})
export class ChangeOrderStatusDialogComponent {

  selectedStatus: OrderStatus;

  constructor(
    private dialogRef: MatDialogRef<ChangeOrderStatusDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: ChangeOrderStatusDialogData
  ) {
    this.selectedStatus = data.currentStatus;
  }

  get availableStatuses(): OrderStatus[] {
    switch (this.data.currentStatus) {
      case 'New':
        return ['New', 'Cooking', 'Cancelled'];

      case 'Cooking':
        return ['Cooking', 'ReadyForPickup', 'Cancelled'];

      case 'ReadyForPickup':
        return ['ReadyForPickup', 'Completed', 'Cancelled'];

      case 'Completed':
        return ['Completed'];

      case 'Cancelled':
        return ['Cancelled'];

      default:
        return [this.data.currentStatus];
    }
  }

  confirm(): void {
    this.dialogRef.close(this.selectedStatus);
  }

  cancel(): void {
    this.dialogRef.close(null);
  }
}
