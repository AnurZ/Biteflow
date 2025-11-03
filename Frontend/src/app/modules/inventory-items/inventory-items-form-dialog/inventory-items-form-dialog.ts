import {Component, Inject, inject, OnInit} from '@angular/core';
import {FormBuilder, Validators} from '@angular/forms';
import {StaffCreateEndpoint} from '../../../endpoints/staff-crud-endpoints/staff-create-endpoint';
import {
  InventoryItemCreateEndpoint
} from '../../../endpoints/inventory-items-crud-endpoints/inventory-item-create-endpoint';
import {
  InventoryItemUpdateEndpoint
} from '../../../endpoints/inventory-items-crud-endpoints/inventory-item-update-endpoint';
import {
  InventoryItemGetByIdEndpoint
} from '../../../endpoints/inventory-items-crud-endpoints/inventory-item-getbyid-endpoint';
import {MAT_DIALOG_DATA, MatDialogRef} from '@angular/material/dialog';
import {CreateInventoryItemDto, InventoryItemDto, UnitTypes, UpdateInventoryItemDto} from '../inventory-item-model';

type DialogData = { mode: 'create' } | { mode: 'edit'; id: number };

@Component({
  selector: 'app-inventory-items-form-dialog',
  standalone: false,
  templateUrl: './inventory-items-form-dialog.html',
  styleUrl: './inventory-items-form-dialog.css'
})
export class InventoryItemsFormDialogComponent implements OnInit {
  private fb: FormBuilder = inject(FormBuilder);
  private createEp = inject(InventoryItemCreateEndpoint);
  private updateEp = inject(InventoryItemUpdateEndpoint);
  private getByIdEp = inject(InventoryItemGetByIdEndpoint);
  constructor(
    private ref: MatDialogRef<InventoryItemsFormDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: DialogData,
  ) {}

  get title() {
    return this.data.mode === 'edit' ? 'Edit Item' : 'Create Item';
  }


  loading: boolean = false;
  selectedId: number = 0;

  form = this.fb.group({
    id: this.fb.control<number | null >(null),
    name: this.fb.control<string>('',{ validators: [Validators.required], nonNullable: true }),
    sku: this.fb.control<string | null>(null),
    unitType: this.fb.control<string | null>(null),
    reorderQty: this.fb.control<number | null>(null),
    reorderFrequency: this.fb.control<number | null>(null),
    currentQty: this.fb.control<number | null>(null),
    restaurantId: this.fb.control<string | null>(null)
  })


  ngOnInit() {
    if(this.data.mode === 'edit') {
      this.loading = true;
      console.log(this.data.id);
      this.getByIdEp.handleAsync({id: this.data.id}).subscribe({
        next: (dto: InventoryItemDto) =>{
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
        error: () => { this.loading = false; },
      });

    }
  }


  save(){
    if(this.form.invalid) return;
    this.loading = true;

    const raw = this.form.getRawValue()
    if(this.data.mode === 'create') {
      const body: CreateInventoryItemDto = {
        name: raw.name.trim(),
        sku: raw.sku?.trim() ?? '',
        unitType: Number(raw.unitType!),
        reorderQty: raw.reorderQty ?? 0,
        reorderFrequency: raw.reorderFrequency?? 0,
        currentQty: raw.currentQty ?? 0
      };
      console.log('Create payload:', body);

      this.createEp.handleAsync(body).subscribe({
        next: () => this.ref.close(true),
        error: () => { this.loading = false;}
      });
    } else {
      const body: UpdateInventoryItemDto = {
        id: this.data.id,
        name: raw.name.trim(),
        sku: raw.sku?.trim() ?? '',
        unitType: Number(raw.unitType!),
        reorderQty: raw.reorderQty ?? 0,
        reorderFrequency: raw.reorderFrequency?? 0,
        currentQty: raw.currentQty ?? 0,
        restaurantId: raw.restaurantId ?? ''
      };
      console.log(
        `id ${body.id}
              name ${body.name}
              sku ${body.sku}
              unitType ${body.unitType}
              reorderQty ${body.reorderQty}
              reorderFrequency ${body.reorderFrequency}
              currentQty ${body.currentQty}`
      )

      console.log('Create payload:', body);

      this.updateEp.handleAsync(body).subscribe({
        next: () => this.ref.close(true),
        error: () => { this.loading = false;}
      });
    }
  }

  cancel(){
    this.ref.close(false);
  }
}
