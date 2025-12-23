import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatSnackBarModule } from '@angular/material/snack-bar';
import { WaiterComponent } from './waiter.component';

@NgModule({
  declarations: [WaiterComponent],
  imports: [
    CommonModule,
    FormsModule,
    MatSnackBarModule
  ],
  exports: [WaiterComponent]
})
export class WaiterModule { }
