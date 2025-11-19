// inventory-item.model.ts
import {UnitTypes} from '../meals/meals-model';

export interface PageResult<T> {
  total: number;
  items: T[];
}


export interface InventoryItemDto {
  id: number;
  restaurantId: string; // GUIDs are strings in TS
  name: string;
  sku: string;
  unitType: UnitTypes;
  reorderQty: number;
  reorderFrequency: number;
  currentQty: number;
}

// For list queries
export type ListInventoryItemsDto = InventoryItemDto;

// For get by ID
export type GetInventoryItemByIdDto = InventoryItemDto;

export type GetInventoryItemByNameDto = InventoryItemDto;
// Create command
export interface CreateInventoryItemDto {
  name: string;
  sku: string;
  unitType: UnitTypes;
  reorderQty: number;
  reorderFrequency: number;
  currentQty: number;
}

// Update command
export interface UpdateInventoryItemDto {
  id: number;
  restaurantId: string;
  name: string;
  sku: string;
  unitType: UnitTypes;
  reorderQty: number;
  reorderFrequency: number;
  currentQty: number;
}

// Delete command (optional)
export interface DeleteInventoryItemDto {
  id: number;
}
