import { Component, Inject, OnInit, inject } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { map, of } from 'rxjs';

import { MealCategory } from '../../meals-model';
import { MealCategoryGetByIdEndpoint } from '../../../../endpoints/meal-category-crud-endpoint/meal-category-get-by-id-endpoint';
import { MealCategoryGetEndpoint } from '../../../../endpoints/meal-category-crud-endpoint/meal-category-get-endpoint';
import { MealCategoryCreateEndpoint } from '../../../../endpoints/meal-category-crud-endpoint/meal-category-create-endpoint';
import { MealCategoryUpdateEndpoint } from '../../../../endpoints/meal-category-crud-endpoint/meal-category-update-endpoint';

type DialogData =
  | { mode: 'create' }
  | { mode: 'edit'; id: number };

@Component({
  selector: 'app-mealcategory-add-edit-dialog',
  standalone: false,
  templateUrl: './mealcategory-add-edit-dialog.html',
  styleUrl: './mealcategory-add-edit-dialog.css'
})
export class MealcategoryAddEditDialog implements OnInit {
  private getByIdMealCategoryEP = inject(MealCategoryGetByIdEndpoint);
  private getListMealCategoryEP = inject(MealCategoryGetEndpoint);
  private createMealCategoryEP = inject(MealCategoryCreateEndpoint);
  private updateMealCategoryEP = inject(MealCategoryUpdateEndpoint);
  private fb = inject(FormBuilder);

  loading = false;

  constructor(
    private ref: MatDialogRef<MealcategoryAddEditDialog>,
    @Inject(MAT_DIALOG_DATA) public data: DialogData
  ) {}

  form = this.fb.group({
    name: this.fb.control('', {
      nonNullable: true,
      validators: [Validators.required],
      asyncValidators: [this.checkNameExists.bind(this)]
    }),
    description: this.fb.control<string | null>(null)
  });

  getTitle(): string {
    return this.data.mode === 'create'
      ? 'Add Category'
      : 'Edit Category';
  }

  checkNameExists(control: { value: string }) {
    const name = control.value?.trim();

    if (!name) {
      return of(null);
    }

    return this.getListMealCategoryEP.handleAsync().pipe(
      map((mealCategories: MealCategory[]) => {
        const exists = mealCategories.some(category =>
          category.name.toLowerCase() === name.toLowerCase() &&
          (this.data.mode === 'create' || category.id !== this.data.id)
        );

        return exists ? { nameExists: true } : null;
      })
    );
  }

  ngOnInit(): void {
    if (this.data.mode !== 'edit') {
      return;
    }

    this.loading = true;

    this.getByIdMealCategoryEP.handleAsync(this.data.id).subscribe({
      next: response => {
        this.form.patchValue({
          name: response.name,
          description: response.description
        });

        this.loading = false;
      },
      error: () => {
        this.loading = false;
      }
    });
  }

  cancel(): void {
    this.ref.close(false);
  }

  save(): void {
    if (this.form.invalid || this.loading) {
      return;
    }

    this.loading = true;

    const raw = this.form.getRawValue();

    const category: MealCategory = {
      id: this.data.mode === 'edit' ? this.data.id : 0,
      name: raw.name.trim(),
      description: raw.description?.trim() ?? ''
    };

    if (this.data.mode === 'create') {
      this.createMealCategoryEP.handleAsync(category).subscribe({
        next: () => {
          this.loading = false;
          this.ref.close(true);
        },
        error: () => {
          this.loading = false;
        }
      });

      return;
    }

    this.updateMealCategoryEP.handleAsync(category).subscribe({
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
