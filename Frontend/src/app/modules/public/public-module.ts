import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { RouterLink, RouterLinkActive } from '@angular/router';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';

import { MatStepperModule } from '@angular/material/stepper';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBarModule } from '@angular/material/snack-bar';
import { Header } from './header/header';
import { Home } from './home/home';
import { TenantActivationComponent } from './tenant-activation/tenant-activation';
import { PublicRoutingModule } from './public-routing.module';


@NgModule({
  declarations: [
    Header,
    Home,

  ],
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,

    PublicRoutingModule,

    RouterLink,
    RouterLinkActive,
    MatStepperModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatSnackBarModule,
    TenantActivationComponent,
  ],
  exports: [
    Header
  ]
})
export class PublicModule { }
