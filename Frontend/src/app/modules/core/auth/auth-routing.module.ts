import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { Login } from './login/login';
import { LogoutComponent } from './logout/logout.component';
import { AuthCallbackComponent } from './callback/callback.component';

const routes: Routes = [
  { path: 'login', component: Login },
  { path: 'logout', component: LogoutComponent },
  { path: 'callback', component: AuthCallbackComponent }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class AuthRoutingModule {}
