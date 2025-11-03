import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BaseEndpointAsync } from '../../helper/base-endpoint-async';
import { Observable } from 'rxjs';
import { GetInventoryItemByIdDto } from '../../modules/inventory-items/inventory-item-model';
import { MyConfig } from '../../my-config';

@Injectable({ providedIn: 'root' })
export class InventoryItemGetByIdEndpoint implements BaseEndpointAsync<{ id: number }, GetInventoryItemByIdDto> {
  private base = `${MyConfig.api_address}/inventoryitem`;
  constructor(private http: HttpClient) {}

  handleAsync(body: { id: number }): Observable<GetInventoryItemByIdDto> {
    return this.http.get<GetInventoryItemByIdDto>(`${this.base}/${body.id}`);
  }
}
