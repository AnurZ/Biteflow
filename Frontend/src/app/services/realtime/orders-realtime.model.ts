import { OrderStatus } from '../orders/orders.service';

export interface OrderCreatedEvent {
  orderId: number;
  tableNumber?: number;
  note?: string;
  createdAt?: string;
  status: OrderStatus;
}

export interface OrderStatusChangedEvent {
  orderId: number;
  status: OrderStatus;
}
