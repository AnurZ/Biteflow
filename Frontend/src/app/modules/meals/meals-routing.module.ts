import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { Meals } from './meals';

const routes: Routes = [
  {
    path: '',
    component: Meals,
    children: [
      { path: '', redirectTo: '', pathMatch: 'full' }
    ]
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class MealsRoutingModule {}
