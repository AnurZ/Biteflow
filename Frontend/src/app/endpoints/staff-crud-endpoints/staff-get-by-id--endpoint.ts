import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BaseEndpointAsync } from '../../helper/base-endpoint-async';
import { Observable } from 'rxjs';
import { StaffDetails } from '../../modules/admin/staff/models';
import { MyConfig } from '../../my-config';

@Injectable({ providedIn: 'root' })
export class StaffGetByIdEndpoint implements BaseEndpointAsync<number, StaffDetails> {
  private base = `${MyConfig.api_address}/staff`;
  constructor(private http: HttpClient) {}

  handleAsync(id: number): Observable<StaffDetails> {
    return this.http.get<StaffDetails>(`${this.base}/${id}`);
  }
}
