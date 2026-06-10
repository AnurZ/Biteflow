import {Component, Inject} from '@angular/core';
import {MAT_DIALOG_DATA, MatDialogRef} from '@angular/material/dialog';
import {AdminOrderDto} from '../../../../services/orders/orders.service';

@Component({
  selector: 'app-view-order-details',
  standalone: false,
  templateUrl: './view-order-details.html',
  styleUrl: './view-order-details.css',
})
export class ViewOrderDetails {
  constructor(
    @Inject(MAT_DIALOG_DATA) public data: AdminOrderDto,
    private dialogRef: MatDialogRef<ViewOrderDetails>
  ) {}

  close() {
    this.dialogRef.close();
  }

  getItemTotal(item: any): number {
    return item.quantity * item.unitPrice;
  }

  getOrderTotal(): number {
    return this.data.totalPrice;
  }
}
