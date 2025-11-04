import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { Home } from './home/home';
import {TenantActivationComponent} from './tenant-activation/tenant-activation';
import {AdminLayout} from '../admin/admin-layout';
import {AdminModule} from '../admin/admin-module';
import {StaffList} from '../admin/staff/staff-list/staff-list';


const routes: Routes = [
  { path: '', component: Home },
  { path: 'admin', component: AdminLayout },
  { path: 'activation/wizard', component: TenantActivationComponent },


@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class PublicRoutingModule {}
