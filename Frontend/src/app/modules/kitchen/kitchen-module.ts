import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { KitchenComponent } from './kitchen.component';

@NgModule({
  declarations: [KitchenComponent],
  imports: [
    CommonModule
  ],
  exports: [KitchenComponent]
})
export class KitchenModule { }
