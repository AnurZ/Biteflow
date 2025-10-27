
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BaseEndpointAsync } from '../../helper/base-endpoint-async';
import { Observable } from 'rxjs';
import { CreateStaffRequest } from '../../modules/admin/staff/models';
import { MyConfig } from '../../my-config';

@Injectable({ providedIn: 'root' })
export class StaffCreateEndpoint implements BaseEndpointAsync<CreateStaffRequest, { id: number }> {
  private base = `${MyConfig.api_address}/staff`;
  constructor(private http: HttpClient) {}

  handleAsync(body: CreateStaffRequest): Observable<{ id: number }> {
    return this.http.post<{ id: number }>(this.base, body);
  }
}
