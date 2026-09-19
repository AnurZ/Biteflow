import {Component, Inject} from '@angular/core';
import {MAT_DIALOG_DATA, MatDialogRef} from '@angular/material/dialog';
import {AdminOrderDto} from '../../../../services/orders/orders.service';
import { CurrencyService } from '../../../../services/currency-service/currency.service';

@Component({
  selector: 'app-view-order-details',
  standalone: false,
  templateUrl: './view-order-details.html',
  styleUrl: './view-order-details.css',
})
export class ViewOrderDetails {
  constructor(
    @Inject(MAT_DIALOG_DATA) public data: AdminOrderDto,
    private dialogRef: MatDialogRef<ViewOrderDetails>,
    public currencyService: CurrencyService
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
