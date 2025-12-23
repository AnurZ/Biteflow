import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { BaseEndpointAsync } from '../../helper/base-endpoint-async';
import { MyConfig } from '../../my-config';
import { GetTableReservationsQueryDto, TableReservationFilter } from '../../modules/table-reservation/table-reservation-model';

@Injectable({ providedIn: 'root' })
export class TableReservationGetListEndpoint
  implements BaseEndpointAsync<TableReservationFilter, GetTableReservationsQueryDto[]> {

  private base = `${MyConfig.api_address}/TableReservation`;

  constructor(private http: HttpClient) {}

  handleAsync(filter?: TableReservationFilter): Observable<GetTableReservationsQueryDto[]> {

    let params = new HttpParams();

    if (filter) {
      Object.keys(filter).forEach(key => {
        const value = (filter as any)[key];
        if (value !== undefined && value !== null && value !== '') {
          params = params.set(key, value);
        }
      });
    }

    return this.http.get<GetTableReservationsQueryDto[]>(this.base, { params });
  }
}
