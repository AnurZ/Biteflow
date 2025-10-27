import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { BaseEndpointAsync } from '../../helper/base-endpoint-async';
import { PageResult, StaffListItem } from '../../modules/admin/staff/models';
import { MyConfig } from '../../my-config';

export interface StaffListRequest {
  pageNumber: number;
  pageSize: number;
  search?: string;
  sort?: string; // firstName or -hireDate
}

@Injectable({ providedIn: 'root' })
export class StaffListEndpoint implements BaseEndpointAsync<StaffListRequest, PageResult<StaffListItem>> {
  private base = `${MyConfig.api_address}/staff`;
  constructor(private http: HttpClient) {}

  handleAsync(req: StaffListRequest): Observable<PageResult<StaffListItem>> {
    let params = new HttpParams()
      .set('pageNumber', req.pageNumber)
      .set('pageSize', req.pageSize);
    if (req.search) params = params.set('search', req.search);
    if (req.sort) params = params.set('sort', req.sort);

    return this.http.get<PageResult<StaffListItem>>(this.base, { params });
  }
}
