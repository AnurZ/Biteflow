export interface PageResult<T> {
  total: number;
  items: T[];
}

export interface CreateTableLayoutDto {
  name: string;
  backgroundColor?: string;
  floorImageUrl?: string;
}

export interface GetTableLayoutDto {
  id: number;
  name: string;
  backgroundColor?: string;
  floorImageUrl?: string;
}

export interface GetTableLayoutListDto {
  id: number;
  name: string;
  backgroundColor?: string;
  floorImageUrl?: string;
  tables:TableDto[]
}

export interface TableDto {
  id: number;
  number: number;
  numberOfSeats: number;
  x:number;
  y:number;
  height:number;
  width:number;
  shape:string;
  color:string;
  tableType: TableType,
  status:TableStatus
  isActive:boolean;
}

export interface CreateDiningTableDto {
  tableLayoutId: number;
  number: number;
  numberOfSeats: number;
  x: number;
  y: number;
  height:number;
  width:number;
  shape: string;
  color: string;
  tableType: TableType;
  status: TableStatus;
}

export interface UpdateDiningTableDto {
  id?: number;
  tableLayoutId: number;
  number: number;
  numberOfSeats: number;
  x: number;
  y: number;
  height:number;
  width:number;
  shape: string;
  color: string;
  tableType: TableType;
  status: TableStatus;
  isActive: boolean;
  lastUsedAt: Date;
}

export interface DiningTableFilter {
  tableId?: number;
  tableLayoutId?: number;
  isActive?: boolean;
  status?: string;  // if you want
}

export interface DiningTableStatusDto {
  id: number;
  tableLayoutId: number;
  status: TableStatus; // static table status
}


export enum TableType {
  "Low Table" = 1,
  "High Table" = 2
}

export enum TableStatus {
  "Free" = 0,
  "Occupied" = 1,
  "Reserved" = 2,
  "Cleaning" = 3,
  "OutOfService" = 4,
}
