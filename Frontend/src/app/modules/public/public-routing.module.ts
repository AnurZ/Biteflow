import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { Home } from './home/home';
import { TenantActivationComponent } from './tenant-activation/tenant-activation';

const routes: Routes = [
  { path: '', component: Home },
  { path: 'activation/wizard', component: TenantActivationComponent },
  { path: '**', redirectTo: '' }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class PublicRoutingModule {}
