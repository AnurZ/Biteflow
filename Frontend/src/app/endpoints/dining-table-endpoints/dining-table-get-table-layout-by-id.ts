import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { BaseEndpointAsync } from '../../helper/base-endpoint-async';
import { MyConfig } from '../../my-config';

export interface GetDiningTableTableLayoutIdByIdDto {
  tableLayoutId: number;
}

@Injectable({ providedIn: 'root' })
export class DiningTableGetTableLayoutIdEndpoint
  implements BaseEndpointAsync<number, GetDiningTableTableLayoutIdByIdDto> {

  private base = `${MyConfig.api_address}/DiningTable/dining-table`;

  constructor(private http: HttpClient) {}

  handleAsync(diningTableId: number): Observable<GetDiningTableTableLayoutIdByIdDto> {
    return this.http.get<GetDiningTableTableLayoutIdByIdDto>(
      `${this.base}/${diningTableId}/table-layout-id`
    );
  }
}
