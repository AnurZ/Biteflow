import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { InventoryItemCreateEndpoint } from '../../endpoints/inventory-items-crud-endpoints/inventory-item-create-endpoint';
import { InventoryItemUpdateEndpoint } from '../../endpoints/inventory-items-crud-endpoints/inventory-item-update-endpoint';
import { InventoryItemDeleteEndpoint } from '../../endpoints/inventory-items-crud-endpoints/inventory-item-delete-endpoint';
import { InventoryItemGetByIdEndpoint } from '../../endpoints/inventory-items-crud-endpoints/inventory-item-getbyid-endpoint';
import { InventoryItemListEndpoint } from '../../endpoints/inventory-items-crud-endpoints/inventory-item-list-endpoint';
import {
  CreateInventoryItemDto,
  UpdateInventoryItemDto,
  DeleteInventoryItemDto,
  GetInventoryItemByIdDto,
  ListInventoryItemsDto,
  PageResult, GetInventoryItemByNameDto
} from '../inventory-items/inventory-item-model';
import {InventoryItems} from './inventory-items';
import {
  InventoryItemGetByNameEndpoint
} from '../../endpoints/inventory-items-crud-endpoints/inventory-item-getbyname-endpoint';
@Injectable({ providedIn: 'root' })
export class InventoryItemService {
  constructor(
    private createEndpoint: InventoryItemCreateEndpoint,
    private updateEndpoint: InventoryItemUpdateEndpoint,
    private deleteEndpoint: InventoryItemDeleteEndpoint,
    private getByIdEndpoint: InventoryItemGetByIdEndpoint,
    private listEndpoint: InventoryItemListEndpoint,
    private getByNameEndpoint: InventoryItemGetByNameEndpoint
  ) {}

  create(item: CreateInventoryItemDto): Observable<{ id: number }> {
    return this.createEndpoint.handleAsync(item);
  }

  update(item: UpdateInventoryItemDto): Observable<void> {
    return this.updateEndpoint.handleAsync(item);
  }

  delete(id: number): Observable<void> {
    return this.deleteEndpoint.handleAsync({ id });
  }

  getById(id: number): Observable<GetInventoryItemByIdDto> {
    return this.getByIdEndpoint.handleAsync({ id });
  }

  list(): Observable<PageResult<ListInventoryItemsDto>> {
    return this.listEndpoint.handleAsync();
    }

  getByName(name: string): Observable<PageResult<GetInventoryItemByNameDto>> {
    return this.getByNameEndpoint.handleAsync({ name });
  }

}

