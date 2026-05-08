import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { MyConfig } from '../../../../my-config';

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

  private baseUrl = `${MyConfig.api_address}/analytics`;

  constructor(private http: HttpClient) {}

  getKpis(from: Date, to: Date): Observable<KpiDto> {

    let params = new HttpParams()
      .set('from', from.toISOString())
      .set('to', to.toISOString());

    return this.http.get<KpiDto>(`${this.baseUrl}/kpis`, { params });
  }
}
