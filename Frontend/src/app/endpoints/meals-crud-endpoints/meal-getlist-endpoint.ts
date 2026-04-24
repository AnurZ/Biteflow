import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { MyConfig } from '../../my-config';
import { MealDto, PageResult} from '../../modules/meals/meals-model';

@Injectable({ providedIn: 'root' })
export class MealGetListEndpoint {
  private base = `${MyConfig.api_address}/meal`;

  constructor(private http: HttpClient) {}

  handleAsync(page?: number, pageSize?: number, search?: string, sort?: string, categoryId?: number) {
    return this.http.get<PageResult<MealDto>>(this.base, {
      params: {
        search: search ?? '',
        sort: sort ?? '',
        categoryId: categoryId ?? 0,

        'paging.page': page ?? 1,
        'paging.pageSize': pageSize ?? 10
      }
    });
  }
}
