import { Component, Inject, inject, OnInit } from '@angular/core';
import {
  InventoryItemListEndpoint
} from '../../../../endpoints/inventory-items-crud-endpoints/inventory-item-list-endpoint';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { MealGetIngredientsEndpoint } from '../../../../endpoints/meals-crud-endpoints/meal-getingredients-endpoint';
import { ListInventoryItemsDto } from '../../../inventory-items/inventory-item-model';
import {addIngredientDto, MealIngredientQueryDto, UnitTypes} from '../../meals-model';
import {FormBuilder, FormGroup} from '@angular/forms';

type DialogData = { mode: 'create' | 'edit'; id?: number; selectedIngredientsList: addIngredientDto[]};

@Component({
  selector: 'app-add-ingredients-dialog',
  standalone: false,
  templateUrl: './add-ingredients-dialog.html',
  styleUrl: './add-ingredients-dialog.css'
})
export class AddIngredientsDialog implements OnInit {
  private getInventoryItemsListEp = inject(InventoryItemListEndpoint);
  private getMealIngredientsEp = inject(MealGetIngredientsEndpoint);

  constructor(
    private ref: MatDialogRef<AddIngredientsDialog>,
    @Inject(MAT_DIALOG_DATA) public data: DialogData
  ) {}

  listIngredients: addIngredientDto[] = [];
  cancelListIngredients: addIngredientDto[] = [];
  loading = false;

  cancel() {
    this.ref.close(false);
  }

  onIngredientChange(ingredient: addIngredientDto) {
    if (ingredient.selected) {
      // Add to array if selected and not already in array
      if (!this.data.selectedIngredientsList.some(x => x.inventoryItemId === ingredient.inventoryItemId)) {
        this.data.selectedIngredientsList.push(ingredient);
      }
    } else {
      // Remove from array if deselected
      this.data.selectedIngredientsList = this.data.selectedIngredientsList.filter(
        x => x.inventoryItemId !== ingredient.inventoryItemId
      );
    }

    console.log('Currently selected ingredients:', this.data.selectedIngredientsList);
  }

  save() {
    const selectedIngredients = this.data.selectedIngredientsList;
    this.ref.close(selectedIngredients);
  }

  isOverflowing(el?: HTMLElement): boolean {
    if (!el) return false;
    return el.scrollWidth > el.clientWidth;
  }





  onInputQty(ingredient: addIngredientDto) {
    const existing = this.data.selectedIngredientsList.find(
      x => x.inventoryItemId === ingredient.inventoryItemId
    );

    if (existing) {
      existing.quantity = ingredient.quantity;
    }
  }

  loadItemsByName(name: string) {
    const filter = name.toLowerCase();
    console.log('filter');
    console.log(this.data.selectedIngredientsList);
    this.getInventoryItemsListEp.handleAsync().subscribe({
      next: (result) => {
        this.listIngredients = result.items
          .filter(ing => ing.name.toLowerCase().includes(filter))
          .map(ing => ({
            inventoryItemId: ing.id,
            inventoryItemName: ing.name,
            quantity: 1,
            unitType: UnitTypes[ing.unitType] as string,
            selected: this.data.selectedIngredientsList.some(x => x.inventoryItemId === ing.id)
          }));
      }
    })
  }

  loadItems() {
    this.loading = true;

    // Step 1: Get all inventory items

    console.log("SELEKTEDINGREDEINTS")
    console.log(this.data.selectedIngredientsList);
      this.getInventoryItemsListEp.handleAsync().subscribe({
        next: (result) => {
          // assuming your endpoint returns { items: [...] }
          this.listIngredients = result.items.map(ing => {
            const existing = this.data.selectedIngredientsList.find(x => x.inventoryItemId === ing.id);
            return {
              inventoryItemId: ing.id,
              inventoryItemName: ing.name,
              quantity: existing ? existing.quantity : 1,
              unitType: UnitTypes[ing.unitType] as string,
              selected: !!existing
            };
          });
          console.log('INGREDIENTS');
          console.log(this.listIngredients);
          this.loading = false;
        },
        error: (err) => {
          console.error('Failed to load inventory items', err);
          this.loading = false;
        }
      });
      return; // so it doesn’t continue to the rest of the function
  }

  ngOnInit() {
    this.cancelListIngredients = this.data.selectedIngredientsList;
    this.loadItems();
  }

  protected readonly UnitTypes = UnitTypes;
}
