import { Component } from '@angular/core';

type KitchenStage = 'new' | 'cooking' | 'ready';

interface KitchenItem {
  name: string;
  qty: number;
}

interface KitchenOrder {
  id: string;
  table: string;
  eta: string;
  items: KitchenItem[];
  stage: KitchenStage;
}

interface KitchenColumn {
  key: KitchenStage;
  title: string;
  icon: string;
  orders: KitchenOrder[];
}

@Component({
  selector: 'app-kitchen',
  templateUrl: './kitchen.component.html',
  styleUrl: './kitchen.component.css',
  standalone: false
})
export class KitchenComponent {
  board: KitchenColumn[] = [
    {
      key: 'new',
      title: 'New Orders',
      icon: '📦',
      orders: [
        {
          id: '#1024',
          table: 'Table 3',
          eta: '96m',
          stage: 'new',
          items: [
            { name: 'House Burger', qty: 2 },
            { name: 'Caesar Salad', qty: 2 }
          ]
        },
        {
          id: '#1026',
          table: 'Table 10',
          eta: '94m',
          stage: 'new',
          items: [
            { name: 'Ribeye Steak', qty: 1 },
            { name: 'Tomato Soup', qty: 1 }
          ]
        }
      ]
    },
    {
      key: 'cooking',
      title: 'Cooking',
      icon: '🔥',
      orders: [
        {
          id: '#1025',
          table: 'Table 4',
          eta: '106m',
          stage: 'cooking',
          items: [
            { name: 'Grilled Salmon', qty: 1 },
            { name: 'Truffle Pasta', qty: 2 }
          ]
        },
        {
          id: '#1027',
          table: 'Table 8',
          eta: '111m',
          stage: 'cooking',
          items: [
            { name: 'Truffle Pasta', qty: 3 },
            { name: 'Caesar Salad', qty: 3 }
          ]
        }
      ]
    },
    {
      key: 'ready',
      title: 'Ready for Pickup',
      icon: '✅',
      orders: [
        {
          id: '#1028',
          table: 'Table 6',
          eta: '116m',
          stage: 'ready',
          items: [
            { name: 'Ribeye Steak', qty: 2 },
            { name: 'Tiramisu', qty: 2 }
          ]
        },
        {
          id: '#1030',
          table: 'Table 1',
          eta: '88m',
          stage: 'ready',
          items: [
            { name: 'House Burger', qty: 1 },
            { name: 'Garden Salad', qty: 1 }
          ]
        }
      ]
    }
  ];

  get activeOrders(): number {
    return this.board.reduce((total, column) => total + column.orders.length, 0);
  }

  actionLabel(stage: KitchenStage): string {
    switch (stage) {
      case 'new':
        return 'Start Cooking';
      case 'cooking':
        return 'Complete';
      case 'ready':
        return 'Ready for pickup';
      default:
        return '';
    }
  }
}
