import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatSnackBarModule } from '@angular/material/snack-bar';
import { KitchenComponent } from './kitchen.component';

@NgModule({
  declarations: [KitchenComponent],
  imports: [
    CommonModule,
    MatSnackBarModule
  ],
  exports: [KitchenComponent]
})
export class KitchenModule { }
