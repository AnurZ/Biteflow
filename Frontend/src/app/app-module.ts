import { NgModule, provideBrowserGlobalErrorListeners } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { HttpClientModule } from '@angular/common/http';
import { ReactiveFormsModule } from '@angular/forms';
import { OAuthModule } from 'angular-oauth2-oidc';

import { AppRoutingModule } from './app-routing-module';
import { App } from './app';
import { SharedModule } from './modules/shared/shared.module';
import { PublicModule } from './modules/public/public-module';
import { InventoryItems } from './modules/inventory-items/inventory-items';
import { Meals } from './modules/meals/meals';
import { AdminLayout } from './modules/admin/admin-layout';
import {
  InventoryItemsFormDialogComponent
} from './modules/inventory-items/inventory-items-form-dialog/inventory-items-form-dialog';
import {MealsFormDialog} from './modules/meals/meals-form-dialog/meals-form-dialog';
import {MatCheckbox} from '@angular/material/checkbox';
import {AddIngredientsDialog} from './modules/meals/meals-form-dialog/add-ingredients-dialog/add-ingredients-dialog';
import {MatTableModule} from '@angular/material/table';
import {MealcategoryFormDialog} from './modules/meals/MealCategory-form-dialog/mealcategory-form-dialog';

@NgModule({
  declarations: [App, AdminLayout, MealsFormDialog, MealcategoryFormDialog, AddIngredientsDialog, InventoryItemsFormDialogComponent],
  imports: [
    BrowserModule,
    HttpClientModule,
    AppRoutingModule,
    ReactiveFormsModule,
    OAuthModule.forRoot({
      resourceServer: {
        allowedUrls: ['https://localhost:7260/api'],
        sendAccessToken: true
      }
    }),
    SharedModule,
    PublicModule,
    MatCheckbox,
    MatTableModule
  ],
  providers: [
    provideBrowserGlobalErrorListeners(),
  ],
  bootstrap: [App]
})
export class AppModule {}
