import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { BaseEndpointAsync } from '../../helper/base-endpoint-async';
import {MealCategory, MealDto} from '../../modules/meals/meals-model';
import { MyConfig } from '../../my-config';

@Injectable({ providedIn: 'root' })
export class MealCategoryGetEndpoint implements BaseEndpointAsync<void, MealCategory[]> {
  private base = `${MyConfig.api_address}/MealCategory`;

  constructor(private http: HttpClient) {}

  handleAsync(): Observable<MealCategory[]> {
    return this.http.get<MealCategory[]>(this.base);
  }
}
