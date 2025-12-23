import { Component } from '@angular/core';

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

interface ReadyTicket {
  id: string;
  table: number;
  status: string;
  items: number;
}

@Component({
  selector: 'app-waiter',
  templateUrl: './waiter.component.html',
  styleUrl: './waiter.component.css',
  standalone: false
})
export class WaiterComponent {
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
        { id: 9, label: 'Table 9', guests: 0, status: 'serving' },
        { id: 10, label: 'Table 10', guests: 2, status: 'seated' },
        { id: 11, label: 'Table 11', guests: 0, status: 'free' },
        { id: 12, label: 'Table 12', guests: 0, status: 'free' }
      ]
    },
    {
      title: 'Regular Section',
      description: '6 standard tables',
      tables: [
        { id: 1, label: 'Table 1', guests: 0, status: 'free' },
        { id: 2, label: 'Table 2', guests: 2, status: 'seated' },
        { id: 3, label: 'Table 3', guests: 4, status: 'seated' },
        { id: 4, label: 'Table 4', guests: 3, status: 'serving' },
        { id: 5, label: 'Table 5', guests: 0, status: 'free' },
        { id: 6, label: 'Table 6', guests: 2, status: 'paying' }
      ]
    }
  ];

  readyTickets: ReadyTicket[] = [
    { id: '#1028', table: 6, status: 'Ready', items: 2 },
    { id: '#1031', table: 2, status: 'Ready', items: 3 }
  ];

  selectedTableId: number = this.sections[0].tables[0].id;
  selectedCategory: MenuItem['category'] = 'Mains';

  tableOrders: Record<number, OrderItem[]> = {};

  menuCategories: MenuItem['category'][] = ['Starters', 'Mains', 'Desserts', 'Drinks'];

  menuItems: MenuItem[] = [
    { name: 'House Burger', price: 14.99, category: 'Mains' },
    { name: 'Truffle Pasta', price: 18.99, category: 'Mains' },
    { name: 'Grilled Salmon', price: 24.99, category: 'Mains' },
    { name: 'Caesar Salad', price: 11.5, category: 'Starters' },
    { name: 'Caprese Skewers', price: 9.5, category: 'Starters' },
    { name: 'Tomato Soup', price: 8.25, category: 'Starters' },
    { name: 'Cheesecake', price: 7.75, category: 'Desserts' },
    { name: 'Tiramisu', price: 7.95, category: 'Desserts' },
    { name: 'Chocolate Lava Cake', price: 8.5, category: 'Desserts' },
    { name: 'Espresso', price: 3.5, category: 'Drinks' },
    { name: 'Iced Tea', price: 4.25, category: 'Drinks' },
    { name: 'Sparkling Water', price: 3.25, category: 'Drinks' }
  ];

  get selectedTable(): TableTile | undefined {
    return this.sections.flatMap(section => section.tables).find(t => t.id === this.selectedTableId);
  }

  get currentOrder(): OrderItem[] {
    return this.tableOrders[this.selectedTableId] || [];
  }

  get readyCount(): number {
    return this.readyTickets.length;
  }

  selectTable(tableId: number): void {
    this.selectedTableId = tableId;
    if (!this.tableOrders[tableId]) {
      this.tableOrders[tableId] = [];
    }
  }

  setCategory(category: MenuItem['category']): void {
    this.selectedCategory = category;
  }

  addItem(item: MenuItem): void {
    const current = this.tableOrders[this.selectedTableId] || [];
    const existing = current.find(orderItem => orderItem.name === item.name);

    if (existing) {
      existing.qty += 1;
    } else {
      current.push({ name: item.name, price: item.price, qty: 1 });
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

  filteredMenu(): MenuItem[] {
    return this.menuItems.filter(item => item.category === this.selectedCategory);
  }
}
