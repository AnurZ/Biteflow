import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BaseEndpointAsync } from '../../helper/base-endpoint-async';
import { Observable } from 'rxjs';
import {PageResult, GetInventoryItemByNameDto} from '../../modules/inventory-items/inventory-item-model';
import { MyConfig } from '../../my-config';

@Injectable({ providedIn: 'root' })
export class InventoryItemGetByNameEndpoint implements BaseEndpointAsync<{ name: string }, PageResult<GetInventoryItemByNameDto>> {
  private base = `${MyConfig.api_address}/inventoryitem`;
  constructor(private http: HttpClient) {}

  handleAsync(body: { name: string }): Observable<PageResult<GetInventoryItemByNameDto>> {
    return this.http.get<PageResult<GetInventoryItemByNameDto>>(`${this.base}/by-name`, {
      params: { name: body.name }
    });
  }

}
