import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BaseEndpointAsync } from '../../helper/base-endpoint-async';
import { Observable } from 'rxjs';
import { CreateInventoryItemDto } from '../../modules/inventory-items/inventory-item-model';
import { MyConfig } from '../../my-config';

@Injectable({ providedIn: 'root' })
export class InventoryItemCreateEndpoint implements BaseEndpointAsync<CreateInventoryItemDto, { id: number }> {
  private base = `${MyConfig.api_address}/inventoryitem`;
  constructor(private http: HttpClient) {}

  handleAsync(body: CreateInventoryItemDto): Observable<{ id: number }> {
    return this.http.post<{ id: number }>(this.base, body);
  }
}
