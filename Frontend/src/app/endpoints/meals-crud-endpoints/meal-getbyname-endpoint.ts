import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BaseEndpointAsync } from '../../helper/base-endpoint-async';
import { Observable } from 'rxjs';
import {PageResult, GetInventoryItemByNameDto} from '../../modules/inventory-items/inventory-item-model';
import { MyConfig } from '../../my-config';
import {GetMealByNameDto} from '../../modules/meals/meals-model';

@Injectable({ providedIn: 'root' })
export class MealGetbynameEndpoint implements BaseEndpointAsync<{ name: string }, PageResult<GetMealByNameDto>> {
  private base = `${MyConfig.api_address}/Meal`;
  constructor(private http: HttpClient) {}

  handleAsync(body: { name: string; pageNumber?: number; pageSize?: number }): Observable<PageResult<GetMealByNameDto>> {
    const params: any = { name: body.name };
    if (body.pageNumber) params["paging.pageNumber"] = body.pageNumber;
    if (body.pageSize) params["paging.pageSize"] = body.pageSize;

    return this.http.get<PageResult<GetMealByNameDto>>(`${this.base}/by-name`, { params });
  }

}
