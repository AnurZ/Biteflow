import { Component, inject, OnInit } from '@angular/core';
import { InventoryItemService } from './inventory-item.servise';
import { InventoryItemDto } from './inventory-item-model';
import { InventoryItemListEndpoint } from '../../endpoints/inventory-items-crud-endpoints/inventory-item-list-endpoint';
import { InventoryItemDeleteEndpoint } from '../../endpoints/inventory-items-crud-endpoints/inventory-item-delete-endpoint';
import { MatDialog } from '@angular/material/dialog';
import { InventoryItemsFormDialogComponent } from './inventory-items-form-dialog/inventory-items-form-dialog';
import { InventoryItemsConfirmDialogComponent } from './confirm-dialog-component/inventory-items-confirm-dialog-component';
import { UnitTypes } from '../meals/meals-model';

@Component({
  selector: 'app-inventory-items',
  templateUrl: './inventory-items.html',
  standalone: false,
  styleUrls: ['./inventory-items.css']
})
export class InventoryItems implements OnInit {
  private listEp = inject(InventoryItemListEndpoint);
  private deleteEp = inject(InventoryItemDeleteEndpoint);
  private dialog = inject(MatDialog);

  items: InventoryItemDto[] = [];
  columns: string[] = [
    'name',
    'sku',
    'unitType',
    'currentQty',
    'actions'
  ];

  unitTypes: string[] = [];
  pageNumber = 1;
  pageSize = 10;
  total = 0;
  search = '';
  sort: string | undefined;

  selectedUnitType: number | null = null;
  minQty: number | null = null;
  maxQty: number | null = null;

  ngOnInit() {
    this.unitTypes = Object.keys(UnitTypes)
      .filter(k => isNaN(Number(k)));

    this.loadItems();
  }

  onUnitTypeChange(unitType: number | null) {
    this.selectedUnitType = unitType;
    this.pageNumber = 1;
    this.loadItems();
  }

  onQuantityChange() {
    this.pageNumber = 1;
    this.loadItems();
  }

  onSearch(search: string) {
    this.search = search;
    this.pageNumber = 1;
    this.loadItems();
  }

  onPage(event: any) {
    this.pageNumber = event.pageIndex + 1;
    this.pageSize = event.pageSize;
    this.loadItems();
  }

  loadItems() {
    const req = {
      pageNumber: this.pageNumber,
      pageSize: this.pageSize,
      search: this.search || undefined,
      sort: this.sort,
      unitType: this.selectedUnitType ?? undefined,
      minQty: this.minQty ?? undefined,
      maxQty: this.maxQty ?? undefined
    };

    this.listEp.handleAsync(req).subscribe(res => {
      this.items = res.items;
      this.total = res.total;
    });
  }

  editItem(id: number) {
    const ref = this.dialog.open(InventoryItemsFormDialogComponent, {
      width: '720px',
      data: { mode: 'edit', id }
    });

    ref.afterClosed().subscribe(changed => {
      if (changed) {
        this.loadItems();
      }
    });
  }

  deleteItem(id: number) {
    const ref = this.dialog.open(InventoryItemsConfirmDialogComponent, {
      data: {
        title: 'Delete inventory item?',
        message: 'This cannot be undone.'
      }
    });

    ref.afterClosed().subscribe(ok => {
      if (ok) {
        this.deleteEp.handleAsync({ id }).subscribe(() => {
          this.loadItems();
        });
      }
    });
  }

  createItem() {
    const ref = this.dialog.open(InventoryItemsFormDialogComponent, {
      width: '720px',
      data: { mode: 'create' }
    });

    ref.afterClosed().subscribe(changed => {
      if (changed) {
        this.loadItems();
      }
    });
  }

  onSort(key: string) {
    if (this.sort === key) {
      this.sort = `-${key}`;
    } else if (this.sort === `-${key}`) {
      this.sort = undefined;
    } else {
      this.sort = key;
    }

    this.loadItems();
  }
}
