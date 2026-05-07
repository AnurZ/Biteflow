import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface KpiDto {
  totalOrders: number;
  revenue: number;
  avgOrderValue: number;
  topItem: string;
}

@Injectable({
  providedIn: 'root'
})
export class DashboardAnalyticsService {

  private baseUrl = 'https://localhost:7260/api/Analytics';

  constructor(private http: HttpClient) {}

  getKpis(from: Date, to: Date): Observable<KpiDto> {

    let params = new HttpParams()
      .set('from', from.toISOString())
      .set('to', to.toISOString());

    return this.http.get<KpiDto>(`${this.baseUrl}/kpis`, { params });
  }
}
