import { Component, Inject, inject, OnInit } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import {
  InventoryItemCreateEndpoint
} from '../../../endpoints/inventory-items-crud-endpoints/inventory-item-create-endpoint';
import {
  InventoryItemUpdateEndpoint
} from '../../../endpoints/inventory-items-crud-endpoints/inventory-item-update-endpoint';
import {
  InventoryItemGetByIdEndpoint
} from '../../../endpoints/inventory-items-crud-endpoints/inventory-item-getbyid-endpoint';
import {
  MAT_DIALOG_DATA,
  MatDialogRef
} from '@angular/material/dialog';
import {
  CreateInventoryItemDto,
  InventoryItemDto,
  UpdateInventoryItemDto
} from '../inventory-item-model';

type DialogData =
  | { mode: 'create' }
  | { mode: 'edit'; id: number };

@Component({
  selector: 'app-inventory-items-form-dialog',
  standalone: false,
  templateUrl: './inventory-items-form-dialog.html',
  styleUrl: './inventory-items-form-dialog.css'
})
export class InventoryItemsFormDialogComponent implements OnInit {
  private fb = inject(FormBuilder);
  private createEp = inject(InventoryItemCreateEndpoint);
  private updateEp = inject(InventoryItemUpdateEndpoint);
  private getByIdEp = inject(InventoryItemGetByIdEndpoint);

  constructor(
    private ref: MatDialogRef<InventoryItemsFormDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: DialogData
  ) {}

  get title() {
    return this.data.mode === 'edit' ? 'Edit Item' : 'Create Item';
  }

  loading = false;

  form = this.fb.group({
    id: this.fb.control<number | null>(null),

    name: this.fb.control<string>('', {
      validators: [Validators.required],
      nonNullable: true
    }),

    sku: this.fb.control<string>('', {
      validators: [Validators.required],
      nonNullable: true
    }),

    unitType: this.fb.control<string | null>(null, {
      validators: [Validators.required]
    }),

    reorderQty: this.fb.control<number | null>(null, {
      validators: [Validators.required, Validators.min(0)]
    }),

    reorderFrequency: this.fb.control<number | null>(null, {
      validators: [Validators.required, Validators.min(0)]
    }),

    currentQty: this.fb.control<number | null>(null, {
      validators: [Validators.required, Validators.min(0)]
    }),

    restaurantId: this.fb.control<string | null>(null)
  });

  ngOnInit() {
    if (this.data.mode === 'edit') {
      this.loading = true;

      this.getByIdEp.handleAsync({ id: this.data.id }).subscribe({
        next: (dto: InventoryItemDto) => {
          this.form.patchValue({
            name: dto.name,
            sku: dto.sku,
            unitType: dto.unitType.toString(),
            reorderQty: dto.reorderQty,
            reorderFrequency: dto.reorderFrequency,
            currentQty: dto.currentQty,
            restaurantId: dto.restaurantId
          });

          this.loading = false;
        },
        error: () => {
          this.loading = false;
        }
      });
    }
  }

  save() {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.loading = true;

    const raw = this.form.getRawValue();

    if (this.data.mode === 'create') {
      const body: CreateInventoryItemDto = {
        name: raw.name.trim(),
        sku: raw.sku.trim(),
        unitType: Number(raw.unitType),
        reorderQty: raw.reorderQty!,
        reorderFrequency: raw.reorderFrequency!,
        currentQty: raw.currentQty!
      };

      this.createEp.handleAsync(body).subscribe({
        next: () => this.ref.close(true),
        error: () => {
          this.loading = false;
        }
      });
    } else {
      const body: UpdateInventoryItemDto = {
        id: this.data.id,
        name: raw.name.trim(),
        sku: raw.sku.trim(),
        unitType: Number(raw.unitType),
        reorderQty: raw.reorderQty!,
        reorderFrequency: raw.reorderFrequency!,
        currentQty: raw.currentQty!,
        restaurantId: raw.restaurantId ?? ''
      };

      this.updateEp.handleAsync(body).subscribe({
        next: () => this.ref.close(true),
        error: () => {
          this.loading = false;
        }
      });
    }
  }

  cancel() {
    this.ref.close(false);
  }
}
