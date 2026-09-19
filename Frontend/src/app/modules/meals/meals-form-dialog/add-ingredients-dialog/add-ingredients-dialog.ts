import { Component, Inject, OnInit } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { InventoryItemListEndpoint } from '../../../../endpoints/inventory-items-crud-endpoints/inventory-item-list-endpoint';
import { addIngredientDto, UnitTypes } from '../../meals-model';

type DialogData = {
  mode?: 'create' | 'edit';
  id?: number;
  selectedIngredientsList: addIngredientDto[];
};

@Component({
  selector: 'app-add-ingredients-dialog',
  standalone: false,
  templateUrl: './add-ingredients-dialog.html',
  styleUrl: './add-ingredients-dialog.css'
})
export class AddIngredientsDialog implements OnInit {
  private getInventoryItemsListEp: InventoryItemListEndpoint;

  constructor(
    private ref: MatDialogRef<AddIngredientsDialog>,
    @Inject(MAT_DIALOG_DATA) public data: DialogData,
    getInventoryItemsListEp: InventoryItemListEndpoint
  ) {
    this.getInventoryItemsListEp = getInventoryItemsListEp;
  }

  listIngredients: addIngredientDto[] = [];
  selectedIngredients: addIngredientDto[] = [];
  loading = false;

  ngOnInit(): void {
    // Work on a copy so Cancel does not modify the parent dialog.
    this.selectedIngredients = this.data.selectedIngredientsList.map(item => ({
      ...item
    }));

    this.loadItems();
  }

  cancel(): void {
    this.ref.close(false);
  }

  onIngredientChange(ingredient: addIngredientDto): void {
    if (ingredient.selected) {
      const exists = this.selectedIngredients.some(
        x => x.inventoryItemId === ingredient.inventoryItemId
      );

      if (!exists) {
        this.selectedIngredients.push({
          ...ingredient
        });
      }
    } else {
      this.selectedIngredients = this.selectedIngredients.filter(
        x => x.inventoryItemId !== ingredient.inventoryItemId
      );
    }
  }

  onInputQty(ingredient: addIngredientDto): void {
    const existing = this.selectedIngredients.find(
      x => x.inventoryItemId === ingredient.inventoryItemId
    );

    if (existing) {
      existing.quantity = ingredient.quantity;
    }
  }

  save(): void {
    this.ref.close(this.selectedIngredients);
  }

  loadItems(search: string = ''): void {
    this.loading = true;

    this.getInventoryItemsListEp.handleAsync({
      pageNumber: 1,
      pageSize: 100,
      search,
      sort: ''
    }).subscribe({
      next: result => {
        this.listIngredients = result.items.map(ing => {
          const existing = this.selectedIngredients.find(
            x => x.inventoryItemId === ing.id
          );

          return {
            inventoryItemId: ing.id,
            inventoryItemName: ing.name,
            quantity: existing?.quantity ?? 1,
            unitType: UnitTypes[ing.unitType] as string,
            selected: !!existing
          };
        });

        this.loading = false;
      },
      error: () => {
        this.loading = false;
      }
    });
  }
}
