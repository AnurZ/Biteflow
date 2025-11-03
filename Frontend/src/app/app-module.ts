import { NgModule, provideBrowserGlobalErrorListeners } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { ReactiveFormsModule } from '@angular/forms';

import { AppRoutingModule } from './app-routing-module';
import { App } from './app';
import { SharedModule } from './modules/shared/shared.module';
import { AuthInterceptor } from './modules/core/auth/auth-interceptor';
import { PublicModule } from './modules/public/public-module';
import { InventoryItems } from './modules/inventory-items/inventory-items';
import { Meals } from './modules/meals/meals';
import { AdminLayout } from './modules/admin/admin-layout';
import { MealsStats } from './modules/meals/Stats/meals-stats';
import {
  InventoryItemsFormDialogComponent
} from './modules/inventory-items/inventory-items-form-dialog/inventory-items-form-dialog';
// src/app/app-module.ts
@NgModule({
  declarations: [App, AdminLayout, InventoryItemsFormDialogComponent],
  imports: [
    BrowserModule,
    HttpClientModule,
    AppRoutingModule,
    ReactiveFormsModule,
    SharedModule,
    PublicModule,
  ],
  providers: [
    provideBrowserGlobalErrorListeners(),
    { provide: HTTP_INTERCEPTORS, useClass: AuthInterceptor, multi: true }
  ],
  bootstrap: [App]
})
export class AppModule {}
