import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import {StaffList} from './staff/staff-list/staff-list';

const routes: Routes = [
  { path: 'staff', component: StaffList },
  { path: '', redirectTo: 'staff', pathMatch: 'full' }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class AdminRoutingModule {}
