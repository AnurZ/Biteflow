import { Component, OnDestroy, OnInit } from '@angular/core';
import { Subscription } from 'rxjs';
import { RealtimeHubService } from '../../../../../../services/realtime/realtime-hub.service';
import { MyConfig } from '../../../../../../my-config';
import {OrdersService} from '../../../../../../services/orders/orders.service';

export interface LiveOrderEvent {
  orderId: number;
  tableNumber?: number;
  status: string;
  createdAt: string;
  items: { name: string; quantity: number }[];
}

export interface OrderStatusChangedEvent {
  orderId: number;
  status: string;
}

@Component({
  selector: 'app-live-orders',
  templateUrl: './live-orders.html',
  styleUrl: './live-orders.css',
  standalone: false
})
export class LiveOrders implements OnInit, OnDestroy {

  private sub = new Subscription();

  expandedOrders = new Set<number>();
  liveOrders: LiveOrderEvent[] = [];
  removingOrders = new Set<number>();

  constructor(private realtime: RealtimeHubService, private OrdersService: OrdersService) {}

  ngOnInit(): void {

    this.loadLiveOrders();

    // connect
    this.realtime.start(MyConfig.orders_hub);

    // NEW ORDER
    this.sub.add(
      this.realtime.on<LiveOrderEvent>('OrderCreated')
        .subscribe(order => {

          console.log('🔥 LIVE ORDER RECEIVED:', order);

          this.liveOrders = [order, ...this.liveOrders];

          if (this.liveOrders.length > 20) {
            this.liveOrders = this.liveOrders.slice(0, 20);
          }
        })
    );

    // STATUS CHANGED
    this.sub.add(
      this.realtime.on<OrderStatusChangedEvent>('OrderStatusChanged')
        .subscribe(update => {

          console.log('🔄 ORDER STATUS UPDATED:', update);

          // HANDLE COMPLETED WITH ANIMATION
          if (update.status === 'Completed') {

            this.removingOrders.add(update.orderId);

            setTimeout(() => {

              this.liveOrders = this.liveOrders.filter(
                order => order.orderId !== update.orderId
              );

              this.removingOrders.delete(update.orderId);

            }, 400); // match CSS transition

            return;
          }

          // UPDATE STATUS NORMALLY
          this.liveOrders = this.liveOrders.map(order => {

            if (order.orderId === update.orderId) {

              return {
                ...order,
                status: update.status
              };
            }

            return order;
          });
        })
    );
  }

  ngOnDestroy(): void {
    this.sub.unsubscribe();
  }

  private loadLiveOrders(): void {

    this.sub.add(
      this.OrdersService
        .list(['New', 'Cooking', 'ReadyForPickup'])
        .subscribe(orders => {

          this.liveOrders = orders.map(order => ({
            orderId: order.id,
            tableNumber: order.tableNumber,
            status: order.status,
            createdAt: order.createdAtUtc,
            items: order.items.map(i => ({
              name: i.name,
              quantity: i.quantity
            }))
          }));

          console.log('📦 INITIAL LIVE ORDERS:', this.liveOrders);
        })
    );
  }



}
