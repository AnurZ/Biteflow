// meals.model.ts

export interface PageResult<T> {
  total: number;
  items: T[];
}

export interface MealCategory{
  id: number;
  name: string;
  description: string;
}

export interface MealCategoryCreateDto{
  name: string;
  description: string;
}

// UnitTypes enum (matches backend exactly)
export enum UnitTypes {
  Kilogram = 0,
  Gram = 1,
  Milligram = 2,

  Liter = 3,
  Milliliter = 4,

  Unit = 5,
  Slice = 6,

  Teaspoon = 7,
  Tablespoon = 8,
  Cup = 9,
  Pinch = 10,
  Drop = 11,

  Meter = 12,
  Centimeter = 13,

  Pack = 14,
  Bottle = 15,
  Can = 16,
  Box = 17,
  Bag = 18
}


// =======================
// Meal DTOs for listing
// =======================
export interface MealDto {
  id: number;
  name: string;
  description?: string;
  basePrice: number;
  isAvailable: boolean;
  isFeatured: boolean;
  imageField: string;
  categoryId: number;
  ingredientsCount?: number; // optional
}

export interface addIngredientDto{
  inventoryItemId: number;
  inventoryItemName: string;
  quantity: number;
  unitType: string;
  selected: boolean;
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
  ingredientsCount?: number;
  categoryId: number;
  ingredients: MealIngredientQueryDto[];
}

export interface GetMealByIdDto {
  id: number;
  name: string;
  description: string;
  basePrice: number;
  isAvailable: boolean;
  isFeatured: boolean;
  stockManaged: boolean;
  imageField: string;
  categoryId: number;
  ingredients: MealIngredientQueryDto[];
}

// =======================
// Commands for creating/updating meals
// =======================
export interface MealIngredientDto {
  inventoryItemId: number;
  inventoryItemName: string;
  quantity: number;
  unitType: UnitTypes;
}

export interface CreateMealCommand {
  name: string;
  description?: string;
  basePrice: number;
  isAvailable: boolean;
  isFeatured: boolean;
  imageField?: string;
  stockManaged: boolean;
  categoryId: number;
  ingredients: MealIngredientApiDto[];
}

export interface UpdateMealCommand {
  id: number;
  name: string;
  description?: string;
  basePrice: number;
  isAvailable: boolean;
  isFeatured: boolean;
  imageField?: string;
  stockManaged: boolean;
  categoryId: number;
  ingredients: MealIngredientApiDto[];
}

export interface MealIngredientApiDto {
  inventoryItemId: number;
  quantity: number;
  unitType: number; // or string depending on API
}


// =======================
// Command for deleting a meal
// =======================
export interface DeleteMealCommand {
  id: number;
}
