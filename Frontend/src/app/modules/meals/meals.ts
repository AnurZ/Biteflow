import { Component, OnInit, ViewChild, inject } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { MatPaginator, PageEvent } from '@angular/material/paginator';

import { MealsService } from './meals-service';
import { MealDto, MealCategory } from './meals-model';
import { MealDeleteEndpoint } from '../../endpoints/meals-crud-endpoints/meal-delete-endpoint';
import { MealsFormDialog } from './meals-form-dialog/meals-form-dialog';
import { ConfirmDialogComponent } from '../admin/staff/confirm-dialog/confirm-dialog-component';
import { MealCategoryGetEndpoint } from '../../endpoints/meal-category-crud-endpoint/meal-category-get-endpoint';
import { CurrencyService } from '../../services/currency-service/currency.service';
import { MealcategoryFormDialog } from './MealCategory-form-dialog/mealcategory-form-dialog';

@Component({
  selector: 'app-meals',
  templateUrl: './meals.html',
  styleUrls: ['./meals.css'],
  standalone: false
})
export class Meals implements OnInit {
  private mealsService = inject(MealsService);
  private dialog = inject(MatDialog);
  private deleteEp = inject(MealDeleteEndpoint);
  private getMealCategoriesList = inject(MealCategoryGetEndpoint);

  currencyService = inject(CurrencyService);

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

  mealCategories: MealCategory[] = [];
  rows: MealDto[] = [];
  total = 0;

  pageNumber = 1;
  pageSize = 10;
  search = '';
  sort: string | undefined;

  selectedCategoryId = 0;
  selectedIsAvailable: boolean | null = null;

  minPrice: number | null = null;
  maxPrice: number | null = null;

  @ViewChild(MatPaginator) paginator!: MatPaginator;

  ngOnInit(): void {
    this.loadMeals();
    this.loadMealCategories();
  }

  loadMeals(): void {
    this.mealsService.getMeals(
      this.pageNumber,
      this.pageSize,
      this.search,
      this.sort,
      this.selectedCategoryId,
      this.minPrice ?? undefined,
      this.maxPrice ?? undefined,
      this.selectedIsAvailable ?? undefined
    ).subscribe(res => {
      this.rows = res.items ?? [];
      this.total = res.total ?? 0;
    });
  }

  onPage(e: PageEvent): void {
    this.pageNumber = e.pageIndex + 1;
    this.pageSize = e.pageSize;
    this.loadMeals();
  }

  onSearch(term: string): void {
    this.search = term;
    this.pageNumber = 1;
    this.paginator?.firstPage();
    this.loadMeals();
  }

  onCategoryChange(categoryId: number): void {
    this.selectedCategoryId = categoryId;
    this.pageNumber = 1;
    this.loadMeals();
  }

  onAvailabilityChange(value: boolean | null): void {
    this.selectedIsAvailable = value;
    this.pageNumber = 1;
    this.loadMeals();
  }

  onPriceChange(): void {
    this.pageNumber = 1;
    this.loadMeals();
  }

  loadMealCategories(): void {
    this.getMealCategoriesList.handleAsync().subscribe(categories => {
      this.mealCategories = categories;
    });
  }

  onSort(key: string): void {
    if (this.sort === key) {
      this.sort = `-${key}`;
    } else if (this.sort === `-${key}`) {
      this.sort = undefined;
    } else {
      this.sort = key;
    }

    this.loadMeals();
  }

  createMeal(): void {
    const ref = this.dialog.open(MealsFormDialog, {
      width: '1200px',
      height: '830px',
      maxWidth: 'none',
      data: { mode: 'create' }
    });

    ref.afterClosed().subscribe(result => {
      if (result) {
        this.loadMeals();
      }
    });
  }

  editMeal(id: number): void {
    const ref = this.dialog.open(MealsFormDialog, {
      width: '1200px',
      height: '830px',
      maxWidth: 'none',
      data: { mode: 'edit', id }
    });

    ref.afterClosed().subscribe(result => {
      if (result) {
        this.loadMeals();
      }
    });
  }

  deleteMeal(id: number): void {
    const ref = this.dialog.open(ConfirmDialogComponent, {
      data: {
        title: 'Delete meal?',
        message: 'This cannot be undone.'
      }
    });

    ref.afterClosed().subscribe(ok => {
      if (!ok) {
        return;
      }

      this.deleteEp.handleAsync(id).subscribe(() => {
        this.loadMeals();
      });
    });
  }

  addMealCategoryForm(): void {
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
