import { Component, Inject, OnInit, inject, ViewChild, ElementRef } from '@angular/core';
import { FormArray, FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { MatDialog, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatPaginator } from '@angular/material/paginator';
import { MatTableDataSource } from '@angular/material/table';
import { finalize, map, of } from 'rxjs';
import { AddIngredientsDialog } from './add-ingredients-dialog/add-ingredients-dialog';
import { ConfirmDialogComponent } from '../../admin/staff/confirm-dialog/confirm-dialog-component';

import {
  addIngredientDto,
  CreateMealCommand,
  GetMealByIdDto,
  MealDto,
  MealIngredientApiDto,
  MealIngredientQueryDto,
  UpdateMealCommand,
  UnitTypes, MealCategory
} from '../meals-model';

import { MealCreateEndpoint } from '../../../endpoints/meals-crud-endpoints/meal-create-endpoint';
import { MealUpdateEndpoint } from '../../../endpoints/meals-crud-endpoints/meal-update-endpoint';
import { MealGetByIdEndpoint } from '../../../endpoints/meals-crud-endpoints/meal-getbyid-endpoint';
import { MealGetListEndpoint } from '../../../endpoints/meals-crud-endpoints/meal-getlist-endpoint';
import { MealGetIngredientsEndpoint } from '../../../endpoints/meals-crud-endpoints/meal-getingredients-endpoint';
import { InventoryItemListEndpoint } from '../../../endpoints/inventory-items-crud-endpoints/inventory-item-list-endpoint';
import { FileUploadEndpoint } from '../../../endpoints/file-upload-endpoint/file-upload-endpoint';
import { mapUnitTypeToString } from '../../../helper/unit-type-helper';
import {MealCategoryGetEndpoint} from '../../../endpoints/meal-category-crud-endpoint/meal-category-get-endpoint';

type DialogData = { mode: 'create' } | { mode: 'edit'; id: number };

@Component({
  selector: 'app-meals-form-dialog',
  standalone: false,
  templateUrl: './meals-form-dialog.html',
  styleUrls: ['./meals-form-dialog.css']
})
export class MealsFormDialog implements OnInit {
  private fb = inject(FormBuilder);
  private createEp = inject(MealCreateEndpoint);
  private updateEp = inject(MealUpdateEndpoint);
  private getByIdEp = inject(MealGetByIdEndpoint);
  private uploadFileEp = inject(FileUploadEndpoint);
  private getListEp = inject(MealGetListEndpoint);
  private dialog = inject(MatDialog);
  private getInventoryItemsListEp = inject(InventoryItemListEndpoint);
  private getMealIngredientsEp = inject(MealGetIngredientsEndpoint);
  private getMealCategoriesList = inject(MealCategoryGetEndpoint)

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild('mealsInput') mealsInput!: ElementRef;

  inventoryItemsList = new MatTableDataSource<MealIngredientQueryDto>();
  columns: string[] = ['name', 'quantity', 'unitType', 'actions'];
  loading = false;

  constructor(
    private ref: MatDialogRef<MealsFormDialog>,
    @Inject(MAT_DIALOG_DATA) public data: DialogData
  ) {}

  form = this.fb.group({
    id: this.fb.control<number | null>(null),
    name: this.fb.control<string>('', {
      validators: [Validators.required],
      asyncValidators: [this.checkNameExists.bind(this)],
      nonNullable: true
    }),
    description: this.fb.control<string | null>(null),
    basePrice: this.fb.control<number>(0, { validators: [Validators.required], nonNullable: true }),
    isAvailable: this.fb.control<boolean | null>(null),
    isFeatured: this.fb.control<boolean | null>(null),
    imageField: this.fb.control<string | null>(null),
    stockManaged: this.fb.control<boolean | null>(null),
    categoryId: this.fb.control<number | null>(null),
    ingredients: this.fb.array<FormGroup<{
      inventoryItemId: FormControl<number>;
      inventoryItemName: FormControl<string>;
      unitType: FormControl<string>;
      quantity: FormControl<number>;
    }>>([])
  });

  ngOnInit(): void {
    if (this.data.mode === 'edit') {
      this.loadMealForEdit();
    }

  }



  mealCategories: MealCategory[] = [];









  get ingredientsArray(): FormArray {
    return this.form.get('ingredients') as FormArray;
  }

  get title() {
    return this.data.mode === 'create' ? 'Add Meal' : 'Edit Meal';
  }

  /* ---------------- Ingredients Handling ---------------- */

  private loadIngredients(): void {
    if (this.data.mode === 'edit') {
      this.getMealIngredientsEp.handleAsync(this.data.id).subscribe(result => {
        this.setIngredients(result);
      });
    }
  }

  private setIngredients(ingredients: MealIngredientQueryDto[]) {
    // Update FormArray
    this.ingredientsArray.clear();
    ingredients.forEach(ing => {
      this.ingredientsArray.push(
        this.fb.group({
          inventoryItemId: [ing.inventoryItemId, Validators.required],
          inventoryItemName: [ing.inventoryItemName, Validators.required],
          unitType: [ing.unitType ?? UnitTypes.Unit, Validators.required],
          quantity: [ing.quantity, [Validators.required, Validators.min(0.1)]]
        })
      );
    });

    // Update MatTable
    this.inventoryItemsList.data = ingredients.map(ing => ({
      inventoryItemId: ing.inventoryItemId,
      inventoryItemName: ing.inventoryItemName,
      unitType: ing.unitType,
      quantity: ing.quantity
    }));
  }

  addIngredient() {
    const ref = this.dialog.open(AddIngredientsDialog, {
      width: '800px',
      height: '680px',
      maxWidth: 'none',
      data: { selectedIngredientsList: this.inventoryItemsList.data }
    });

    ref.afterClosed().subscribe((result: addIngredientDto[] | false) => {
      if (!result) return;

      const ingredients: MealIngredientQueryDto[] = result.map(item => ({
        inventoryItemId: item.inventoryItemId,
        inventoryItemName: item.inventoryItemName,
        quantity: item.quantity ?? 0,
        unitType: mapUnitTypeToString(item.unitType)
      }));

      this.setIngredients(ingredients);
    });
  }

  removeIngredient(element: MealIngredientQueryDto) {
    const ref = this.dialog.open(ConfirmDialogComponent, {
      data: { title: 'Delete ingredient?', message: 'This cannot be undone.' }
    });

    ref.afterClosed().subscribe(ok => {
      if (!ok) return;

      const newIngredients = this.inventoryItemsList.data.filter(item => item !== element);
      this.setIngredients(newIngredients);
    });
  }

  onQuantityChange(index: number, value: string) {
    const qty = Number(value);
    if (!isNaN(qty) && qty >= 0) {
      const ing = this.ingredientsArray.at(index);
      ing.get('quantity')?.setValue(qty);
      this.inventoryItemsList.data[index].quantity = qty;
    }
  }

  onNameSearch(name: string) {
    if (!name) {
      this.inventoryItemsList.data = this.ingredientsArray.controls.map(ctrl => ({
        inventoryItemId: ctrl.get('inventoryItemId')!.value,
        inventoryItemName: ctrl.get('inventoryItemName')!.value,
        unitType: ctrl.get('unitType')!.value,
        quantity: ctrl.get('quantity')!.value
      }));
      return;
    }

    const lowerName = name.toLowerCase();
    this.inventoryItemsList.data = this.ingredientsArray.controls
      .map(ctrl => ({
        inventoryItemId: ctrl.get('inventoryItemId')!.value,
        inventoryItemName: ctrl.get('inventoryItemName')!.value,
        unitType: ctrl.get('unitType')!.value,
        quantity: ctrl.get('quantity')!.value
      }))
      .filter(item => item.inventoryItemName.toLowerCase().includes(lowerName));
  }

  /* ---------------- Save & Cancel ---------------- */

  cancel() {
    this.ref.close(false);
  }

  save() {
    if (this.form.invalid) return;
    this.loading = true;

    const raw = this.form.getRawValue();

    const mappedIngredients: MealIngredientApiDto[] = raw.ingredients.map(i => ({
      inventoryItemId: i.inventoryItemId,
      unitType: UnitTypes[i.unitType as keyof typeof UnitTypes], // convert string to enum
      quantity: i.quantity
    }));

    if (this.data.mode === 'create') {
      const body: CreateMealCommand = {
        name: raw.name.trim(),
        description: raw.description?.trim() ?? '',
        basePrice: raw.basePrice,
        isAvailable: raw.isAvailable ?? true,
        isFeatured: raw.isFeatured ?? false,
        imageField: raw.imageField?.trim(),
        stockManaged: raw.stockManaged ?? false,
        categoryId: raw.categoryId ?? 0,
        ingredients: mappedIngredients
      };

      this.createEp.handleAsync(body).pipe(
        finalize(() => this.loading = false)
      ).subscribe({ next: () => this.ref.close(true) });
    } else {
      const body: UpdateMealCommand = {
        id: raw.id!,
        name: raw.name.trim(),
        description: raw.description?.trim() ?? '',
        basePrice: raw.basePrice,
        isAvailable: raw.isAvailable ?? true,
        isFeatured: raw.isFeatured ?? false,
        imageField: raw.imageField?.trim(),
        stockManaged: raw.stockManaged ?? false,
        categoryId: raw.categoryId ?? 0,
        ingredients: mappedIngredients
      };

      this.updateEp.handleAsync(body).pipe(
        finalize(() => this.loading = false)
      ).subscribe({ next: () => this.ref.close(true) });
    }
  }

  onFileSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    if (!input.files || input.files.length === 0) return;

    const file = input.files[0];
    this.uploadFileEp.uploadFile(file).subscribe({
      next: res => this.form.patchValue({ imageField: res.url }),
      error: err => console.error('File upload failed', err)
    });
  }

  /* ---------------- Helpers ---------------- */

  checkNameExists(control: any) {
    const name = control.value?.trim();
    if (!name) return of(null);

    return this.getListEp.handleAsync().pipe(
      map((result: MealDto[] | { meals: MealDto[] }) => {
        const meals = Array.isArray(result) ? result : result.meals;
        const exists = meals.some(meal => {
          return this.data.mode === 'create'
            ? meal.name.toLowerCase() === name.toLowerCase()
            : meal.name.toLowerCase() === name.toLowerCase() && meal.id !== this.data.id;
        });
        return exists ? { nameExists: true } : null;
      })
    );
  }


  private loadMealForEdit() {
    this.loading = true;
    this.getMealCategoriesList.handleAsync().subscribe(categories => {
      this.mealCategories = categories;
      if (this.data.mode !== 'create') {
        this.getByIdEp.handleAsync(this.data.id).subscribe({
          next: (dto: GetMealByIdDto) => {
            console.log(dto);
            console.log(typeof dto.categoryId, dto.categoryId);
            this.form.patchValue({
              id: dto.id,
              name: dto.name,
              description: dto.description,
              basePrice: dto.basePrice,
              isAvailable: dto.isAvailable,
              isFeatured: dto.isFeatured,
              imageField: dto.imageField,
              stockManaged: dto.stockManaged,
              categoryId: dto.categoryId
            });

            if (dto.ingredients?.length) {
              const ingredients = dto.ingredients.map(ing => ({
                inventoryItemId: ing.inventoryItemId,
                inventoryItemName: ing.inventoryItemName,
                quantity: ing.quantity,
                unitType: ing.unitType
              }));
              this.setIngredients(ingredients);
            }

            this.loading = false;
          },
          error: () => (this.loading = false)
        });
      }
    });
  }

}
