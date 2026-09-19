import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { BaseEndpointAsync } from '../../helper/base-endpoint-async';
import {
  PageResult,
  ListInventoryItemsDto,
  InventoryItemListRequest
} from '../../modules/inventory-items/inventory-item-model';
import { MyConfig } from '../../my-config';

@Injectable({ providedIn: 'root' })
export class InventoryItemListEndpoint
  implements BaseEndpointAsync<
    InventoryItemListRequest,
    PageResult<ListInventoryItemsDto>
  > {

  private base = `${MyConfig.api_address}/inventoryitem`;

  constructor(private http: HttpClient) {}

  handleAsync(
    req: InventoryItemListRequest
  ): Observable<PageResult<ListInventoryItemsDto>> {

    let params = new HttpParams()
      .set('paging.page', String(req.pageNumber))
      .set('paging.pageSize', String(req.pageSize));

    if (req.search) {
      params = params.set('search', req.search);
    }

    if (req.sort) {
      params = params.set('sort', req.sort);
    }

    if (req.unitType !== null && req.unitType !== undefined) {
      params = params.set('unitType', String(req.unitType));
    }

    if (req.minQty !== null && req.minQty !== undefined) {
      params = params.set('minQty', String(req.minQty));
    }

    if (req.maxQty !== null && req.maxQty !== undefined) {
      params = params.set('maxQty', String(req.maxQty));
    }

    return this.http.get<PageResult<ListInventoryItemsDto>>(
      this.base,
      { params }
    );
  }
}
