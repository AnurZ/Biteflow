import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { map } from 'rxjs/operators';
import { MyConfig } from '../../my-config';

export type OrderStatus = 'New' | 'Cooking' | 'ReadyForPickup' | 'Completed' | 'Cancelled';

export interface OrderItemDto {
  id: number;
  mealId?: number;
  name: string;
  quantity: number;
  unitPrice: number;
}

export interface OrderDto {
  id: number;
  diningTableId?: number;
  tableNumber?: number;
  status: OrderStatus;
  createdAtUtc: string;
  notes?: string;
  items: OrderItemDto[];
}

export interface PageResult<T> {
  total: number;
  items: T[];
}

export interface CreateOrderItemRequest {
  mealId?: number;
  isCustom?: boolean;
  name?: string;
  quantity: number;
  unitPrice?: number;
}

export interface CreateOrderRequest {
  diningTableId: number;
  notes?: string;
  items: CreateOrderItemRequest[];
}

@Injectable({
  providedIn: 'root'
})
export class OrdersService {
  private http = inject(HttpClient);
  private base = `${MyConfig.api_address}/orders`;
  private statusMap: Record<number, OrderStatus> = {
    0: 'New',
    1: 'Cooking',
    2: 'ReadyForPickup',
    3: 'Completed',
    4: 'Cancelled'
  };
  private statusToNumber: Record<OrderStatus, number> = {
    New: 0,
    Cooking: 1,
    ReadyForPickup: 2,
    Completed: 3,
    Cancelled: 4
  };

  list(statuses?: OrderStatus[], page = 1, pageSize = 100) {
    let params = new HttpParams()
      .set('page', page)
      .set('pageSize', pageSize);

    if (statuses?.length) {
      statuses.forEach(s => {
        params = params.append('statuses', s);
      });
    }
    return this.http.get<PageResult<OrderDto>>(this.base, { params }).pipe(
      map(result =>
        (result.items ?? []).map(o => ({
          ...o,
          status:
            typeof o.status === 'number'
              ? this.statusMap[o.status as number] ?? String(o.status)
              : o.status
        }))
      )
    );
  }

  create(body: CreateOrderRequest) {
    return this.http.post<{ id: number }>(this.base, body);
  }

  updateStatus(id: number, status: OrderStatus) {
    const payload = {
      status: this.statusToNumber[status] ?? status
    };
    return this.http.put<void>(`${this.base}/${id}/status`, payload);
  }
}
