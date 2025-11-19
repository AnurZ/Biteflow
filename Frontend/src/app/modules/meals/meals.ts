import { Component, OnInit, ViewChild, inject } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { MatPaginator, PageEvent } from '@angular/material/paginator';

import { MealsService } from './meals-service';
import {GetMealByNameDto, MealDto, PageResult} from './meals-model';
import {MealDeleteEndpoint} from '../../endpoints/meals-crud-endpoints/meal-delete-endpoint';
import {MealsFormDialog} from './meals-form-dialog/meals-form-dialog';
import {ConfirmDialogComponent} from '../admin/staff/confirm-dialog/confirm-dialog-component';
import {FormBuilder} from '@angular/forms';
import {map, Observable, of} from 'rxjs';
import {FileGetEndpoint} from '../../endpoints/file-upload-endpoint/file-get-endpoint';
import {MealCategoryGetEndpoint} from '../../endpoints/meal-category-crud-endpoint/meal-category-get-endpoint';
import {MealcategoryFormDialog} from './MealCategory-form-dialog/mealcategory-form-dialog';
import {
  MealCategoryGetByIdEndpoint
} from '../../endpoints/meal-category-crud-endpoint/meal-category-get-by-id-endpoint';
//import { MealFormDialogComponent } from './meal-form-dialog/meal-form-dialog.component';
//import { ConfirmDialogComponent } from './confirm-dialog/confirm-dialog.component';


@Component({
  selector: 'app-meals',
  templateUrl: './meals.html',
  styleUrls: ['./meals.css'],
  standalone: false
})
export class Meals implements OnInit {
  private fileGetEp = inject(FileGetEndpoint);
  private mealsService = inject(MealsService);
  private dialog = inject(MatDialog);
  private deleteEp = inject(MealDeleteEndpoint);
  private getMealCategoriesList = inject(MealCategoryGetEndpoint);
  private getByIdMealCategoryEp = inject(MealCategoryGetByIdEndpoint)

  columns = ['image', 'name', 'category', 'basePrice', 'isAvailable', 'isFeatured', 'ingredientsCount', 'actions'];

  rows: MealDto[] = [];
  total = 0;

  pageNumber = 1;
  pageSize = 10;
  search = '';
  sort: string | undefined;


  @ViewChild(MatPaginator) paginator!: MatPaginator;

  ngOnInit(): void {
    this.loadMeals();
    this.loadMealCategories();
  }

  addMealCategoryForm(){
    const ref = this.dialog.open(MealcategoryFormDialog, {
      width: '800px',
      maxWidth: '1000px',
      maxHeight: '600px'
    });
    ref.afterClosed().subscribe(() => {
      // This runs whether backdrop clicked or dialog closed programmatically
      this.loadMeals();
      this.loadMealCategories();
    });}

  loadMealCategories() {
    const picker = document.getElementById('mealCategoryPicker') as HTMLSelectElement;

    // Clear existing options
    picker.innerHTML = '<option value="0">Any</option>';

    this.getMealCategoriesList.handleAsync().subscribe(categories => {
      console.log(categories);

      for (const category of categories) {
        const option = document.createElement('option');
        option.value = category.id.toString();
        option.textContent = category.name;
        picker.appendChild(option);
      }
    });
  }



  loadMeals() {
    this.mealsService.getMeals().subscribe(res => {
      console.log(res);
      this.rows = res;
      this.total = res.length;
    });

  }


  onPage(e: PageEvent) {
    this.pageNumber = e.pageIndex + 1;
    this.pageSize = e.pageSize;
    this.loadMeals();
  }

  onSearch(term: string) {
    this.search = term;
    this.pageNumber = 1;
    this.loadMeals();
  }


  loadMealsByName(name: string) {
    const trimmedName = name.trim();

    this.mealsService.getMeals().subscribe((res: MealDto[]) => {
      let meals = res || [];

      // filter by name
      if (trimmedName) {
        meals = meals.filter((m: MealDto) =>
          m.name.toLowerCase().includes(trimmedName.toLowerCase())
        );
      }

      // filter by category
      if (this.selectedCategoryId && this.selectedCategoryId !== 0) {
        meals = meals.filter((m: MealDto) => m.categoryId === this.selectedCategoryId);
      }

      // map to add ingredientsCount
      this.rows = meals.map((meal: MealDto) => ({
        ...meal,
        ingredientsCount: meal.ingredientsCount || 0
      }));

      this.total = this.rows.length;
    });
  }






  onSort(key: string) {
    if (!this.sort || !this.sort.includes(key)) this.sort = key;
    else if (this.sort === key) this.sort = `-${key}`;
    else this.sort = undefined;
    this.loadMeals();
  }

  createMeal() {
    const ref = this.dialog.open(MealsFormDialog, {
      width: '1200px',       // desired width
      height: '800px',
      maxWidth: 'none',      // remove Angular Material's default max-width
      data: { mode: 'create' }
    });
    ref.afterClosed().subscribe(result => result && this.loadMeals());
  }

  editMeal(id: number) {
    const ref = this.dialog.open(MealsFormDialog, {
      width: '1200px',
      height: '800px',
      maxWidth: 'none',
      data: { mode: 'edit', id: id }
    });
    ref.afterClosed().subscribe(result => result && this.loadMeals());
  }

  categoryCache$: { [id: number]: Observable<string> } = {};

  getCategoryName$(categoryId: number): Observable<string> {
    if (!this.categoryCache$[categoryId]) {
      this.categoryCache$[categoryId] = this.getByIdMealCategoryEp.handleAsync(categoryId).pipe(
        map(res => res.name)
      );
    }
    return this.categoryCache$[categoryId];
  }

  selectedCategoryId: number = 0;

  currentSearchName: string = '';

  onNameSearch(name: string) {
    this.currentSearchName = name;
    this.loadMealsByName(name);
  }
  onCategoryChange(event: Event) {
    this.selectedCategoryId = +(event.target as HTMLSelectElement).value;
    this.loadMealsByName(this.currentSearchName || '');
  }





  deleteMeal(id: number) {
    const ref = this.dialog.open(ConfirmDialogComponent, {
      data: { title: 'Delete meal?', message: 'This cannot be undone.' }
    });
    ref.afterClosed().subscribe(ok => {
      if (ok) this.deleteEp.handleAsync(id).subscribe(() => this.loadMeals());
    });
  }






  //createMeal() {
  //const ref = this.dialog.open(MealFormDialogComponent, { width: '720px', data: { mode: 'create' } });
  //ref.afterClosed().subscribe(changed => changed && this.loadMeals());
  //}

  //editMeal(id: number) {
  //const ref = this.dialog.open(MealFormDialogComponent, { width: '720px', data: { mode: 'edit', id } });
  //ref.afterClosed().subscribe(changed => changed && this.loadMeals());
  //}

  //deleteMeal(id: number) {
  //const ref = this.dialog.open(ConfirmDialogComponent, {
  //  data: { title: 'Delete meal?', message: 'This cannot be undone.' }
  //});
  //ref.afterClosed().subscribe(ok => {
  //  if (ok) this.mealsService.deleteMeal(id).subscribe(() => this.loadMeals());
  //});
  //}
}
