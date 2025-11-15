import { UnitTypes} from '../modules/meals/meals-model';

// Convert enum number to readable string (for UI)
export function mapUnitTypeToString(unitType: string | number | undefined): string {
  if (unitType === undefined || unitType === null) return UnitTypes[UnitTypes.Unit]; // default: "Unit"
  return typeof unitType === 'number' ? UnitTypes[unitType] : unitType;
}

// Convert string (UI) to enum number (for API)
export function mapUnitTypeToNumber(unitType: string | number): number {
  return typeof unitType === 'string'
    ? UnitTypes[unitType as keyof typeof UnitTypes]
    : unitType;
}
