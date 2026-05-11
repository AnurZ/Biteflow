import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { DateRange } from '../../admin-model';
import {MyConfig} from '../../../../my-config';

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

  private formatDate(date: Date): string {
    return date.toISOString();
  }

  // ---------------- ORDERS ----------------
  getOrdersPerDay(range: DateRange): Observable<OrdersPerDayDto[]> {

    let params = new HttpParams();

    if (range?.from) {
      params = params.set('from', this.formatDate(range.from));
    }

    if (range?.to) {
      params = params.set('to', this.formatDate(range.to));
    }

    return this.http.get<OrdersPerDayDto[]>(
      `${this.baseUrl}/orders-per-day`,
      { params }
    );
  }

  // ---------------- REVENUE ----------------
  getRevenuePerDay(range: DateRange): Observable<RevenuePerDayDto[]> {

    let params = new HttpParams();

    if (range?.from) {
      params = params.set('from', this.formatDate(range.from));
    }

    if (range?.to) {
      params = params.set('to', this.formatDate(range.to));
    }

    return this.http.get<RevenuePerDayDto[]>(
      `${this.baseUrl}/revenue-per-day`,
      { params }
    );
  }

  // ---------------- TOP SELLING ----------------
  getTopSellingItems(range: DateRange): Observable<TopSellingItemDto[]> {

    let params = new HttpParams();

    if (range?.from) {
      params = params.set('from', this.formatDate(range.from));
    }

    if (range?.to) {
      params = params.set('to', this.formatDate(range.to));
    }

    return this.http.get<TopSellingItemDto[]>(
      `${this.baseUrl}/top-selling-items`,
      { params }
    );
  }
}
