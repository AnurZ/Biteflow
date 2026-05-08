import { Component } from '@angular/core';

interface ShoppingMeal {
  name: string;
  price: string;
  badge?: string;
}

interface ShoppingCategory {
  title: string;
  subtitle: string;
  meals: ShoppingMeal[];
}

@Component({
  selector: 'app-home',
  standalone: false,
  templateUrl: './home.html',
  styleUrl: './home.css'
})
export class Home {
  loadingShopping = false;
  isFallback = true;
  shoppingCategories: ShoppingCategory[] = this.buildFallback();

  private buildFallback(): ShoppingCategory[] {
    return [
      {
        title: 'Ramadan bundles',
        subtitle: '4 items',
        meals: [
          { name: 'Iftar box classic', price: '12.00 BAM', badge: 'Featured' },
          { name: 'Family iftar platter', price: '29.00 BAM' },
          { name: 'Date and soup combo', price: '8.50 BAM' },
          { name: 'Samosa snack set', price: '7.00 BAM' }
        ]
      },
      {
        title: 'Grill favorites',
        subtitle: '4 items',
        meals: [
          { name: 'Chicken wrap', price: '9.00 BAM' },
          { name: 'Mixed grill plate', price: '18.00 BAM', badge: 'Top' },
          { name: 'Beef burger meal', price: '11.00 BAM' },
          { name: 'Cevapi in somun', price: '10.00 BAM' }
        ]
      },
      {
        title: 'Desserts and drinks',
        subtitle: '4 items',
        meals: [
          { name: 'Baklava trio', price: '6.00 BAM' },
          { name: 'Fresh lemonade', price: '4.00 BAM' },
          { name: 'Trilece cake', price: '5.50 BAM' },
          { name: 'Mint yogurt drink', price: '3.50 BAM' }
        ]
      }
    ];
  }
}
