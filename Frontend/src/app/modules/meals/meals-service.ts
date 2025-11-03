import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import {
  MealDto,
  GetMealByIdDto,
  MealIngredientQueryDto,
  CreateMealCommand,
  UpdateMealCommand, PageResult, GetMealByNameDto
} from '../meals/meals-model';

import { MealCreateEndpoint } from '../../endpoints/meals-crud-endpoints/meal-create-endpoint';
import { MealUpdateEndpoint } from '../../endpoints/meals-crud-endpoints/meal-update-endpoint';
import { MealDeleteEndpoint } from '../../endpoints/meals-crud-endpoints/meal-delete-endpoint';
import { MealGetByIdEndpoint } from '../../endpoints/meals-crud-endpoints/meal-getbyid-endpoint';
import { MealGetListEndpoint } from '../../endpoints/meals-crud-endpoints/meal-getlist-endpoint';
import { MealGetIngredientsEndpoint } from '../../endpoints/meals-crud-endpoints/meal-getingredients-endpoint';
import {MealGetbynameEndpoint} from '../../endpoints/meals-crud-endpoints/meal-getbyname-endpoint';

@Injectable({ providedIn: 'root' })
export class MealsService {
  constructor(
    private createEp: MealCreateEndpoint,
    private updateEp: MealUpdateEndpoint,
    private deleteEp: MealDeleteEndpoint,
    private getByIdEp: MealGetByIdEndpoint,
    private getListEp: MealGetListEndpoint,
    private getByNameEp: MealGetbynameEndpoint,
    private getIngredientsEp: MealGetIngredientsEndpoint
  ) {}

  // === CRUD Methods ===
  getMeals(): Observable<MealDto[]> {
    return this.getListEp.handleAsync();
  }

  getMealById(id: number): Observable<GetMealByIdDto> {
    return this.getByIdEp.handleAsync(id);
  }

  getMealByName(name: string): Observable<PageResult<GetMealByNameDto>>{
    return this.getByNameEp.handleAsync({name});
  }

  getMealIngredients(mealId: number): Observable<MealIngredientQueryDto[]> {
    return this.getIngredientsEp.handleAsync(mealId);
  }

  createMeal(command: CreateMealCommand): Observable<{ id: number }> {
    return this.createEp.handleAsync(command);
  }

  updateMeal(command: UpdateMealCommand): Observable<void> {
    return this.updateEp.handleAsync(command);
  }

  deleteMeal(id: number): Observable<void> {
    return this.deleteEp.handleAsync(id);
  }

}
