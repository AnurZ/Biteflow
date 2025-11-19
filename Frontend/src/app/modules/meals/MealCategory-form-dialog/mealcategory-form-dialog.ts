import {MealCategory, MealCategoryCreateDto, MealIngredientQueryDto} from '../meals-model';
import { Component, Inject, OnInit, inject, ViewChild, ElementRef } from '@angular/core';
import { FormArray, FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import {MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatPaginator } from '@angular/material/paginator';
import { MatTableDataSource } from '@angular/material/table';
import { finalize, map, of } from 'rxjs';
import { ConfirmDialogComponent } from '../../admin/staff/confirm-dialog/confirm-dialog-component';
import { NgModule } from '@angular/core';
import { CommonModule, CurrencyPipe } from '@angular/common';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule } from '@angular/material/paginator';
import { MatDialogModule } from '@angular/material/dialog';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import {MealGetByIdEndpoint} from '../../../endpoints/meals-crud-endpoints/meal-getbyid-endpoint';
import {
  MealCategoryGetByIdEndpoint
} from '../../../endpoints/meal-category-crud-endpoint/meal-category-get-by-id-endpoint';
import {MealCategoryGetEndpoint} from '../../../endpoints/meal-category-crud-endpoint/meal-category-get-endpoint';
import {MealCategoryDeleteEndpoint} from '../../../endpoints/meal-category-crud-endpoint/meal-category-delete-endpoint';
import {MatDialog} from '@angular/material/dialog';
import {AddIngredientsDialog} from '../meals-form-dialog/add-ingredients-dialog/add-ingredients-dialog';
import {MealcategoryAddEditDialog} from './mealcategory-add-edit-dialog/mealcategory-add-edit-dialog';
@Component({
  selector: 'app-mealcategory-form-dialog',
  standalone: false,
  templateUrl: './mealcategory-form-dialog.html',
  styleUrl: './mealcategory-form-dialog.css'
})
export class MealcategoryFormDialog implements OnInit {
  private MealCategoryGetListEndpoint = inject(MealCategoryGetEndpoint);
  private MealCategoryDeleteEndpoint = inject(MealCategoryDeleteEndpoint);
  private dialog = inject(MatDialog);

  private ref = inject(MatDialogRef<MealcategoryFormDialog>);


  loading = false;

  columns = ['name', 'description', 'actions'];
  mealCategoriesList:MealCategory[] = [];

  openAddEditCategoryDialog(elementId?: number) {
    const ref = this.dialog.open(MealcategoryAddEditDialog, {
      width: '500px',
      height: '680px',
      maxWidth: 'none',
      disableClose: true,
      data: elementId
        ? { mode: 'edit', id: elementId }
        : { mode: 'create' }
    });
    ref.afterClosed().subscribe(result => result && this.loadMealCategories());
  }


  deleteMealCategory(id:number) {
    const ref = this.dialog.open(ConfirmDialogComponent, {
      data: { title: 'Delete category?', message: 'This cannot be undone.' }
    });

    ref.afterClosed().subscribe(ok => {
      if (!ok) return;
      this.MealCategoryDeleteEndpoint.handleAsync(id)
        .subscribe(() => this.loadMealCategories())
      });
  }



  loadMealCategories() {
    this.loading = true;
    this.MealCategoryGetListEndpoint.handleAsync().pipe(
      finalize(() => this.loading = false)
    ).subscribe(result => {
      this.mealCategoriesList = result;
    });
  }

  cancel() {
    this.ref.close(false); // or just this.ref.close() if you want undefined
  }

  ngOnInit() {
    this.loadMealCategories();
  }

}

