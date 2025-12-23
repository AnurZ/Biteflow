import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';

export interface UpdateTableReservationStatusDto {
  id: number;
  status: number; // 0 = Pending, 1 = Confirmed, etc.
}

@Injectable({ providedIn: 'root' })
export class TableReservationUpdateStatusEndpoint {
  private baseUrl = 'https://localhost:7260/api/TableReservation'; // full backend URL

  constructor(private http: HttpClient) {}

  handleAsync(dto: UpdateTableReservationStatusDto): Promise<void> {
    return this.http.patch<void>(`${this.baseUrl}/update-status`, dto).toPromise();
  }
}

