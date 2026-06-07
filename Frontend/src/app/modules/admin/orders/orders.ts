import {Component, inject, OnInit} from '@angular/core';
import {
  OrdersService,
  OrderStatus,
  OrderDto,
  AdminOrderDto
} from '../../../services/orders/orders.service';
import {FormBuilder} from '@angular/forms';
import {OrderExportEndpoint} from '../../../services/data-export-services/orders-export-services/OrderExportEndpoint';
import {OrderImportEndpoint} from '../../../services/data-import-services/orders-import-services/OrderImportEndpoint';
import {MatDialog} from '@angular/material/dialog';
import {ExportFormatDialogComponent} from './Orders-export-dialog/ExportFormatDialogComponent';

@Component({
  selector: 'app-orders',
  standalone: false,
  templateUrl: './orders.html',
  styleUrl: './orders.css',
})
export class Orders implements OnInit {
  fromDate: Date | null = null;
  toDate: Date | null = null;

  orders: AdminOrderDto[] = [];

  total = 0;
  pageSize = 10;
  pageNumber = 1;

  selectedStatus?: OrderStatus;
  sort: string = '-createdAt';

  displayedColumns: string[] = [
    'id',
    'tableNumber',
    'items',
    'total',
    'status',
    'createdAt',
    'actions'
  ];

  constructor(private orderService: OrdersService, private orderExport: OrderExportEndpoint,
              private orderImport: OrderImportEndpoint,
              private dialog: MatDialog) {}


  ngOnInit(): void {
    this.loadOrders();
  }

  dateFilter() {
    this.pageNumber = 1;
    this.loadOrders();
  }

  loadOrders() {
    this.orderService.adminList({
      pageNumber: this.pageNumber,
      pageSize: this.pageSize,
      statuses: this.selectedStatus ? [this.selectedStatus] : undefined,
      sort: this.sort,

      fromUtc: this.fromDate ? new Date(this.fromDate).toISOString() : undefined,
      toUtc: this.toDate ? new Date(this.toDate).toISOString() : undefined
    }).subscribe({
      next: (res) => {
        this.orders = res.items;
        this.total = res.total;
      }
    });
  }

  onStatusFilter(value: string) {
    this.selectedStatus = value ? (value as OrderStatus) : undefined;
    this.pageNumber = 1;
    this.loadOrders();
  }

  onPage(event: any) {
    console.log(event);
    this.pageNumber = event.pageIndex + 1;
    this.pageSize = event.pageSize;
    this.loadOrders();
  }

  onSort(column: string) {
    if (this.sort === column) {
      this.sort = `-${column}`;
    } else {
      this.sort = column;
    }

    this.loadOrders();
  }

  view(id: number) {
    console.log('View order', id);
  }

  changeStatus(id: number) {
    console.log('Change status', id);
  }

  exportOrders() {
    const dialogRef = this.dialog.open(ExportFormatDialogComponent, {
      width: '250px',
      height: '210px'
    });

    dialogRef.afterClosed().subscribe(format => {
      if (!format) return;

      this.orderExport.handleAsync({
        fromDate: this.fromDate ? new Date(this.fromDate).toISOString() : undefined,
        toDate: this.toDate ? new Date(this.toDate).toISOString() : undefined,
        status: this.selectedStatus ?? undefined,
        format
      }).subscribe(blob => {
        const file = new Blob([blob], {
          type: format === 'xlsx'
            ? 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet'
            : 'text/csv'
        });

        const url = window.URL.createObjectURL(file);
        const a = document.createElement('a');

        a.href = url;
        a.download = `orders.${format}`;
        a.click();

        URL.revokeObjectURL(url);
      });
    });
  }

  onFileSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];

    if (!file) return;

    const extension =
      file.name.split('.').pop()?.toLowerCase();

    let request;

    if (extension === 'csv') {
      request = this.orderImport.importCsv(file);
    }
    else if (extension === 'xlsx') {
      request = this.orderImport.importExcel(file);
    }
    else {
      alert('Only CSV and XLSX files are supported');
      return;
    }

    request.subscribe({
      next: () => {
        this.loadOrders();
      },
      error: err => {
        console.error(err);
      }
    });
  }


}
