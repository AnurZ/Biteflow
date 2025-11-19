
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BaseEndpointAsync } from '../../helper/base-endpoint-async';
import { Observable } from 'rxjs';
import { CreateStaffRequest } from '../../modules/admin/staff/models';
import { MyConfig } from '../../my-config';
import {MealCategoryCreateDto} from '../../modules/meals/meals-model';

@Injectable({ providedIn: 'root' })
export class MealCategoryCreateEndpoint implements BaseEndpointAsync<MealCategoryCreateDto, { id: number }> {
  private base = `${MyConfig.api_address}/MealCategory`;
  constructor(private http: HttpClient) {}

  handleAsync(body: MealCategoryCreateDto): Observable<{ id: number }> {
    return this.http.post<{ id: number }>(this.base, body);
  }
}
