import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { BaseEndpointAsync } from '../../helper/base-endpoint-async';
import { MealDto } from '../../modules/meals/meals-model';
import { MyConfig } from '../../my-config';

@Injectable({ providedIn: 'root' })
export class MealGetListEndpoint implements BaseEndpointAsync<void, MealDto[]> {
  private base = `${MyConfig.api_address}/meal`;

  constructor(private http: HttpClient) {}

  handleAsync(): Observable<MealDto[]> {
    return this.http.get<MealDto[]>(this.base);
  }
}
