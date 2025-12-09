import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BaseEndpointAsync } from '../../helper/base-endpoint-async';
import { Observable } from 'rxjs';
import { UpdateStaffRequest } from '../../modules/admin/staff/models';
import { MyConfig } from '../../my-config';
import {GetTableLayoutListDto} from '../../modules/table-layout/table-layout-model';

@Injectable({ providedIn: 'root' })
export class TableLayoutUpdateEndpoint implements BaseEndpointAsync<GetTableLayoutListDto, void> {
  private base = `${MyConfig.api_address}/TableLayout`;
  constructor(private http: HttpClient) {}

  handleAsync(body: GetTableLayoutListDto): Observable<void> {
    return this.http.put<void>(`${this.base}/${body.id}`, body);
  }
}
