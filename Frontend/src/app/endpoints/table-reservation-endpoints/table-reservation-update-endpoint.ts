import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BaseEndpointAsync } from '../../helper/base-endpoint-async';
import { Observable } from 'rxjs';
import { UpdateStaffRequest } from '../../modules/admin/staff/models';
import { MyConfig } from '../../my-config';
import {GetTableLayoutListDto} from '../../modules/table-layout/table-layout-model';
import {GetTableReservationsQueryDto} from '../../modules/table-reservation/table-reservation-model';

@Injectable({ providedIn: 'root' })
export class TableReservationUpdateEndpoint implements BaseEndpointAsync<GetTableReservationsQueryDto, void> {
  private base = `${MyConfig.api_address}/TableReservation`;
  constructor(private http: HttpClient) {}

  handleAsync(body: GetTableReservationsQueryDto): Observable<void> {
    console.log('handleAsync', body);
    return this.http.put<void>(`${this.base}/${body.id}`, body);
  }
}
