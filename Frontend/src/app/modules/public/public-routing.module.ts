import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { Home } from './home/home';
import {AdminLayout} from '../admin/admin-layout';
import {AdminModule} from '../admin/admin-module';
import {StaffList} from '../admin/staff/staff-list/staff-list';

const routes: Routes = [
  { path: 'admin', component: AdminLayout },
  { path: '', component: Home }  // /public → Home
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class PublicRoutingModule {}
