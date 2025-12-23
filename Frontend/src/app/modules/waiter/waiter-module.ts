import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { WaiterComponent } from './waiter.component';

@NgModule({
  declarations: [WaiterComponent],
  imports: [
    CommonModule
  ],
  exports: [WaiterComponent]
})
export class WaiterModule { }
