import {Component, Inject, OnInit, inject, ViewChild, NgModule, ElementRef} from '@angular/core';
import {FormArray, FormBuilder, FormControl, FormGroup, ReactiveFormsModule, Validators} from '@angular/forms';
import { MealCreateEndpoint } from '../../../endpoints/meals-crud-endpoints/meal-create-endpoint';
import { MealUpdateEndpoint } from '../../../endpoints/meals-crud-endpoints/meal-update-endpoint';
import { MealGetByIdEndpoint } from '../../../endpoints/meals-crud-endpoints/meal-getbyid-endpoint';
import { FileUploadEndpoint } from '../../../endpoints/file-upload-endpoint/file-upload-endpoint';
import {MAT_DIALOG_DATA, MatDialog, MatDialogModule, MatDialogRef} from '@angular/material/dialog';
import {
  addIngredientDto,
  CreateMealCommand,
  GetMealByIdDto,
  MealDto, MealIngredientApiDto,
  MealIngredientDto,
  MealIngredientQueryDto, UnitTypes,
  UpdateMealCommand
} from '../meals-model';
import {MealGetListEndpoint} from '../../../endpoints/meals-crud-endpoints/meal-getlist-endpoint';
import {finalize, map, of} from 'rxjs';
import {AddIngredientsDialog} from './add-ingredients-dialog/add-ingredients-dialog';
import {MealGetIngredientsEndpoint} from '../../../endpoints/meals-crud-endpoints/meal-getingredients-endpoint';
import {ListInventoryItemsDto} from '../../inventory-items/inventory-item-model';
import {
  InventoryItemListEndpoint
} from '../../../endpoints/inventory-items-crud-endpoints/inventory-item-list-endpoint';
import {MatTableDataSource} from '@angular/material/table';
import {MatPaginator, MatPaginatorModule} from '@angular/material/paginator';
import {MatTableModule} from '@angular/material/table';
import {MatTabsModule} from '@angular/material/tabs';
import {CommonModule} from '@angular/common';
import {MatFormFieldModule} from '@angular/material/form-field';
import {MatInputModule} from '@angular/material/input';
import {MatButtonModule} from '@angular/material/button';
import {MatCheckboxModule} from '@angular/material/checkbox';
import {mapUnitTypeToNumber, mapUnitTypeToString} from '../../../helper/unit-type-helper';
import {ConfirmDialogComponent} from '../../admin/staff/confirm-dialog/confirm-dialog-component';

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

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild('mealsInput') mealsInput!: ElementRef;

  constructor(
    private ref: MatDialogRef<MealsFormDialog>,
    @Inject(MAT_DIALOG_DATA) public data: DialogData
  ) {}

  ingredientsTemp: MealIngredientQueryDto[] = [];
  listIngredients: MealIngredientQueryDto[] = [];
  ingredientsCopy: MealIngredientQueryDto[] = [];

  inventoryItemsList = new MatTableDataSource<MealIngredientQueryDto>();
  columns: string[] = [
    'name',
    'quantity',
    'unitType',
    'actions'
  ];




  getIngredientControl(i: number): FormControl {
    const control = this.ingredientsArray.at(i)?.get('quantity');
    return control instanceof FormControl ? control : this.fb.control(0);
  }


  onQuantityChange(index: number, value: string) {
    console.log(`Ingredient #${index} changed to:`, value);
    this.inventoryItemsList.data[index].quantity = Number(value);
  }

  removeIngredient(element: MealIngredientQueryDto) {
    const ref = this.dialog.open(ConfirmDialogComponent, {
      data: { title: 'Delete ingredient?', message: 'This cannot be undone.' }
    });

    ref.afterClosed().subscribe(ok => {
      if (!ok) return;

      // 1️⃣ Remove from displayed data
      this.inventoryItemsList.data = this.inventoryItemsList.data.filter(
        item => item !== element
      );

      // 2️⃣ Remove from FormArray
      const ingredientsArray = this.form.get('ingredients') as FormArray;
      const index = ingredientsArray.value.findIndex(
        (ing: any) => ing.inventoryItemId === element.inventoryItemId
      );
      if (index !== -1) {
        ingredientsArray.removeAt(index);
      }

      // 3️⃣ Update temporary backup
      this.ingredientsTemp = this.ingredientsTemp.filter(
        item => item !== element
      )
    });
  }



  get ingredientsArray(): FormArray {
    return this.form.get('ingredients') as FormArray;
  }

  addIngredient(mealId?: number | null) {
    console.log('AAAAAAAAAAAAA');
    console.log(mealId);

    this.mealsInput.nativeElement.value = '';

    const data: any = {
      mode: this.data.mode
    };

    // Only pass id if we're in 'edit' mode
    if (mealId !== undefined) {
      data.id = mealId;
      data.mode = 'edit';
    }
    else{
      data.mode = 'create';
    }

    this.inventoryItemsList.data = this.ingredientsTemp;
    this.ingredientsCopy = this.inventoryItemsList.data;

    const ref = this.dialog.open(AddIngredientsDialog, {
      width: '800px',
      height: '680px',
      maxWidth: 'none',
      data: {mode:data.mode, id: mealId, selectedIngredientsList: this.inventoryItemsList.data},
    });

    ref.afterClosed().subscribe((result: addIngredientDto[] | false) => {
      if (result) {
        // Update the data array
        this.inventoryItemsList.data = result.map((item: addIngredientDto) => ({
          inventoryItemId: item.inventoryItemId,
          inventoryItemName: item.inventoryItemName,
          quantity: item.quantity ?? 0,
          unitType: mapUnitTypeToString(item.unitType)
        }));

        // Rebuild the FormArray
        this.ingredientsArray.clear();
        this.inventoryItemsList.data.forEach(ing => {
          this.ingredientsArray.push(
            this.fb.group({
              inventoryItemId: [ing.inventoryItemId, Validators.required],
              unitType: [ing.unitType ?? UnitTypes.Unit, Validators.required],
              quantity: [ing.quantity, [Validators.required, Validators.min(0.1)]]
            })
          );
        });
      } else {
        this.inventoryItemsList.data = [...this.ingredientsCopy];
      }

      // If you haven't done so already, initialize a backup of the full data

        this.ingredientsTemp = [...this.inventoryItemsList.data];



    });

  }



  loadIngredients(): void {
    if (this.data.mode === 'edit') {
      this.getMealIngredientsEp.handleAsync(this.data.id).subscribe((result) => {
        // result IS the array of ingredients
        this.listIngredients = result;

        // ✅ Show in the MatTable
        this.inventoryItemsList.data = result;

        // ✅ Update the form as well
        const ingredientsArray = this.form.get('ingredients') as FormArray;
        ingredientsArray.clear();

        for (const ing of result) {
          ingredientsArray.push(this.fb.group({
            inventoryItemId: [ing.inventoryItemId, Validators.required],
            unitType: [ing.unitType ?? UnitTypes.Unit, Validators.required],
            quantity: [ing.quantity, [Validators.required, Validators.min(0.1)]]
          }));
        }

        console.log('Loaded ingredients:', result);
        // If you haven't done so already, initialize a backup of the full data

          this.ingredientsTemp = [...this.inventoryItemsList.data];


      });
    }
  }

  onNameSearch(name: string) {
    if (!name) {
      this.inventoryItemsList.data = [...this.ingredientsTemp];
      return;
    }

    const lowerName = name.toLowerCase();

    this.inventoryItemsList.data = this.ingredientsTemp.filter(
      (item: MealIngredientQueryDto) =>
        item.inventoryItemName.toLowerCase().includes(lowerName)
    );
  }



  checkNameExists(control: any) {
    const name = control.value?.trim();
    if (!name) {
      return of(null); // skip empty
    }

    return this.getListEp.handleAsync().pipe(
      map((result: MealDto[] | { meals: MealDto[] }) => {
        const meals = Array.isArray(result) ? result : result.meals;


        const exists = meals.some((meal: MealDto) => {
          if (this.data.mode === 'create') {
            // In create mode, check all meals
            return meal.name.toLowerCase() === name.toLowerCase();
          } else {
            // In edit mode, skip the current meal
            return meal.name.toLowerCase() === name.toLowerCase() && meal.id !== this.data.id;
          }
        });




        return exists ? { nameExists: true } : null;
      })
    );
  }



  get title() {
    return this.data.mode === 'create' ? 'Add Meal' : 'Edit Meal';
  }

  loading = false;

  form = this.fb.group({
    id: this.fb.control<number | null>(null),
    name: this.fb.control<string>('', { validators: [Validators.required], asyncValidators:[this.checkNameExists.bind(this)], nonNullable: true }),
    description: this.fb.control<string | null>(null),
    basePrice: this.fb.control<number>(0, { validators: [Validators.required], nonNullable: true }),
    isAvailable: this.fb.control<boolean | null>(null),
    isFeatured: this.fb.control<boolean | null>(null),
    imageField: this.fb.control<string>('', { validators: [Validators.required], nonNullable: true }),
    ingredients: this.fb.array<FormGroup<{
      inventoryItemId: FormControl<number>;
      unitType: FormControl<string>;
      quantity: FormControl<number>;
    }>>([]),
    ingredientsCount: this.fb.control<number | null>(null),
    stockManaged: this.fb.control<boolean | null>(null),

  });

  cancel() {
    this.ref.close(false);
  }

  lookupIngredientName(id: number): string {
    const item = this.inventoryItemsList.data?.find(x => x.inventoryItemId === id);
    return item ? item.inventoryItemName : '';
  }


  ngOnInit(): void {
    if (this.data.mode === 'edit') {
      this.loading = true;

      this.getByIdEp.handleAsync(this.data.id).subscribe({
        next: (dto: UpdateMealCommand) => {
          console.log("updated meal", dto);

          this.form.patchValue({
            id: dto.id,
            name: dto.name,
            description: dto.description,
            basePrice: dto.basePrice,
            isAvailable: dto.isAvailable,
            isFeatured: dto.isFeatured,
            imageField: dto.imageField,
            stockManaged: dto.stockManaged
          });


          if (dto.ingredients?.length) {
            const ingredientsArray = this.form.get('ingredients') as FormArray;
            ingredientsArray.clear();

            // Map API DTO to frontend DTO for FormArray
            dto.ingredients.forEach(ing => {
              const ingredientName = this.lookupIngredientName(ing.inventoryItemId); // you need a way to get name
              ingredientsArray.push(
                this.fb.group({
                  inventoryItemId: [ing.inventoryItemId, Validators.required],
                  unitType: [ing.unitType ?? UnitTypes.Unit, Validators.required],
                  quantity: [ing.quantity, [Validators.required, Validators.min(0.1)]]
                })
              );
            });


            // Map API DTO to display DTO for MatTable
            this.inventoryItemsList.data = dto.ingredients.map(ing => ({
              inventoryItemId: ing.inventoryItemId,
              inventoryItemName: this.lookupIngredientName(ing.inventoryItemId),
              unitType: mapUnitTypeToString(ing.unitType),
              quantity: ing.quantity
            }));
          }

          this.loading = false;
        },
        error: () => (this.loading = false)
      });
    }

    this.loadIngredients();
  }




  /** Helper to create a FormGroup for an ingredient */




  save() {
    if (this.form.invalid) return;
    this.loading = true;

    const raw = this.form.getRawValue();

    const mappedIngredients: MealIngredientApiDto[] = raw.ingredients.map(i => ({
      inventoryItemId: i.inventoryItemId,
      unitType: mapUnitTypeToNumber(i.unitType), // already a number
      quantity: i.quantity
    }));



    if (this.data.mode === 'create') {
      const body: CreateMealCommand = {
        name: raw.name.trim(),
        description: raw.description?.trim() ?? '',
        basePrice: raw.basePrice,
        isAvailable: raw.isAvailable ?? true,
        isFeatured: raw.isFeatured ?? false,
        imageField: raw.imageField.trim(),
        stockManaged: raw.stockManaged ?? false,
        ingredients: mappedIngredients,
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
        imageField: raw.imageField.trim(),
        stockManaged: raw.stockManaged ?? false,
        ingredients: mappedIngredients
      };

      this.updateEp.handleAsync(body).subscribe({
        next: () => this.ref.close(true),
        error: () => (this.loading = false)
      });
    }
  }


  onFileSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    if (!input.files || input.files.length === 0) return;

    const file = input.files[0];

    // Upload or preview
    this.uploadFileEp.uploadFile(file).subscribe({
      next: (res) => this.form.patchValue({ imageField: res.url }),
      error: (err) => console.error('File upload failed', err)
    });
  }




  protected readonly UnitTypes = UnitTypes;
}
