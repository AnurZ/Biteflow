
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BaseEndpointAsync } from '../../helper/base-endpoint-async';
import { Observable } from 'rxjs';
import { CreateStaffRequest } from '../../modules/admin/staff/models';
import { MyConfig } from '../../my-config';
import {
  CreateDiningTableDto
} from '../../modules/table-layout/table-layout-model';

@Injectable({ providedIn: 'root' })
export class DiningTableCreateEndpoint implements BaseEndpointAsync<CreateDiningTableDto, { id: number }> {
  private base = `${MyConfig.api_address}/DiningTable`;
  constructor(private http: HttpClient) {}

  handleAsync(body: CreateDiningTableDto): Observable<{ id: number }> {
    return this.http.post<{ id: number }>(this.base, body);
  }
}
