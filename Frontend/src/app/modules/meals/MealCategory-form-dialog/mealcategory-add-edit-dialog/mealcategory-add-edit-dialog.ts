import {Component, inject, Inject, OnInit} from '@angular/core';
import {MAT_DIALOG_DATA, MatDialogRef} from '@angular/material/dialog';
import {
  MealCategoryGetByIdEndpoint
} from '../../../../endpoints/meal-category-crud-endpoint/meal-category-get-by-id-endpoint';
import {
  MealCategoryCreateEndpoint
} from '../../../../endpoints/meal-category-crud-endpoint/meal-category-create-endpoint';
import {
  MealCategoryUpdateEndpoint
} from '../../../../endpoints/meal-category-crud-endpoint/meal-category-update-endpoint';
import {FormBuilder, Validators} from '@angular/forms';
import {map, of} from 'rxjs';
import {MealCategory, MealDto} from '../../meals-model';
import {MealCategoryGetEndpoint} from '../../../../endpoints/meal-category-crud-endpoint/meal-category-get-endpoint';


type DialogData = { mode: 'create' } | { mode: 'edit'; id: number };

@Component({
  selector: 'app-mealcategory-add-edit-dialog',
  standalone: false,
  templateUrl: './mealcategory-add-edit-dialog.html',
  styleUrl: './mealcategory-add-edit-dialog.css'
})
export class MealcategoryAddEditDialog implements OnInit {
  private getByIdMealCategoryEP = inject(MealCategoryGetByIdEndpoint);
  private getListMealCategoryEP = inject(MealCategoryGetEndpoint)
  private createMealCategoryEP = inject(MealCategoryCreateEndpoint);
  private updateMealCategoryEP = inject(MealCategoryUpdateEndpoint);
  private fb = inject(FormBuilder);

  loading = false;
  constructor(
    private ref: MatDialogRef<MealcategoryAddEditDialog>,
    @Inject(MAT_DIALOG_DATA) public data: { mode: string; id: number }
  ) {

  }

  form = this.fb.group({
    name: this.fb.control<string>('', {
      validators: [Validators.required],
      asyncValidators: [this.checkNameExists.bind(this)],
      nonNullable: true
    }),
    description: this.fb.control<string | null>(null),
  })

  checkNameExists(control: any) {
    const name = control.value?.trim();
    if (!name) return of(null);

    return this.getListMealCategoryEP.handleAsync().pipe(
      map((result: MealCategory[] | { mealCategories: MealCategory[] }) => {
        const mealCategories = Array.isArray(result) ? result : result.mealCategories;
        const exists = mealCategories.some(mealCategory => {
          return this.data.mode === 'create'
            ? mealCategory.name.toLowerCase() === name.toLowerCase()
            : mealCategory.name.toLowerCase() === name.toLowerCase() && mealCategory.id !== this.data.id;
        });
        return exists ? { nameExists: true } : null;
      })
    );
  }

  getTitle():string{
    return this.data.mode == 'create' ? 'Add New Meal' : 'Edit Meal';
  }

  cancel() {
    this.ref.close(false);
  }

  save() {
    if (this.form.invalid) return;

    this.loading = true;

    const raw = this.form.getRawValue();

    const mealCategoryEntity: MealCategory = {
      id: this.data.id,
      name: raw.name!,
      description: raw.description?.trim() || ''
    };

    if (this.data.mode === 'create') {

      this.createMealCategoryEP.handleAsync(mealCategoryEntity)
        .subscribe({
          next: () => {
            this.loading = false;
            this.ref.close(true); // return success to parent
          },
          error: () => {
            this.loading = false;
          }
        });

    } else {
      this.updateMealCategoryEP.handleAsync(mealCategoryEntity)
        .subscribe({
          next: () => {
            this.loading = false;
            this.ref.close(true);
          },
          error: () => {
            this.loading = false;
          }
        });
    }
  }

  ngOnInit(): void {
    if(this.data.mode == 'edit') {
      this.getByIdMealCategoryEP.handleAsync(this.data.id).subscribe(response => {
        this.form.patchValue({
          name: response.name,
          description: response.description
        });
      })
    }
  }

}
