import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Meals } from './meals';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule } from '@angular/material/paginator';
import { MatButtonModule } from '@angular/material/button';
import {FormsModule, ReactiveFormsModule} from '@angular/forms';
import {MealsRoutingModule} from './meals-routing.module';
import {MatIconModule} from '@angular/material/icon';
import {MealsFormDialog} from './meals-form-dialog/meals-form-dialog';
import {MatOption, MatSelect, MatSelectModule} from '@angular/material/select';
import { MatOptionModule } from '@angular/material/core';
import { MealcategoryFormDialog} from './MealCategory-form-dialog/mealcategory-form-dialog';
import {MatDialogActions, MatDialogContent, MatDialogTitle} from '@angular/material/dialog';
import { MealcategoryAddEditDialog } from './MealCategory-form-dialog/mealcategory-add-edit-dialog/mealcategory-add-edit-dialog';

// for mat-option

@NgModule({
  declarations: [
    Meals,
    MealcategoryAddEditDialog,
  ],
  imports: [
    CommonModule,
    FormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatTableModule,
    MatPaginatorModule,
    MatButtonModule,
    MealsRoutingModule,
    CommonModule,
    MatIconModule,
    MatOption,
    MatSelect,
    ReactiveFormsModule,
    MatDialogContent,
    MatDialogTitle,
    MatDialogActions
  ]
})
export class MealsModule {}
