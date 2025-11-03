import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import {AdminLayout} from './modules/admin/admin-layout';
import {AdminModule} from './modules/admin/admin-module';

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
  { path: '**', redirectTo: 'public', pathMatch: 'full' }
];



@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
