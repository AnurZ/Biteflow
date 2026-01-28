import { Component, OnDestroy, OnInit } from '@angular/core';
import { Subscription } from 'rxjs';
import {
  CreateOrderRequest,
  OrderDto,
  OrdersService
} from '../../services/orders/orders.service';
import { MyConfig } from '../../my-config';
import { RealtimeHubService } from '../../services/realtime/realtime-hub.service';
import { OrderStatusChangedEvent } from '../../services/realtime/orders-realtime.model';
import { MealsService } from '../meals/meals-service';
import { MealDto, MealCategory } from '../meals/meals-model';
import { MealCategoryGetEndpoint } from '../../endpoints/meal-category-crud-endpoint/meal-category-get-endpoint';
import { OrderStatus } from '../../services/orders/orders.service';
import { MatSnackBar } from '@angular/material/snack-bar';

type TableStatus = 'free' | 'seated' | 'serving' | 'paying';

interface TableTile {
  id: number;
  label: string;
  guests: number;
  status: TableStatus;
}

interface TableSection {
  title: string;
  description: string;
  tables: TableTile[];
}

interface MenuItem {
  name: string;
  price: number;
  category: 'Starters' | 'Mains' | 'Desserts' | 'Drinks';
}

interface OrderItem {
  name: string;
  price: number;
  qty: number;
}

@Component({
  selector: 'app-waiter',
  templateUrl: './waiter.component.html',
  styleUrl: './waiter.component.css',
  standalone: false
})
export class WaiterComponent implements OnInit, OnDestroy {
  statusLegend: { key: TableStatus; label: string; color: string }[] = [
    { key: 'free', label: 'Free', color: '#e5e7eb' },
    { key: 'seated', label: 'Seated', color: '#e0e9ff' },
    { key: 'serving', label: 'Serving', color: '#dcfce7' },
    { key: 'paying', label: 'Paying', color: '#ffe4e6' }
  ];

  sections: TableSection[] = [
    {
      title: 'Family Section',
      description: '4 large tables',
      tables: [
        { id: 9, label: 'Table 9', guests: 6, status: 'free' },
        { id: 10, label: 'Table 10', guests: 6, status: 'free' },
        { id: 11, label: 'Table 11', guests: 6, status: 'free' },
        { id: 12, label: 'Table 12', guests: 6, status: 'free' }
      ]
    },
    {
      title: 'Regular Section',
      description: '6 standard tables',
      tables: [
        { id: 1, label: 'Table 1', guests: 2, status: 'free' },
        { id: 2, label: 'Table 2', guests: 2, status: 'free' },
        { id: 3, label: 'Table 3', guests: 4, status: 'free' },
        { id: 4, label: 'Table 4', guests: 4, status: 'free' },
        { id: 5, label: 'Table 5', guests: 2, status: 'free' },
        { id: 6, label: 'Table 6', guests: 2, status: 'free' }
      ]
    }
  ];

  orders: OrderDto[] = [];
  loadingOrders = false;
  submitting = false;
  meals: MealDto[] = [];
  categories: MealCategory[] = [];
  loadingMeals = false;
  pollHandle?: ReturnType<typeof setInterval>;
  billingOrder?: OrderDto;
  billTipRate = 0.1;
  tableStatusOverrides: Record<number, TableStatus> = {};
  private realtimeSub = new Subscription();

  selectedTableId: number = this.sections[0].tables[0].id;
  selectedCategoryId?: number;

  tableOrders: Record<number, OrderItem[]> = {};

  constructor(
    private readonly ordersService: OrdersService,
    private readonly mealsService: MealsService,
    private readonly mealCategoryEndpoint: MealCategoryGetEndpoint,
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
    this.loadMenu();
    this.pollHandle = setInterval(() => this.loadOrders(), 10000);
    void this.realtime.start(MyConfig.orders_hub);
    this.realtimeSub.add(
      this.realtime.on<OrderStatusChangedEvent>('OrderStatusChanged').subscribe(() => {
        this.loadOrders();
      })
    );
  }

  ngOnDestroy(): void {
    if (this.pollHandle) {
      clearInterval(this.pollHandle);
    }
    this.realtimeSub.unsubscribe();
  }

  loadOrders(): void {
    this.loadingOrders = true;
    this.ordersService.list(['New', 'Cooking', 'ReadyForPickup', 'Completed']).subscribe({
      next: orders => {
        this.orders = orders;
        this.updateTableStatuses();
      },
      complete: () => (this.loadingOrders = false),
      error: () => (this.loadingOrders = false)
    });
  }

  loadMenu(): void {
    this.loadingMeals = true;
    this.mealCategoryEndpoint.handleAsync().subscribe({
      next: categories => {
        this.categories = categories;
        if (categories.length && this.selectedCategoryId === undefined) {
          this.selectedCategoryId = categories[0].id;
        }
      }
    });

    this.mealsService.getMeals().subscribe({
      next: meals => {
        this.meals = meals.filter(m => m.isAvailable);
      },
      complete: () => (this.loadingMeals = false),
      error: () => (this.loadingMeals = false)
    });
  }

  get selectedTable(): TableTile | undefined {
    return this.sections.flatMap(section => section.tables).find(t => t.id === this.selectedTableId);
  }

  get currentOrder(): OrderItem[] {
    const local = this.tableOrders[this.selectedTableId];
    if (local && local.length) return local;

    const latest = this.latestOrderForSelectedTable(this.selectedTable?.status === 'paying' ? false : true);
    if (latest) {
      return latest.items.map(i => ({
        name: i.name,
        price: i.unitPrice,
        qty: i.quantity
      }));
    }

    return [];
  }

  get readyCount(): number {
    return this.readyOrders.length;
  }

  get readyOrders(): OrderDto[] {
    return this.orders.filter(o => o.status === 'ReadyForPickup');
  }

  get isEditingLocal(): boolean {
    const local = this.tableOrders[this.selectedTableId];
    return !!(local && local.length);
  }

  get hasActiveOrderForSelectedTable(): boolean {
    const tableId = this.selectedTableId;
    return this.orders.some(
      o =>
        (o.tableNumber ?? o.diningTableId) === tableId &&
        this.isActiveStatus(o.status)
    );
  }

  private isActiveStatus(status: OrderStatus): boolean {
    return status === 'New' || status === 'Cooking' || status === 'ReadyForPickup';
  }

  private updateTableStatuses(): void {
    const statusByTable: Record<number, TableStatus> = {};

    const activeOrders = this.orders.filter(o => this.isActiveStatus(o.status));
    const latestByTable = new Map<number, OrderDto>();
    for (const order of activeOrders) {
      const tableNum = order.tableNumber ?? order.diningTableId;
      if (!tableNum) continue;
      const existing = latestByTable.get(tableNum);
      if (!existing || new Date(order.createdAtUtc) > new Date(existing.createdAtUtc)) {
        latestByTable.set(tableNum, order);
      }
    }

    latestByTable.forEach((order, tableNum) => {
      let status: TableStatus = 'free';
      if (order.status === 'New' || order.status === 'Cooking') {
        status = 'seated';
      } else if (order.status === 'ReadyForPickup') {
        status = 'serving';
      }
      statusByTable[tableNum] = status;
    });

    Object.entries(this.tableStatusOverrides).forEach(([tableId, status]) => {
      statusByTable[Number(tableId)] = status;
    });

    this.sections = this.sections.map(section => ({
      ...section,
      tables: section.tables.map(table => ({
        ...table,
        status: statusByTable[table.id] ?? 'free'
      }))
    }));
  }

  selectTable(tableId: number): void {
    this.selectedTableId = tableId;
    if (!this.tableOrders[tableId]) {
      this.tableOrders[tableId] = [];
    }
  }

  setCategory(categoryId: number): void {
    this.selectedCategoryId = categoryId;
  }

  addItem(item: MealDto): void {
    const current = this.tableOrders[this.selectedTableId] || [];
    const existing = current.find(orderItem => orderItem.name === item.name);

    if (existing) {
      existing.qty += 1;
    } else {
      current.push({ name: item.name, price: item.basePrice, qty: 1 });
    }

    this.tableOrders[this.selectedTableId] = [...current];
  }

  updateQuantity(itemName: string, delta: number): void {
    const current = this.tableOrders[this.selectedTableId] || [];
    const updated = current
      .map(orderItem =>
        orderItem.name === itemName ? { ...orderItem, qty: Math.max(1, orderItem.qty + delta) } : orderItem
      )
      .filter(orderItem => orderItem.qty > 0);

    this.tableOrders[this.selectedTableId] = updated;
  }

  removeItem(itemName: string): void {
    const current = this.tableOrders[this.selectedTableId] || [];
    this.tableOrders[this.selectedTableId] = current.filter(orderItem => orderItem.name !== itemName);
  }

  get tablesFlat(): TableTile[] {
    return this.sections.flatMap(section => section.tables);
  }

  getStatusLabel(status: TableStatus): string {
    switch (status) {
      case 'free':
        return 'Free';
      case 'seated':
        return 'Seated';
      case 'serving':
        return 'Serving';
      case 'paying':
        return 'Paying';
      default:
        return '';
    }
  }

  filteredMenu(): MealDto[] {
    if (!this.selectedCategoryId) {
      return this.meals;
    }

    return this.meals.filter(item => item.categoryId === this.selectedCategoryId);
  }

  sendToKitchen(): void {
    if (!this.currentOrder.length || this.submitting) {
      return;
    }

    const payload: CreateOrderRequest = {
      tableNumber: this.selectedTable?.label ? Number(this.selectedTable.label.replace(/\D/g, '')) : this.selectedTableId,
      items: this.currentOrder.map(item => ({
        name: item.name,
        quantity: item.qty,
        unitPrice: item.price
      }))
    };

    this.submitting = true;
    this.ordersService.create(payload).subscribe({
      next: () => {
        this.tableOrders[this.selectedTableId] = [];
        this.tableStatusOverrides[this.selectedTableId] = 'seated';
        this.loadOrders();
        this.showSnack(`Order sent to kitchen for ${this.selectedTable?.label ?? 'table'}`, 'success');
      },
      error: () => {
        this.submitting = false;
      },
      complete: () => {
        this.submitting = false;
      }
    });
  }

  markPickedUp(order: OrderDto): void {
    this.ordersService.updateStatus(order.id, 'Completed').subscribe({
      next: () => {
        const tableNum = order.tableNumber ?? order.diningTableId;
        if (tableNum) {
          this.tableStatusOverrides[tableNum] = 'paying';
        }
        this.loadOrders();
        this.showSnack(`Order #${order.id} picked up. Table set to paying.`, 'info');
      }
    });
  }

  printBill(): void {
    const order = this.latestOrderForSelectedTable(false);
    if (!order) return;
    this.billingOrder = order;
  }

  closeBill(): void {
    this.billingOrder = undefined;
  }

  payBill(): void {
    if (!this.billingOrder) return;
    const order = this.billingOrder;
    this.ordersService.updateStatus(order.id, 'Completed').subscribe({
      next: () => {
        const tableNum = order.tableNumber ?? order.diningTableId;
        if (tableNum) {
          this.tableStatusOverrides[tableNum] = 'free';
        }
        this.loadOrders();
        this.closeBill();
        this.showSnack(`Bill paid for ${this.selectedTable?.label ?? 'table'}.`, 'success');
      },
      error: () => this.closeBill()
    });
  }

  latestOrderForSelectedTable(activeOnly: boolean = true): OrderDto | undefined {
    const tableId = this.selectedTableId;
    const matching = this.orders
      .filter(
        o =>
          (o.tableNumber ?? o.diningTableId) === tableId &&
          (!activeOnly || this.isActiveStatus(o.status))
      )
      .sort((a, b) => new Date(b.createdAtUtc).getTime() - new Date(a.createdAtUtc).getTime());
    return matching[0];
  }

  get billSubtotal(): number {
    if (!this.billingOrder) return 0;
    return this.billingOrder.items.reduce((sum, item) => sum + item.quantity * item.unitPrice, 0);
  }

  get billTax(): number {
    return this.billSubtotal * 0.08;
  }

  get billTip(): number {
    return this.billSubtotal * this.billTipRate;
  }

  get billTotal(): number {
    return this.billSubtotal + this.billTax + this.billTip;
  }
}
