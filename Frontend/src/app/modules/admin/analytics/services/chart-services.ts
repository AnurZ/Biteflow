import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { MyConfig } from '../../../../my-config';

export interface OrdersPerDayDto {
  date: string;
  count: number;
}

export interface TopSellingItemDto {
  itemName: string;
  quantity: number;
}

export interface RevenuePerDayDto {
  date: string;
  revenue: number;
}

@Injectable({
  providedIn: 'root'
})
export class ChartService {

  private baseUrl = `${MyConfig.api_address}/analytics`;

  constructor(private http: HttpClient) {}

  // Orders per day
  getOrdersPerDay(from?: Date, to?: Date): Observable<OrdersPerDayDto[]> {
    let params = new HttpParams();

    if (from) {
      params = params.set('from', from.toISOString());
    }

    if (to) {
      params = params.set('to', to.toISOString());
    }

    return this.http.get<OrdersPerDayDto[]>(
      `${this.baseUrl}/orders-per-day`,
      { params }
    );
  }

  // Revenue per day
  getRevenuePerDay(from?: Date, to?: Date): Observable<RevenuePerDayDto[]> {
    let params = new HttpParams();

    if (from) {
      params = params.set('from', from.toISOString());
    }

    if (to) {
      params = params.set('to', to.toISOString());
    }

    return this.http.get<RevenuePerDayDto[]>(
      `${this.baseUrl}/revenue-per-day`,
      { params }
    );
  }

  // Top selling items
  getTopSellingItems(from?: Date, to?: Date): Observable<TopSellingItemDto[]> {
    let params = new HttpParams();

    if (from) {
      params = params.set('from', from.toISOString());
    }

    if (to) {
      params = params.set('to', to.toISOString());
    }

    return this.http.get<TopSellingItemDto[]>(
      `${this.baseUrl}/top-selling-items`,
      { params }
    );
  }
}
