import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { Login } from './login/login';
import {Logout} from './logout/logout';

const routes: Routes = [
  { path: 'login', component: Login },
  { path: 'logout', component: Logout },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class AuthRoutingModule {}
