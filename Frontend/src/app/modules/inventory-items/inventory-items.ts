import {AfterViewInit, Component, Inject, inject, OnInit, ViewChild, viewChild} from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';
import { InventoryItemService } from './inventory-item.servise';
import { InventoryItemDto } from './inventory-item-model';
import {MatPaginator} from '@angular/material/paginator';
import {MatTableDataSource} from '@angular/material/table';
import {InventoryItemListEndpoint} from '../../endpoints/inventory-items-crud-endpoints/inventory-item-list-endpoint';
import {
  InventoryItemDeleteEndpoint
} from '../../endpoints/inventory-items-crud-endpoints/inventory-item-delete-endpoint';
import {MAT_DIALOG_DATA, MatDialog} from '@angular/material/dialog';
import {StaffFormDialogComponent} from '../admin/staff/staff-form-dialog/staff-form-dialog';
import {
  InventoryItemCreateEndpoint
} from '../../endpoints/inventory-items-crud-endpoints/inventory-item-create-endpoint';
import {InventoryItemsFormDialogComponent} from './inventory-items-form-dialog/inventory-items-form-dialog';
import {
  InventoryItemsConfirmDialogComponent
} from './confirm-dialog-component/inventory-items-confirm-dialog-component';
import {UnitTypes} from '../meals/meals-model';



@Component({
  selector: 'app-inventory-items',
  templateUrl: './inventory-items.html',
  standalone: false,
  styleUrls: ['./inventory-items.css']
})
export class InventoryItems implements OnInit, AfterViewInit {
  private listEp = inject(InventoryItemListEndpoint);
  private deleteEp = inject(InventoryItemDeleteEndpoint);
  private dialog = inject(MatDialog);

  @ViewChild(MatPaginator) paginator!: MatPaginator;

  items = new MatTableDataSource<InventoryItemDto>([]);
  columns: string[] = [
    'name',
    'sku',
    'unitType',
    'currentQty',
    'actions'
  ];
  form!: FormGroup;
  selectedId: number | null = null;
  unitTypes: string[] = [];
  pageNumber = 1;
  pageSize = 10;
  total = 0;
  search = '';
  sort: string | undefined;
  constructor(private inventoryService: InventoryItemService, private fb: FormBuilder
              ) {}
  onPage(event: any) {
    console.log('Paginator event', event);
    // Implement server-side pagination if needed
  }

  ngAfterViewInit() {
    this.items.paginator = this.paginator;
  }
  ngOnInit() {
    this.unitTypes = Object.keys(UnitTypes).filter(k => isNaN(Number(k))); // ["Kilogram","Gram",...]


    this.form = this.fb.group({
      name: [''],
      sku: [''],
      unitType: [0],        // default to first enum value
      reorderQty: [0],
      reorderFrequency: [0],
      currentQty: [0]
    });

    this.loadItems();
  }

  loadItems() {
    this.inventoryService.list().subscribe({
      next: (res) => {
        console.log('API response:', res);
        this.items.data = res.items || res; // ✅ assign array to .data
        this.total = (res.items || res).length; // optional: update total for paginator
      },
      error: (err) => console.error('Error fetching inventory items', err)
    });
  }


  saveItem() {
    const data = this.form.value;
    if (this.selectedId) {
      this.inventoryService.update({ ...data, id: this.selectedId }).subscribe(() => this.loadItems());
    } else {
      this.inventoryService.create(data).subscribe(() => this.loadItems());
    }
    this.form.reset();
    this.selectedId = null;
  }

  edit(item: InventoryItemDto) {
    this.form.patchValue(item);
    this.selectedId = item.id;
  }

  delete(id: number) {
    this.inventoryService.delete(id).subscribe(() => this.loadItems());
  }

  loadItemsByName(value: string) {
    if (!value.trim()) {
      this.loadItems();
      return;
    }

    this.inventoryService.getByName(value).subscribe({
      next: (res) => {
        console.log('API response:', res);
        this.items.data = res.items || res; // ✅ assign array to .data
        this.total = (res.items || res).length;
      },
      error: (err) => console.error('Error fetching inventory items', err)
    });
  }

  editItem(id: number) {
    const ref = this.dialog.open(InventoryItemsFormDialogComponent, { width: '720px', data: { mode: 'edit', id } });
    console.log(`ID edit ${id}`);
    ref.afterClosed().subscribe(changed => changed && this.loadItems());
  }

  deleteItem(id: number) {
    const ref = this.dialog.open(InventoryItemsConfirmDialogComponent, {
      data: { title: 'Delete staff member?', message: 'This cannot be undone.' }
    });
    ref.afterClosed().subscribe(ok => {
      console.log(`ID ${id}`);
      if (ok) this.deleteEp.handleAsync({id}).subscribe(() => this.loadItems());
    });
  }

  createItem() {
    const ref = this.dialog.open(InventoryItemsFormDialogComponent, { width: '720px', data: { mode: 'create' } });
    ref.afterClosed().subscribe(changed => changed && this.loadItems());
  }

}
