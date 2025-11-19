import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BaseEndpointAsync } from '../../helper/base-endpoint-async';
import { Observable } from 'rxjs';
import { StaffDetails } from '../../modules/admin/staff/models';
import { MyConfig } from '../../my-config';
import {MealCategory} from '../../modules/meals/meals-model';

@Injectable({ providedIn: 'root' })
export class MealCategoryGetByIdEndpoint implements BaseEndpointAsync<number, MealCategory> {
  private base = `${MyConfig.api_address}/MealCategory`;
  constructor(private http: HttpClient) {}

  handleAsync(id: number): Observable<MealCategory> {
    return this.http.get<MealCategory>(`${this.base}/${id}`);
  }
}
