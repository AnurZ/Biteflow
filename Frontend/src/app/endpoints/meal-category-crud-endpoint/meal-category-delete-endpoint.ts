
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BaseEndpointAsync } from '../../helper/base-endpoint-async';
import { Observable } from 'rxjs';
import { MyConfig } from '../../my-config';

@Injectable({ providedIn: 'root' })
export class MealCategoryDeleteEndpoint implements BaseEndpointAsync<number, void> {
  private base = `${MyConfig.api_address}/MealCategory`;
  constructor(private http: HttpClient) {}

  handleAsync(id: number): Observable<void> {
    return this.http.delete<void>(`${this.base}/${id}`);
  }
}
