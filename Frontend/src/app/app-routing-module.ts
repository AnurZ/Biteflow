import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import {AdminLayout} from './modules/admin/admin-layout';
import {AdminModule} from './modules/admin/admin-module';
import { ActivationConfirmComponent } from './modules/public/activation-confirm/activation-confirm.component';
import { WaiterComponent } from './modules/waiter/waiter.component';
import { KitchenComponent } from './modules/kitchen/kitchen.component';
import { StaffGuard } from './modules/core/auth/staff.guard';

const routes: Routes = [
  {
    path: 'auth',
    loadChildren: () => import('./modules/core/auth/auth-module').then(m => m.AuthModule)
  },
  {
    path: 'public',
    loadChildren: () => import('./modules/public/public-module').then(m => m.PublicModule)
  },
  {
    path: 'admin',
    loadChildren: () => import('./modules/admin/admin-module').then(m => m.AdminModule)
  },
  {
    path: 'waiter',
    component: WaiterComponent,
    canActivate: [StaffGuard]
  },
  {
    path: 'kitchen',
    component: KitchenComponent,
    canActivate: [StaffGuard]
  },
  {
    path: 'activate',
    component: ActivationConfirmComponent
  },

  { path: 'activation/wizard', redirectTo: 'public/activation/wizard', pathMatch: 'full' },

  {path: '**', redirectTo: 'public', pathMatch: 'full'}
];



@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
