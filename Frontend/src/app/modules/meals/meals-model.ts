// meals.model.ts

export interface PageResult<T> {
  total: number;
  items: T[];
}

// UnitTypes enum (matches backend exactly)
export enum UnitTypes {
  Kilogram = 'Kilogram',
  Gram = 'Gram',
  Milligram = 'Milligram',

  Liter = 'Liter',
  Milliliter = 'Milliliter',

  Unit = 'Unit',
  Slice = 'Slice',

  Teaspoon = 'Teaspoon',
  Tablespoon = 'Tablespoon',
  Cup = 'Cup',
  Pinch = 'Pinch',
  Drop = 'Drop',

  Meter = 'Meter',
  Centimeter = 'Centimeter',

  Pack = 'Pack',
  Bottle = 'Bottle',
  Can = 'Can',
  Box = 'Box',
  Bag = 'Bag'
}

// =======================
// Meal DTOs for listing
// =======================
export interface MealDto {
  id: number;
  name: string;
  description: string;
  basePrice: number;
  isAvailable: boolean;
  isFeatured: boolean;
  imageField: string;
  ingredientsCount?: number; // optional
}

// =======================
// Meal DTO for single meal with ingredients
// =======================
export interface MealIngredientQueryDto {
  inventoryItemId: number;
  inventoryItemName: string;
  quantity: number;
  unitType: string; // could be typed as UnitTypes if backend returns enum name
}

export interface GetMealByNameDto {
  id: number;
  name: string;
  description: string;
  basePrice: number;
  isAvailable: boolean;
  isFeatured: boolean;
  imageField: string;
  ingredients: MealIngredientQueryDto[];
}

export interface GetMealByIdDto {
  id: number;
  name: string;
  description: string;
  basePrice: number;
  isAvailable: boolean;
  isFeatured: boolean;
  imageField: string;
  ingredients: MealIngredientQueryDto[];
}

// =======================
// Commands for creating/updating meals
// =======================
export interface MealIngredientDto {
  inventoryItemId: number;
  quantity: number;
  unitType: UnitTypes;
}

export interface CreateMealCommand {
  name: string;
  description: string;
  basePrice: number;
  isAvailable: boolean;
  isFeatured: boolean;
  imageField: string;
  stockManaged: boolean;
  ingredients: MealIngredientDto[];
}

export interface UpdateMealCommand {
  id: number;
  name: string;
  description: string;
  basePrice: number;
  isAvailable: boolean;
  isFeatured: boolean;
  imageField: string;
  stockManaged: boolean;
  ingredients: MealIngredientDto[];
}

// =======================
// Command for deleting a meal
// =======================
export interface DeleteMealCommand {
  id: number;
}
