import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { BaseEndpointAsync } from '../../helper/base-endpoint-async';
import { UpdateDiningTableDto } from '../../modules/table-layout/table-layout-model';
import { MyConfig } from '../../my-config';
import {DiningTableFilter} from '../../modules/table-layout/table-layout-model';

@Injectable({ providedIn: 'root' })
export class DiningTableGetListEndpoint implements BaseEndpointAsync<DiningTableFilter, UpdateDiningTableDto[]> {

  private base = `${MyConfig.api_address}/DiningTable`;

  constructor(private http: HttpClient) {}

  handleAsync(filter?: DiningTableFilter): Observable<UpdateDiningTableDto[]> {

    let params = new HttpParams();

    if (filter) {
      Object.keys(filter).forEach(key => {
        const value = (filter as any)[key];
        if (value !== undefined && value !== null) {
          params = params.set(key, value);
        }
      });
    }

    return this.http.get<UpdateDiningTableDto[]>(this.base, { params });
  }
}
