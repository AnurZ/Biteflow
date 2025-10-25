import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Login } from './login/login';
import { LogoutComponent } from './logout/logout.component';
import { AuthRoutingModule } from './auth-routing.module';
import { ReactiveFormsModule } from '@angular/forms';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { SharedModule } from '../../shared/shared.module';
import { HttpClientModule } from '@angular/common/http';
import {MatDialogModule} from '@angular/material/dialog';
import {LogoutConfirmDialogComponent} from './logout/logout-confirm-dialog.component';
import {MatButtonModule} from '@angular/material/button';

@NgModule({
  declarations: [
    Login,
    LogoutComponent
  ],
  imports: [
    CommonModule,
    AuthRoutingModule,
    ReactiveFormsModule,
    HttpClientModule,
    MatSlideToggleModule,
    SharedModule,
    MatDialogModule,
    LogoutConfirmDialogComponent,
    MatButtonModule,
  ]
})
export class AuthModule { }
