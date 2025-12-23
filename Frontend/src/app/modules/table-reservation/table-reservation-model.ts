export interface GetTableReservationsQueryDto {
  id: number;
  diningTableId: number;
  diningTableNumber: number;
  tableLayoutName: string;
  tableLayoutId: number;
  applicationUserId?: string | null; // Guid? → string | null
  firstName: string;
  lastName: string;
  email?: string;
  phoneNumber: string;
  numberOfGuests: number;
  notes?: string | null;
  reservationStart: string;          // DateTime → ISO string
  reservationEnd?: string | null;    // DateTime? → string | null
  status: TableReservationStatus;
}

export interface CreateTableReservationCommandDto {
  diningTableId: number;
  numberOfGuests: number;
  reservationStart: string; // DateTime

  applicationUserId?: string | null;
  reservationEnd?: string | null;
  notes?: string | null;

  firstName: string;
  lastName: string;
  email?: string;
  phoneNumber: string;
}

export interface TableReservationFilter {
  reservationId?: number;       // optional
  diningTableId?: number;       // optional
  requestedStart?: string;      // ISO datetime string
  requestedEnd?: string;        // ISO datetime string
}


export enum TableReservationStatus {
  "Pending" = 0,
  "Confirmed" = 1,
  "Completed" = 2,
  "Cancelled" = 3,
  "NoShow" = 4,
}
