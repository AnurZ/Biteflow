import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { OrderStatus} from '../../orders/orders.service';
import {MyConfig} from '../../../my-config';

@Injectable({
  providedIn: 'root'
})
export class OrderExportEndpoint {

  private base = `${MyConfig.api_address}/OrderExport`;

  constructor(private http: HttpClient) {}

  handleAsync(query: {
    fromDate?: string;
    toDate?: string;
    status?: OrderStatus;
    format?: 'xlsx' | 'csv';
  }) {
    return this.http.get(`${this.base}/export`, {
      params: {
        fromDate: query.fromDate ?? '',
        toDate: query.toDate ?? '',
        status: query.status ?? '',
        format: query.format ?? 'xlsx'
      },
      responseType: 'blob'
    });
  }
}
