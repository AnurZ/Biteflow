import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BaseEndpointAsync } from '../../helper/base-endpoint-async';
import { Observable } from 'rxjs';
import { UpdateStaffRequest } from '../../modules/admin/staff/models';
import { MyConfig } from '../../my-config';
import {GetTableLayoutListDto, UpdateDiningTableDto} from '../../modules/table-layout/table-layout-model';

@Injectable({ providedIn: 'root' })
export class DiningTableUpdateEndpoint implements BaseEndpointAsync<UpdateDiningTableDto, void> {
  private base = `${MyConfig.api_address}/DiningTable`;
  constructor(private http: HttpClient) {}

  handleAsync(body: UpdateDiningTableDto): Observable<void> {
    return this.http.put<void>(`${this.base}/${body.id}`, body);
  }
}
