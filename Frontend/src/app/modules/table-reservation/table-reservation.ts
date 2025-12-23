import {Component, ElementRef, inject, OnInit, ViewChild} from '@angular/core';
import {
  TableReservationGetListEndpoint
} from '../../endpoints/table-reservation-endpoints/table-reservation-get-list-endpoint';
import {
  TableReservationDeleteEndpoint
} from '../../endpoints/table-reservation-endpoints/table-reservation-delete-endpoint';
import {TableLayoutCreateEndpoint} from '../../endpoints/table-layout-endpoints/table-layout-create-endpoint';
import {
  TableReservationCreateEndpoint
} from '../../endpoints/table-reservation-endpoints/table-reservation-create-endpoint';
import {
  TableReservationUpdateEndpoint
} from '../../endpoints/table-reservation-endpoints/table-reservation-update-endpoint';
import {TableLayoutGetListEndpoint} from '../../endpoints/table-layout-endpoints/table-layout-get-list-endpoint';
import {DiningTableGetListEndpoint} from '../../endpoints/dining-table-endpoints/dining-table-get-list-endpoint';
import {
  DiningTableStatusDto,
  GetTableLayoutDto,
  GetTableLayoutListDto,
  TableStatus,
  UpdateDiningTableDto
} from '../table-layout/table-layout-model';
import {TableLayoutGetPreviewEndpoint} from '../../endpoints/table-layout-endpoints/table-layout-get-endpoint';
import {GetTableReservationsQueryDto, TableReservationStatus} from './table-reservation-model';
import {TableReservationFormDialog} from './table-reservation-form-dialog/table-reservation-form-dialog';
import {MatDialog} from '@angular/material/dialog';
import {HttpClient} from '@angular/common/http';
import {
  DiningTableGetTableLayoutIdEndpoint
} from '../../endpoints/dining-table-endpoints/dining-table-get-table-layout-by-id';
import {
  TableLayoutGetNameByIdEndpoint
} from '../../endpoints/table-layout-endpoints/table-layout-get-name-by-id-endpoint';
import {map, switchMap} from 'rxjs';
import {ConfirmDialogComponent} from '../admin/staff/confirm-dialog/confirm-dialog-component';
import {
  TableReservationUpdateStatusEndpoint
} from '../../endpoints/table-reservation-endpoints/table-reservation-update-status-endpoint';
import {
  DiningTablesGetStatusEndpoint
} from '../../endpoints/dining-table-endpoints/dining-table-get-table-status-endpoint';

@Component({
  selector: 'app-table-reservation',
  standalone: false,
  templateUrl: './table-reservation.html',
  styleUrl: './table-reservation.css'
})
export class TableReservation implements OnInit {
private tableReservationGetListEndpoint = inject(TableReservationGetListEndpoint);
private tableReservationDeleteEndpoint = inject(TableReservationDeleteEndpoint);
private tableReservationCreateEndpoint = inject(TableReservationCreateEndpoint);
private tableReservationUpdateEndpoint = inject(TableReservationUpdateEndpoint);
private tableLayoutGetListEndpoint = inject(TableLayoutGetListEndpoint);
private diningTableGetListEndpoint = inject(DiningTableGetListEndpoint);
private diningTableGetTableLayoutIdEndpoint = inject(DiningTableGetTableLayoutIdEndpoint);
  private tableLayoutGetNameByIdEndpoint = inject(TableLayoutGetNameByIdEndpoint);
  private tableReservationUpdateStatusEndpoint = inject(TableReservationUpdateStatusEndpoint);
  private diningTableGetStatusEndpoint = inject(DiningTablesGetStatusEndpoint)

  statusColors: Record<TableStatus, string> = {
    [TableStatus.Free]: '#66BB6A',        // medium soft green
    [TableStatus.Occupied]: '#EF5350',    // medium soft red
    [TableStatus.Reserved]: '#42A5F5',    // medium soft blue
    [TableStatus.Cleaning]: '#FDD835',    // medium soft yellow
    [TableStatus.OutOfService]: '#9E9E9E' // medium gray
  };

  selectedLayout?: GetTableLayoutListDto;
  selectedTable?: UpdateDiningTableDto;
  tableLayouts: GetTableLayoutListDto[] = [];
  diningTables: UpdateDiningTableDto[] = [];
  tableReservations: GetTableReservationsQueryDto[] = [];
  isLoadingTables: boolean = false;
  allTableReservations: GetTableReservationsQueryDto[] = [];




  filters = {
    search: '',
    date: new Date(),
    tableLayoutId: null as number | null,
    diningTableId: null as number | null,
    status: null as TableReservationStatus | null,
    showPast: false as boolean,
    timeFrom: null as string | null,
    timeTo: null as string | null,
    onlySelectedLayout: true as boolean,
  };

  constructor(
    private dialog: MatDialog
  ) {}

  @ViewChild('layoutsContainer', { static: false })
  layoutsContainer!: ElementRef<HTMLDivElement>;

  scrollLayouts(amount: number) {
    this.layoutsContainer.nativeElement.scrollBy({
      left: amount,
      behavior: 'smooth'
    });
  }


  applyFilters(): void {
    const now = new Date();

    this.tableReservations = this.allTableReservations.filter(r => {
      // --- Past / upcoming ---
      if (!this.filters.showPast) {
        const end = r.reservationEnd ? new Date(r.reservationEnd) : new Date(r.reservationStart);
        if (end < now) return false;
      }

      // --- Search by name ---
      if (this.filters.search) {
        const term = this.filters.search.toLowerCase();
        const fullName = `${r.firstName} ${r.lastName}`.toLowerCase();
        if (!fullName.includes(term)) return false;
      }

      // Inside applyFilters()
      if (this.filters.timeFrom || this.filters.timeTo) {
        const resStart = new Date(r.reservationStart);
        const resEnd = r.reservationEnd ? new Date(r.reservationEnd) : resStart;

        // Base date: either selected date or reservation date
        const baseDate = this.filters.date || resStart;

        // Convert HH:mm to Date objects on the same day
        let filterFrom: Date | null = null;
        let filterTo: Date | null = null;

        if (this.filters.timeFrom) {
          const [h, m] = this.filters.timeFrom.split(':').map(Number);
          filterFrom = new Date(baseDate);
          filterFrom.setHours(h, m, 0, 0);
        }

        if (this.filters.timeTo) {
          const [h, m] = this.filters.timeTo.split(':').map(Number);
          filterTo = new Date(baseDate);
          filterTo.setHours(h, m, 59, 999); // include entire last minute
        }

        // Correct overlapping logic:
        if (filterFrom && filterTo) {
          // Only include if reservation overlaps the selected range
          if (resEnd < filterFrom || resStart > filterTo) return false;
        } else if (filterFrom) {
          if (resEnd < filterFrom) return false;
        } else if (filterTo) {
          if (resStart > filterTo) return false;
        }
      }


      // --- Filter by table layout ---
      if (
        this.filters.tableLayoutId !== null &&
        r.tableLayoutId !== this.filters.tableLayoutId &&
        this.filters.onlySelectedLayout === true
      ) {
        return false;
      }


      // --- Filter by dining table ---
      if (this.filters.diningTableId) {
        if (r.diningTableId !== this.filters.diningTableId) return false;
      }

      // --- Filter by status ---
      if (this.filters.status !== null) {
        if (r.status !== this.filters.status) return false;
      }

      return true;
    });
  }

  tableHasUpcomingReservation(table: UpdateDiningTableDto | DiningTableStatusDto) {
    if (!this.tableReservations || !this.selectedDate || !this.selectedTime) return false;

    // Get the current computed status
    const currentStatus = this.computeTableStatus(table);
    if (currentStatus !== TableStatus.Free) return false;

    // Parse selectedTime, expected format: "HH:mm"
    const [hours, minutes] = this.selectedTime.split(':').map(Number);

    // Combine date and time into a single Date object
    const selectedDateTime = new Date(this.selectedDate);
    selectedDateTime.setHours(hours, minutes, 0, 0);

    // Only consider reservations starting after the selected time
    return this.allTableReservations.some(r => {
      const start = new Date(r.reservationStart);

      const isSameTable = r.diningTableId === table.id;
      const isSameDate = start.toDateString() === this.selectedDate.toDateString();
      const isAfterSelectedTime = start > selectedDateTime;

      return isSameTable && isSameDate && isAfterSelectedTime;
    });
  }






  isPastReservation(r: GetTableReservationsQueryDto): boolean {
    const now = new Date();

    const end = r.reservationEnd
      ? new Date(r.reservationEnd)
      : new Date(r.reservationStart);

    return end < now;
  }



  selectedDate: Date = new Date();   // today
  selectedTime: string = this.getNowTime(); // HH:mm

  get selectedMoment(): Date {
    const [h, m] = this.selectedTime.split(':').map(Number);
    const d = new Date(this.selectedDate);
    d.setHours(h, m, 0, 0);
    return d;
  }

  private getNowTime(): string {
    const now = new Date();
    return now.getHours().toString().padStart(2, '0') + ':' +
      now.getMinutes().toString().padStart(2, '0');
  }

  onDateTimeChange() {
    this.selectedDate = this.filters.date ?? '';
    this.selectedTime = this.filters.timeFrom ?? '';

    this.updateTableStates();
    this.updateAllTableStates();
  }


  async onStatusChange(reservation: any, newStatus: number) {
    try {
      await this.tableReservationUpdateStatusEndpoint.handleAsync({
        id: reservation.id,
        status: newStatus
      });
      console.log(`Reservation ${reservation.id} status updated to ${newStatus}`);
      reservation.status = newStatus;
    } catch (err) {
      console.error('Failed to update status', err);
    }
  }





  tableStatuses: Record<number, TableStatus> = {};



  computeTableStatus(table: UpdateDiningTableDto | DiningTableStatusDto): TableStatus {
    const t = this.selectedMoment.getTime();
    const preReservationMinutes = 30;
    const preReservationMs = preReservationMinutes * 60 * 1000;

    const isReserved = this.allTableReservations.some(r => {
      if (r.diningTableId !== table.id) return false;
      const start = new Date(r.reservationStart).getTime() - preReservationMs;
      const end = r.reservationEnd ? new Date(r.reservationEnd).getTime() : start + 90*60*1000;
      return t >= start && t < end;
    });

    if (isReserved) return TableStatus.Reserved;

    // fallback to real status from backend
    return table.status !== undefined ? table.status : TableStatus.Free;
  }



  updateTableStates() {
    this.tableStatuses = {};
    for (const table of this.diningTables) {
      this.tableStatuses[table.id!] = this.computeTableStatus(table);
    }
  }




  loadAllTables() {
    this.isLoadingTables = true;
    this.diningTableGetStatusEndpoint.handleAsync(this.selectedLayout?.id)
      .subscribe(t => {
        this.isLoadingTables = false;
        this.allDiningTables = t;
        this.updateTableStates();
        console.log("Loaded dining tables:", t);
      }, err => {
        this.isLoadingTables = false;
        console.error("Failed to load tables", err);
      });
  }

  allTableStatuses: Record<number, TableStatus> = {};
  allDiningTables :DiningTableStatusDto[] = [];
  allTableComputedStatuses: Record<number, TableStatus> = {};

  updateAllTableStates() {
    this.allTableComputedStatuses = {};
    for (const table of this.allDiningTables) {
      this.allTableComputedStatuses[table.id!] = this.computeTableStatus(table);
    }
  }


  getNumberOfFreeTables(layoutId: number): number {
    this.updateAllTableStates();
    return this.allDiningTables.filter(table =>
      table.tableLayoutId === layoutId &&
      this.allTableComputedStatuses[table.id!] !== TableStatus.Free
    ).length;
  }





  delete(id: number) {
    const ref = this.dialog.open(ConfirmDialogComponent, {
      data: { title: 'Delete table reservation member?', message: 'This cannot be undone.' }
    });
    ref.afterClosed().subscribe(ok => {
      if (ok) this.tableReservationDeleteEndpoint.handleAsync(id).subscribe(() => this.getTableReservations());
    });
  }






  getTableLayouts() {
    this.tableLayoutGetListEndpoint.handleAsync()
      .subscribe(t => {
        this.tableLayouts = t;
        console.log("Loaded layouts:", this.tableLayouts);

        if (this.tableLayouts && this.tableLayouts.length > 0) {
          this.selectedLayout = this.tableLayouts[0];
          this.filters.tableLayoutId = this.selectedLayout.id; // ✅
          this.getDiningTables();
          this.applyFilters();
        }
      });
  }


  getDiningTables(){
    this.isLoadingTables = true;
    this.diningTableGetListEndpoint.handleAsync({tableLayoutId:this.selectedLayout?.id})
      .subscribe(t => {
        this.diningTables = t;
        this.isLoadingTables = false;
        this.updateTableStates();
        console.log("Loaded dining tables:", this.diningTables);
      })
  }

  getTableReservations() {
    const baseDate = new Date(this.filters.date); // selected day
    let start: Date;
    let end: Date;

    // --- Start ---
    if (this.filters.timeFrom) {
      const [h, m] = this.filters.timeFrom.split(':').map(Number);
      start = new Date(baseDate);
      start.setHours(h, m, 0, 0);
    } else {
      start = new Date(baseDate);
      start.setHours(0, 0, 0, 0);
    }

    // --- End ---
    if (this.filters.timeTo) {
      const [h, m] = this.filters.timeTo.split(':').map(Number);
      end = new Date(baseDate);
      end.setHours(h, m, 59, 999);
    } else {
      end = new Date(baseDate);
      end.setHours(23, 59, 59, 999);
    }


    this.tableReservations = [];

    // send ISO strings to backend
    this.tableReservationGetListEndpoint.handleAsync({
      requestedStart: start.toISOString(),
      requestedEnd: end.toISOString()
    })
      .subscribe(t => {
        // sort by reservationStart ascending
        this.allTableReservations = t.sort((a, b) =>
          new Date(a.reservationStart).getTime() - new Date(b.reservationStart).getTime()
        );

        this.tableReservations = this.allTableReservations;

        this.applyFilters();
        this.updateTableStates();
        this.UpdateSelectedTableReservations();
      });
  }




  ngOnInit() {

    this.getTableLayouts();
    this.getTableReservations();
    this.loadAllTables();
  }


  selectLayout(layout: GetTableLayoutListDto) {
    this.selectedLayout = layout;

    this.filters.tableLayoutId = layout.id;

    this.getDiningTables();
    this.applyFilters();
    this.selectedTableReservations = [];
  }


  selectTable(table:UpdateDiningTableDto){
    this.selectedTable = table;
  }

  deselectTable(event: MouseEvent) {
    if ((event.target as HTMLElement).classList.contains('table-layout-full')) {
      this.selectedTable = undefined;
      this.selectedTableReservations = [];
    }
  }

  createTableReservation() {
    this.openReservationDialog('create');
  }

  editReservation(reservation: GetTableReservationsQueryDto) {
    const start = new Date(reservation.reservationStart);

    const reservationDate = new Date(
      start.getFullYear(),
      start.getMonth(),
      start.getDate()
    );

    const reservationStartTime =
      start.getHours().toString().padStart(2, '0') +
      ':' +
      start.getMinutes().toString().padStart(2, '0');

    this.diningTableGetTableLayoutIdEndpoint
      .handleAsync(reservation.diningTableId)
      .subscribe(res => {
        this.openReservationDialog('edit', {
          ...reservation,
          reservationDate,
          reservationStartTime,
          tableLayoutId: res.tableLayoutId,
          diningTableId: reservation.diningTableId
        });
      });
  }





  showFullNotes = false;

  toggleNotes() {
    this.showFullNotes = !this.showFullNotes;
  }



  openReservationDialog(mode: 'create' | 'edit' | 'createForSelectedTable', existingData: any = null) {

    this.tableLayoutGetListEndpoint.handleAsync().subscribe(layouts => {
      this.diningTableGetListEndpoint.handleAsync().subscribe(tables => {

        const dialogRef = this.dialog.open(TableReservationFormDialog, {
          width: '700px',
          data: {
            mode,
            layouts,
            tables,
            ...existingData,
            diningTableId: existingData?.diningTableId ?? this.selectedTable?.id ?? null
          }
        });

        dialogRef.afterClosed().subscribe(success => {
          if (!success) return;


          this.getTableReservations();
          this.UpdateSelectedTableReservations();
        });


      });
    });
  }

  selectedTableReservations :GetTableReservationsQueryDto[] = [];

  UpdateSelectedTableReservations() {
    if (!this.selectedTable) {
      this.selectedTableReservations = [];
      return;
    }

    this.selectedTableReservations = this.allTableReservations.filter(t =>
      t.diningTableId === this.selectedTable!.id
    );
  }

  onTableClick(table: any, event: MouseEvent): void {
    event.stopPropagation();
    this.selectedTable = table;
    this.UpdateSelectedTableReservations();
    console.log(this.selectedTableReservations);
  }

  createReservationForSelectedTable(){
    this.openReservationDialog('createForSelectedTable', {
      tableLayoutId: this.selectedLayout!.id,
      diningTableId: this.selectedTable!.id,
      tableLayoutName: this.selectedLayout!.name,
      diningTableNumber: this.selectedTable?.number
    });
  }




  protected readonly TableReservationStatus = TableReservationStatus;
}
