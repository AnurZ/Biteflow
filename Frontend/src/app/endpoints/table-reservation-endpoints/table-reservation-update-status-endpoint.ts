import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { MyConfig } from '../../my-config';

export interface UpdateTableReservationStatusDto {
  id: number;
  status: number;
}

@Injectable({ providedIn: 'root' })
export class TableReservationUpdateStatusEndpoint {
  private base = `${MyConfig.api_address}/TableReservation`;

  constructor(private http: HttpClient) {}

  handleAsync(dto: UpdateTableReservationStatusDto): Observable<void> {
    return this.http.patch<void>(`${this.base}/update-status`, dto);
  }
}

