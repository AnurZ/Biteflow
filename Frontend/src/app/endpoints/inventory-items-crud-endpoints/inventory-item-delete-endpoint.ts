import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BaseEndpointAsync } from '../../helper/base-endpoint-async';
import { Observable } from 'rxjs';
import { DeleteInventoryItemDto } from '../../modules/inventory-items/inventory-item-model';
import { MyConfig } from '../../my-config';

@Injectable({ providedIn: 'root' })
export class InventoryItemDeleteEndpoint implements BaseEndpointAsync<DeleteInventoryItemDto, void> {
  private base = `${MyConfig.api_address}/inventoryitem`;
  constructor(private http: HttpClient) {}

  handleAsync(body: DeleteInventoryItemDto): Observable<void> {
    return this.http.delete<void>(`${this.base}/${body.id}`);
  }
}
