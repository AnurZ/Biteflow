import { Component, OnDestroy, OnInit } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Subscription } from 'rxjs';
import { MyConfig } from '../../my-config';
import { OrderDto, OrderStatus, OrdersService } from '../../services/orders/orders.service';
import { RealtimeHubService } from '../../services/realtime/realtime-hub.service';
import { OrderCreatedEvent, OrderStatusChangedEvent } from '../../services/realtime/orders-realtime.model';

type KitchenStage = OrderStatus;

interface KitchenColumn {
  key: KitchenStage;
  title: string;
  icon: string;
  orders: OrderDto[];
  cta: string;
  next?: OrderStatus;
}

@Component({
  selector: 'app-kitchen',
  templateUrl: './kitchen.component.html',
  styleUrl: './kitchen.component.css',
  standalone: false
})
export class KitchenComponent implements OnInit, OnDestroy {
  orders: OrderDto[] = [];
  loading = false;
  updatingId?: number;
  private realtimeSub = new Subscription();

  columnsMeta: Omit<KitchenColumn, 'orders'>[] = [
    { key: 'New', title: 'New Orders', icon: '📦', cta: 'Start Cooking', next: 'Cooking' },
    { key: 'Cooking', title: 'Cooking', icon: '🔥', cta: 'Complete', next: 'ReadyForPickup' },
    { key: 'ReadyForPickup', title: 'Ready for Pickup', icon: '✅', cta: 'Ready for pickup' }
  ];

  constructor(
    private readonly ordersService: OrdersService,
    private readonly snack: MatSnackBar,
    private readonly realtime: RealtimeHubService
  ) {}

  private showSnack(message: string, type: 'success' | 'info' | 'warn' = 'info'): void {
    this.snack.open(message, 'Close', {
      duration: 3000,
      panelClass: ['app-snackbar', `app-snackbar-${type}`]
    });
  }

  ngOnInit(): void {
    this.loadOrders();
    void this.realtime.start(MyConfig.orders_hub);
    this.realtimeSub.add(
      this.realtime.on<OrderCreatedEvent>('OrderCreated').subscribe(() => {
        this.loadOrders();
      })
    );
    this.realtimeSub.add(
      this.realtime.on<OrderStatusChangedEvent>('OrderStatusChanged').subscribe(() => {
        this.loadOrders();
      })
    );
  }

  ngOnDestroy(): void {
    this.realtimeSub.unsubscribe();
  }

  get board(): KitchenColumn[] {
    return this.columnsMeta.map(meta => ({
      ...meta,
      orders: this.orders.filter(o => o.status === meta.key)
    }));
  }

  get activeOrders(): number {
    return this.orders.length;
  }

  statusLabel(stage: KitchenStage): string {
    switch (stage) {
      case 'New':
        return 'New';
      case 'Cooking':
        return 'Cooking';
      case 'ReadyForPickup':
        return 'Ready for pickup';
      default:
        return stage;
    }
  }

  actionLabel(stage: KitchenStage): string {
    switch (stage) {
      case 'New':
        return 'Start Cooking';
      case 'Cooking':
        return 'Complete';
      case 'ReadyForPickup':
        return 'Ready for pickup';
      default:
        return '';
    }
  }

  loadOrders(): void {
    this.loading = true;
    this.ordersService.list(['New', 'Cooking', 'ReadyForPickup']).subscribe({
      next: orders => {
        this.orders = orders;
      },
      complete: () => (this.loading = false),
      error: () => (this.loading = false)
    });
  }

  advance(order: OrderDto): void {
    const next = this.nextStatus(order.status);
    if (!next) return;

    this.updatingId = order.id;
    this.ordersService.updateStatus(order.id, next).subscribe({
      next: () => {
        this.showSnack(`Order #${order.id} moved to ${this.statusLabel(next)}`, 'success');
        this.loadOrders();
      },
      complete: () => (this.updatingId = undefined),
      error: () => (this.updatingId = undefined)
    });
  }

  nextStatus(status: OrderStatus): OrderStatus | null {
    switch (status) {
      case 'New':
        return 'Cooking';
      case 'Cooking':
        return 'ReadyForPickup';
      default:
        return null;
    }
  }
}
