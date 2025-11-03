import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BaseEndpointAsync } from '../../helper/base-endpoint-async';
import { Observable } from 'rxjs';
import { MealIngredientQueryDto } from '../../modules/meals/meals-model';
import { MyConfig } from '../../my-config';

@Injectable({ providedIn: 'root' })
export class MealGetIngredientsEndpoint implements BaseEndpointAsync<number, MealIngredientQueryDto[]> {
  private base = `${MyConfig.api_address}/meal`;

  constructor(private http: HttpClient) {}

  handleAsync(mealId: number): Observable<MealIngredientQueryDto[]> {
    return this.http.get<MealIngredientQueryDto[]>(`${this.base}/${mealId}/ingredients`);
  }
}
