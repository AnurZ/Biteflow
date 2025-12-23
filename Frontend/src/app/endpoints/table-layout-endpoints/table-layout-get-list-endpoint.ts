import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { BaseEndpointAsync } from '../../helper/base-endpoint-async';
import {GetTableLayoutListDto} from '../../modules/table-layout/table-layout-model';
import { MyConfig } from '../../my-config';

@Injectable({ providedIn: 'root' })
export class TableLayoutGetListEndpoint implements BaseEndpointAsync<void, GetTableLayoutListDto[]> {
  private base = `${MyConfig.api_address}/TableLayout`; // adjust API path

  constructor(private http: HttpClient) {}

  handleAsync(): Observable<GetTableLayoutListDto[]> {
    return this.http.get<GetTableLayoutListDto[]>(this.base);
  }
}
