import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { map } from 'rxjs/operators';
import { MyConfig } from '../../my-config';

export type OrderStatus = 'New' | 'Cooking' | 'ReadyForPickup' | 'Completed' | 'Cancelled';

export interface AdminOrderDto {
  id: number;
  tableLayoutId?: number;
  tableLayoutName?: string;
  diningTableId?: number;
  tableNumber?: number;
  status: OrderStatus;
  createdAtUtc: string;
  notes?: string;
  itemsCount: number;
  totalPrice: number;
  items: OrderItemDto[];
}

export interface PageResult<T> {
  items: T[];
  total: number;
}

export interface AdminGetOrdersQuery {
  pageNumber: number;
  pageSize: number;
  statuses?: OrderStatus[];
  fromUtc?: string;
  toUtc?: string;
  sort?: string;
  searchById?: number;
}

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
  private base = `${MyConfig.api_address}/Orders`;
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

  getAdminOrderById(id: number) {
    return this.http
      .get<AdminOrderDto>(`${this.base}/${id}`)
      .pipe(
        map(o => ({
          ...o,
          status:
            typeof o.status === 'number'
              ? this.statusMap[o.status as number] ?? String(o.status)
              : o.status
        }))
      );
  }

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

  adminList(query: AdminGetOrdersQuery) {
    let params = new HttpParams();

    params = params.set('paging.page', query.pageNumber);
    params = params.set('paging.pageSize', query.pageSize);

    if (query.sort) {
      params = params.set('sort', query.sort);
    }

    if (query.fromUtc) {
      params = params.set('fromUtc', query.fromUtc);
    }

    if (query.toUtc) {
      params = params.set('toUtc', query.toUtc);
    }

    if(query.searchById) {
      params = params.set('searchById', query.searchById);
    }

    if (query.statuses?.length) {
      query.statuses.forEach(s => {
        params = params.append('statuses', s);
      });
    }

    return this.http
      .get<PageResult<AdminOrderDto>>(`${this.base}/admin`, { params })
      .pipe(
        map(res => ({
          ...res,
          items: res.items.map(o => ({
            ...o,
            status:
              typeof o.status === 'number'
                ? this.statusMap[o.status as number] ?? String(o.status)
                : o.status
          })),
        }))
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
