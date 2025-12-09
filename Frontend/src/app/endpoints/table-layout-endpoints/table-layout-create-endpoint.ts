
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BaseEndpointAsync } from '../../helper/base-endpoint-async';
import { Observable } from 'rxjs';
import { CreateStaffRequest } from '../../modules/admin/staff/models';
import { MyConfig } from '../../my-config';
import {CreateTableLayoutDto} from '../../modules/table-layout/table-layout-model';

@Injectable({ providedIn: 'root' })
export class TableLayoutCreateEndpoint implements BaseEndpointAsync<CreateTableLayoutDto, { id: number }> {
  private base = `${MyConfig.api_address}/TableLayout`;
  constructor(private http: HttpClient) {}

  handleAsync(body: CreateTableLayoutDto): Observable<{ id: number }> {
    return this.http.post<{ id: number }>(this.base, body);
  }
}
