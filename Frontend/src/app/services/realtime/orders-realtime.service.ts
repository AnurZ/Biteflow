import { Injectable } from '@angular/core';
import { RealtimeHubService } from './realtime-hub.service';
import { OrderCreatedEvent, OrderStatusChangedEvent} from './orders-realtime.model';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class OrdersRealtimeService {
  private readonly hubUrl = '/hubs/orders';

  constructor(private readonly hub: RealtimeHubService) {}

  start(): Promise<void> {
    return this.hub.start(this.hubUrl);
  }

  onOrderCreated(): Observable<OrderCreatedEvent> {
    return this.hub.on<OrderCreatedEvent>('OrderCreated');
  }

  onOrderStatusChanged(): Observable<OrderStatusChangedEvent> {
    return this.hub.on<OrderStatusChangedEvent>('OrderStatusChanged');
  }

  onDashboardUpdated(): Observable<any> {
    return this.hub.on<any>('DashboardUpdated');
  }
}
