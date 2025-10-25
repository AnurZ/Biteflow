import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Header } from './header/header';
import { Home } from './home/home';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { PublicRoutingModule } from './public-routing.module';
import {MatButtonModule} from '@angular/material/button';

@NgModule({
  declarations: [
    Header,
    Home
  ],
  exports: [
    Header
  ],
  imports: [
    CommonModule,
    RouterLink,
    RouterLinkActive,
    PublicRoutingModule,
    MatButtonModule,
  ]
})
export class PublicModule { }
