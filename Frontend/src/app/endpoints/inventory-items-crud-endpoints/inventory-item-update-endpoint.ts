import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BaseEndpointAsync } from '../../helper/base-endpoint-async';
import { Observable } from 'rxjs';
import { UpdateInventoryItemDto } from '../../modules/inventory-items/inventory-item-model';
import { MyConfig } from '../../my-config';

@Injectable({ providedIn: 'root' })
export class InventoryItemUpdateEndpoint implements BaseEndpointAsync<UpdateInventoryItemDto, void> {
  private base = `${MyConfig.api_address}/inventoryitem`;
  constructor(private http: HttpClient) {}

  handleAsync(body: UpdateInventoryItemDto): Observable<void> {
    return this.http.put<void>(`${this.base}/${body.id}`, body);
  }
}
