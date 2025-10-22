import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Login } from './login/login';



@NgModule({
  declarations: [
    Login
  ],
  imports: [
    CommonModule
  ]
})
export class AuthModule { }
