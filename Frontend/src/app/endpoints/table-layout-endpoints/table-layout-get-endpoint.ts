import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { MyConfig } from '../../my-config';
import {GetTableLayoutDto} from '../../modules/table-layout/table-layout-model';

@Injectable({ providedIn: 'root' })
export class TableLayoutGetPreviewEndpoint {
  private base = `${MyConfig.api_address}/TableLayout`;

  constructor(private http: HttpClient) {}

  handleAsync(): Observable<GetTableLayoutDto[]> {
    return this.http.get<GetTableLayoutDto[]>(this.base);
  }
}
