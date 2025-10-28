import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BaseEndpointAsync } from '../../helper/base-endpoint-async';
import { Observable } from 'rxjs';
import { UpdateStaffRequest } from '../../modules/admin/staff/models';
import { MyConfig } from '../../my-config';

@Injectable({ providedIn: 'root' })
export class StaffUpdateEndpoint implements BaseEndpointAsync<UpdateStaffRequest, void> {
  private base = `${MyConfig.api_address}/staff`;
  constructor(private http: HttpClient) {}

  handleAsync(body: UpdateStaffRequest): Observable<void> {
    return this.http.put<void>(`${this.base}/${body.id}`, body);
  }
}
