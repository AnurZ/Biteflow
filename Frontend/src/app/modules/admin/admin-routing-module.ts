import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AdminLayout } from './admin-layout';
import { StaffList } from './staff/staff-list/staff-list';
import {Meals} from '../meals/meals';
import {InventoryItems} from '../inventory-items/inventory-items';
import {MealsStats} from '../meals/Stats/meals-stats';
import { ActivationRequestsListComponent } from './activation-requests/activation-requests-list.component';
const routes: Routes = [
  {
    path: '',
    component: AdminLayout,
    children: [
      { path: 'staff', component: StaffList },
      {
        path: 'meals',
        loadChildren: () => import('../meals/meals-module').then(m => m.MealsModule)
      },
      { path: 'inventory-items', component: InventoryItems },
      { path: 'activation-requests', component: ActivationRequestsListComponent },
      { path: '', redirectTo: 'admin', pathMatch: 'full' }
    ]
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class AdminRoutingModule {}
