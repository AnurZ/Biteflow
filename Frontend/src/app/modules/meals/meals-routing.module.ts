import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { Meals } from './meals';
import {MealsStats} from './Stats/meals-stats';

const routes: Routes = [
  {
    path: '',
    component: Meals,
    children: [
      { path: 'stats', component: MealsStats }, // /admin/meals/stats
      { path: '', redirectTo: '', pathMatch: 'full' } // optional default
    ]
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class MealsRoutingModule {}
