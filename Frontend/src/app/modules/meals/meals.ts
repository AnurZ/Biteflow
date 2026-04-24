import { Component, OnInit, ViewChild, inject } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { MatPaginator, PageEvent } from '@angular/material/paginator';

import { MealsService } from './meals-service';
import { MealDto } from './meals-model';
import { MealDeleteEndpoint } from '../../endpoints/meals-crud-endpoints/meal-delete-endpoint';
import { MealsFormDialog } from './meals-form-dialog/meals-form-dialog';
import { ConfirmDialogComponent } from '../admin/staff/confirm-dialog/confirm-dialog-component';
import { FileGetEndpoint } from '../../endpoints/file-upload-endpoint/file-get-endpoint';
import { MealCategoryGetEndpoint } from '../../endpoints/meal-category-crud-endpoint/meal-category-get-endpoint';
import { MealCategoryGetByIdEndpoint } from '../../endpoints/meal-category-crud-endpoint/meal-category-get-by-id-endpoint';
import { map, Observable } from 'rxjs';
import {MealcategoryFormDialog} from './MealCategory-form-dialog/mealcategory-form-dialog';

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
  private getByIdMealCategoryEp = inject(MealCategoryGetByIdEndpoint);

  columns = [
    'image',
    'name',
    'category',
    'basePrice',
    'isAvailable',
    'isFeatured',
    'ingredientsCount',
    'actions'
  ];

  rows: MealDto[] = [];
  total = 0;

  pageNumber = 1;
  pageSize = 10;
  search = '';
  sort: string | undefined;

  selectedCategoryId: number = 0;
  currentSearchName: string = '';

  @ViewChild(MatPaginator) paginator!: MatPaginator;

  ngOnInit(): void {
    this.loadMeals();
    this.loadMealCategories();
  }

  /* ---------------- MAIN LOAD (UNIFIED) ---------------- */

  loadMeals() {

    this.mealsService.getMeals(
      this.pageNumber,
      this.pageSize,
      this.search,
      this.sort,
      this.selectedCategoryId
    ).subscribe(res => {
      this.rows = res.items ?? [];
      this.total = res.total ?? 0;
    });
  }

  /* ---------------- PAGING ---------------- */

  onPage(e: PageEvent) {
    this.pageNumber = e.pageIndex + 1;
    this.pageSize = e.pageSize;
    this.loadMeals();
  }

  /* ---------------- SEARCH ---------------- */

  onSearch(term: string) {
    this.search = term;
    this.pageNumber = 1;
    this.paginator?.firstPage();
    this.loadMeals();
  }


  /* ---------------- CATEGORY ---------------- */

  onCategoryChange(event: Event) {
    this.selectedCategoryId = +(event.target as HTMLSelectElement).value;
    this.pageNumber = 1;
    this.loadMeals();
  }

  loadMealCategories() {
    const picker = document.getElementById('mealCategoryPicker') as HTMLSelectElement;
    picker.innerHTML = '<option value="0">Any</option>';

    this.getMealCategoriesList.handleAsync().subscribe(categories => {
      for (const category of categories) {
        const option = document.createElement('option');
        option.value = category.id.toString();
        option.textContent = category.name;
        picker.appendChild(option);
      }
    });
  }

  /* ---------------- SORT FIXED ---------------- */

  onSort(key: string) {
    const map: any = {
      name: 'name',
      baseprice: 'baseprice',
      isavailable: 'isavailable',
      isfeatured: 'isfeatured',
      ingredientscount: 'ingredientscount',
      category: 'category'
    };

    const k = map[key];
    if (!k) return;

    if (this.sort === k) this.sort = `-${k}`;
    else if (this.sort === `-${k}`) this.sort = undefined;
    else this.sort = k;

    this.loadMeals();
  }

  /* ---------------- CRUD ---------------- */

  createMeal() {
    const ref = this.dialog.open(MealsFormDialog, {
      width: '1200px',
      height: '830px',
      maxWidth: 'none',
      data: { mode: 'create' }
    });

    ref.afterClosed().subscribe(r => r && this.loadMeals());
  }

  editMeal(id: number) {
    const ref = this.dialog.open(MealsFormDialog, {
      width: '1200px',
      height: '830px',
      maxWidth: 'none',
      data: { mode: 'edit', id }
    });

    ref.afterClosed().subscribe(r => r && this.loadMeals());
  }

  deleteMeal(id: number) {
    const ref = this.dialog.open(ConfirmDialogComponent, {
      data: { title: 'Delete meal?', message: 'This cannot be undone.' }
    });

    ref.afterClosed().subscribe(ok => {
      if (ok) {
        this.deleteEp.handleAsync(id).subscribe(() => this.loadMeals());
      }
    });
  }

  /* ---------------- CATEGORY CACHE ---------------- */

  categoryCache$: { [id: number]: Observable<string> } = {};

  getCategoryName$(categoryId: number): Observable<string> {
    if (!this.categoryCache$[categoryId]) {
      this.categoryCache$[categoryId] = this.getByIdMealCategoryEp
        .handleAsync(categoryId)
        .pipe(map(res => res.name));
    }
    return this.categoryCache$[categoryId];
  }

  /* ---------------- CATEGORY FORM ---------------- */

  addMealCategoryForm() {
    const ref = this.dialog.open(MealcategoryFormDialog, {
      width: '800px',
      maxWidth: '1000px',
      maxHeight: '600px'
    });

    ref.afterClosed().subscribe(() => {
      this.loadMeals();
      this.loadMealCategories();
    });
  }
}
