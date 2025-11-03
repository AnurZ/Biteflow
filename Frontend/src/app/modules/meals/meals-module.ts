import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Meals } from './meals';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule } from '@angular/material/paginator';
import { MatButtonModule } from '@angular/material/button';
import { FormsModule } from '@angular/forms';
import {MealsRoutingModule} from './meals-routing.module';
import {MatIconModule} from '@angular/material/icon';

@NgModule({
  declarations: [
    Meals
  ],
  imports: [
    CommonModule,
    FormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatTableModule,
    MatPaginatorModule,
    MatButtonModule,
    MealsRoutingModule,
    CommonModule,
    MatIconModule
  ]
})
export class MealsModule {}
