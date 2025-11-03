import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BaseEndpointAsync } from '../../helper/base-endpoint-async';
import { Observable } from 'rxjs';
import { GetMealByIdDto } from '../../modules/meals/meals-model';
import { MyConfig } from '../../my-config';

@Injectable({ providedIn: 'root' })
export class MealGetByIdEndpoint implements BaseEndpointAsync<number, GetMealByIdDto> {
  private base = `${MyConfig.api_address}/meal`;

  constructor(private http: HttpClient) {}

  handleAsync(id: number): Observable<GetMealByIdDto> {
    return this.http.get<GetMealByIdDto>(`${this.base}/${id}`);
  }
}
