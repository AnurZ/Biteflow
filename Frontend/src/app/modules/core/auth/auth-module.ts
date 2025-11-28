import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Login } from './login/login';
import { LogoutComponent } from './logout/logout.component';
import { AuthCallbackComponent } from './callback/callback.component';
import { AuthRoutingModule } from './auth-routing.module';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { SharedModule } from '../../shared/shared.module';
import { MatDialogModule } from '@angular/material/dialog';
import { LogoutConfirmDialogComponent } from './logout/logout-confirm-dialog.component';
import { MatButtonModule } from '@angular/material/button';
import { ReactiveFormsModule } from '@angular/forms';

@NgModule({
  declarations: [
    Login,
    LogoutComponent,
    AuthCallbackComponent
  ],
  imports: [
    CommonModule,
    AuthRoutingModule,
    ReactiveFormsModule,
    MatSlideToggleModule,
    SharedModule,
    MatDialogModule,
    LogoutConfirmDialogComponent,
    MatButtonModule,
  ]
})
export class AuthModule { }
