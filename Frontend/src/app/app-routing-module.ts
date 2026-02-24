import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { ActivationConfirmComponent } from './modules/public/activation-confirm/activation-confirm.component';
import { WaiterComponent } from './modules/waiter/waiter.component';
import { KitchenComponent } from './modules/kitchen/kitchen.component';
import { SuperAdminGuard } from './modules/core/auth/superadmin.guard';
import { SuperAdminActivationRequestsComponent } from './modules/superadmin/superadmin-activation-requests.component';
import { WaiterGuard } from './modules/core/auth/waiter.guard';
import { KitchenGuard } from './modules/core/auth/kitchen.guard';
import { LoggedInGuard } from './modules/core/auth/logged-in.guard';

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
    path: 'superadmin',
    component: SuperAdminActivationRequestsComponent,
    canActivate: [SuperAdminGuard]
  },
  {
    path: 'waiter',
    component: WaiterComponent,
    canActivate: [WaiterGuard]
  },
  {
    path: 'kitchen',
    component: KitchenComponent,
    canActivate: [KitchenGuard]
  },
  {
    path: 'activate',
    component: ActivationConfirmComponent
  },
  {
    path: 'settings',
    canActivate: [LoggedInGuard],
    loadComponent: () => import('./modules/settings/settings.component').then(m => m.SettingsComponent)
  },

  { path: 'activation/wizard', redirectTo: 'public/activation/wizard', pathMatch: 'full' },

  {path: '**', redirectTo: 'public', pathMatch: 'full'}
];



@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
