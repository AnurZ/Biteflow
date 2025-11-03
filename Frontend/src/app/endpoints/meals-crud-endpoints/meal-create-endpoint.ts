import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BaseEndpointAsync } from '../../helper/base-endpoint-async';
import { Observable } from 'rxjs';
import { CreateMealCommand } from '../../modules/meals/meals-model';
import { MyConfig } from '../../my-config';

@Injectable({ providedIn: 'root' })
export class MealCreateEndpoint implements BaseEndpointAsync<CreateMealCommand, { id: number }> {
  private base = `${MyConfig.api_address}/meal`;

  constructor(private http: HttpClient) {}

  handleAsync(body: CreateMealCommand): Observable<{ id: number }> {
    return this.http.post<{ id: number }>(this.base, body);
  }
}
