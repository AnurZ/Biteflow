import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { BaseEndpointAsync } from '../../helper/base-endpoint-async';
import { MyConfig } from '../../my-config';
import {DiningTableStatusDto, TableStatus} from '../../modules/table-layout/table-layout-model';



@Injectable({ providedIn: 'root' })
export class DiningTablesGetStatusEndpoint
  implements BaseEndpointAsync<number | undefined, DiningTableStatusDto[]> {

  private base = `${MyConfig.api_address}/DiningTable/status`; // singular and /status

  constructor(private http: HttpClient) {}

  handleAsync(tableLayoutId?: number): Observable<DiningTableStatusDto[]> {
    let params = new HttpParams();
    if (tableLayoutId !== undefined) {
      params = params.set('tableLayoutId', tableLayoutId.toString());
    }

    return this.http.get<DiningTableStatusDto[]>(this.base, { params });
  }
}

