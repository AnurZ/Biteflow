import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import { MyConfig } from '../../my-config';

export interface UpdateTableReservationStatusDto {
  id: number;
  status: number; // 0 = Pending, 1 = Confirmed, etc.
}

@Injectable({ providedIn: 'root' })
export class TableReservationUpdateStatusEndpoint {
  private base = `${MyConfig.api_address}/TableReservation`;

  constructor(private http: HttpClient) {}

  handleAsync(dto: UpdateTableReservationStatusDto): Promise<void> {
    return this.http.patch<void>(`${this.base}/update-status`, dto).toPromise();
  }
}

