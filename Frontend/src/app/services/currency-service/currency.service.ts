import { Injectable } from '@angular/core';
import {
  APP_CURRENCY,
  APP_LOCALE} from '../../helper/currency.helper';


@Injectable({
  providedIn: 'root'
})
export class CurrencyService {

  format(value: number | null | undefined): string {
    if (value === null || value === undefined) {
      return '';
    }

    return new Intl.NumberFormat(APP_LOCALE, {
      style: 'currency',
      currency: APP_CURRENCY
    }).format(value);
  }
}
