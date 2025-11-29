import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { LogoutComponent } from './logout/logout.component';
import { AuthCallbackComponent } from './callback/callback.component';
import { LoginGuard } from './login/login.guard';
import { RegisterComponent } from './register/register';

const routes: Routes = [
  { path: 'login', canActivate: [LoginGuard], pathMatch: 'full', loadComponent: () => import('./login/login').then(m => m.Login) },
  { path: 'register', component: RegisterComponent },
  { path: 'logout', component: LogoutComponent },
  { path: 'callback', component: AuthCallbackComponent }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class AuthRoutingModule {}
