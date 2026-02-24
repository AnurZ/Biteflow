import { Component, OnInit, inject } from '@angular/core';
import { forkJoin } from 'rxjs';
import { MealCategoryGetEndpoint } from '../../../endpoints/meal-category-crud-endpoint/meal-category-get-endpoint';
import { MealsService } from '../../meals/meals-service';
import { MealCategory, MealDto } from '../../meals/meals-model';

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
export class Home implements OnInit {
  private readonly mealsService = inject(MealsService);
  private readonly categoryEndpoint = inject(MealCategoryGetEndpoint);

  private readonly currency = new Intl.NumberFormat('bs-BA', {
    style: 'currency',
    currency: 'BAM',
    minimumFractionDigits: 2,
    maximumFractionDigits: 2
  });

  loadingShopping = true;
  isFallback = false;
  shoppingCategories: ShoppingCategory[] = [];

  ngOnInit(): void {
    this.loadShopping();
  }

  private loadShopping(): void {
    this.loadingShopping = true;

    forkJoin({
      meals: this.mealsService.getMeals(),
      categories: this.categoryEndpoint.handleAsync()
    }).subscribe({
      next: ({ meals, categories }) => {
        const mapped = this.mapFromApi(meals, categories);
        if (mapped.length > 0) {
          this.shoppingCategories = mapped;
          this.isFallback = false;
        } else {
          this.shoppingCategories = this.buildFallback();
          this.isFallback = true;
        }
        this.loadingShopping = false;
      },
      error: () => {
        this.shoppingCategories = this.buildFallback();
        this.isFallback = true;
        this.loadingShopping = false;
      }
    });
  }

  private mapFromApi(meals: MealDto[] | null | undefined, categories: MealCategory[] | null | undefined): ShoppingCategory[] {
    if (!meals || meals.length === 0) return [];

    const categoryName = new Map<number, string>();
    for (const category of categories ?? []) {
      categoryName.set(category.id, category.name);
    }

    const preferredMeals = meals.filter(x => x.isAvailable);
    const source = preferredMeals.length > 0 ? preferredMeals : meals;

    const grouped = new Map<number, MealDto[]>();
    for (const meal of source) {
      const key = meal.categoryId ?? -1;
      if (!grouped.has(key)) grouped.set(key, []);
      grouped.get(key)!.push(meal);
    }

    return Array.from(grouped.entries())
      .sort((a, b) => b[1].length - a[1].length)
      .slice(0, 6)
      .map(([categoryId, items]) => {
        const title = categoryName.get(categoryId) ?? 'Popular picks';
        return {
          title,
          subtitle: `${items.length} item${items.length === 1 ? '' : 's'}`,
          meals: items.slice(0, 5).map(meal => ({
            name: meal.name,
            price: this.currency.format(meal.basePrice ?? 0),
            badge: meal.isFeatured ? 'Featured' : undefined
          }))
        } satisfies ShoppingCategory;
      })
      .filter(x => x.meals.length > 0);
  }

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
