import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BaseEndpointAsync } from '../../helper/base-endpoint-async';
import { Observable } from 'rxjs';
import {MealCategory, UpdateMealCommand} from '../../modules/meals/meals-model';
import { MyConfig } from '../../my-config';

@Injectable({ providedIn: 'root' })
export class MealCategoryUpdateEndpoint implements BaseEndpointAsync<MealCategory, void> {
  private base = `${MyConfig.api_address}/MealCategory`;

  constructor(private http: HttpClient) {}

  handleAsync(body: MealCategory): Observable<void> {
    console.log(body);
    return this.http.put<void>(`${this.base}/${body.id}`, body);
  }
}
