import { Component, OnInit, ViewChild, inject } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { MatPaginator, PageEvent } from '@angular/material/paginator';

import { MealsService } from './meals-service';
import {GetMealByNameDto, MealDto, PageResult} from './meals-model';
//import { MealFormDialogComponent } from './meal-form-dialog/meal-form-dialog.component';
//import { ConfirmDialogComponent } from './confirm-dialog/confirm-dialog.component';

@Component({
  selector: 'app-meals',
  templateUrl: './meals.html',
  styleUrls: ['./meals.css'],
  standalone: false
})
export class Meals implements OnInit {
  private mealsService = inject(MealsService);
  private dialog = inject(MatDialog);

  columns = ['name', 'description', 'basePrice', 'isAvailable', 'isFeatured', 'ingredientsCount', 'actions'];
  rows: MealDto[] = [];
  total = 0;

  pageNumber = 1;
  pageSize = 10;
  search = '';
  sort: string | undefined;

  @ViewChild(MatPaginator) paginator!: MatPaginator;

  ngOnInit(): void {
    this.loadMeals();
  }

  loadMeals() {
    this.mealsService.getMeals().subscribe(res => {
      console.log('Meals fetched:', res); // <- add this
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

    if (!trimmedName) {
      // Fetch all meals if search is empty
      this.loadMeals();
      return;
    }

    // Search by name
    this.mealsService.getMealByName( trimmedName ).subscribe({
      next: (res: PageResult<GetMealByNameDto>) => {
        this.rows = res.items || [];
        this.total = res.total || 0;
      },
      error: (err) => console.error('Error fetching meals by name', err)
    });
  }



  onSort(key: string) {
    if (!this.sort || !this.sort.includes(key)) this.sort = key;
    else if (this.sort === key) this.sort = `-${key}`;
    else this.sort = undefined;
    this.loadMeals();
  }

  createMeal() {
    console.log('Create new meal');
  }

  editMeal(meal: any) {
    console.log('Edit clicked', meal);
  }

  deleteMeal(meal: any) {
    console.log('Delete clicked', meal);
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
