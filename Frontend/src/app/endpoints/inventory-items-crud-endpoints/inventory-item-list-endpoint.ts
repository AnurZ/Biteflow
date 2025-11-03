import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BaseEndpointAsync } from '../../helper/base-endpoint-async';
import { Observable } from 'rxjs';
import { PageResult, ListInventoryItemsDto } from '../../modules/inventory-items/inventory-item-model';
import { MyConfig } from '../../my-config';

@Injectable({ providedIn: 'root' })
export class InventoryItemListEndpoint implements BaseEndpointAsync<void, PageResult<ListInventoryItemsDto>> {
  private base = `${MyConfig.api_address}/inventoryitem`;
  constructor(private http: HttpClient) {}

  handleAsync(): Observable<PageResult<ListInventoryItemsDto>> {
    return this.http.get<PageResult<ListInventoryItemsDto>>(this.base);
  }
}
