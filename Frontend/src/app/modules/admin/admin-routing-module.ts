import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AdminLayout } from './admin-layout';
import { StaffList } from './staff/staff-list/staff-list';
import {Meals} from '../meals/meals';
import {InventoryItems} from '../inventory-items/inventory-items';
import {MealsStats} from '../meals/Stats/meals-stats';
import {TableLayoutComponent} from '../table-layout/table-layout';
import {TableReservation} from '../table-reservation/table-reservation';
import {OrdersChartComponent} from './analytics/admin-dashboard/analyticsdashboard/orders-chart/orders-chart';
import {AdminDashboard} from './analytics/admin-dashboard/admin-dashboard';
const routes: Routes = [
  {
    path: '',
    component: AdminLayout,
    children: [
      { path: 'dashboard', component: AdminDashboard },
      { path: 'staff', component: StaffList },
      {
        path: 'meals',
        loadChildren: () => import('../meals/meals-module').then(m => m.MealsModule)
      },
      { path: 'inventory-items', component: InventoryItems },
      { path: 'table-reservations', component: TableReservation },
      { path: 'tables-layout', component: TableLayoutComponent },
      { path: 'analytics', component: OrdersChartComponent },
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' }
    ]
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class AdminRoutingModule {}
