import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { MyConfig } from '../../my-config';

export interface TableLayoutGetNameByIdDto {
  id: string;
  name: string;
}

@Injectable({ providedIn: 'root' })
export class TableLayoutGetNameByIdEndpoint {
  private base = `${MyConfig.api_address}/TableLayout`;

  constructor(private http: HttpClient) {}

  handleAsync(id: number): Observable<TableLayoutGetNameByIdDto> {
    return this.http.get<TableLayoutGetNameByIdDto>(
      `${this.base}/${id}/name`
    );
  }
}

