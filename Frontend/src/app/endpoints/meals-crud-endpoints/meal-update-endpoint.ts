import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BaseEndpointAsync } from '../../helper/base-endpoint-async';
import { Observable } from 'rxjs';
import { UpdateMealCommand } from '../../modules/meals/meals-model';
import { MyConfig } from '../../my-config';

@Injectable({ providedIn: 'root' })
export class MealUpdateEndpoint implements BaseEndpointAsync<UpdateMealCommand, void> {
  private base = `${MyConfig.api_address}/meal`;

  constructor(private http: HttpClient) {}

  handleAsync(body: UpdateMealCommand): Observable<void> {
    return this.http.put<void>(`${this.base}/${body.id}`, body);
  }
}
