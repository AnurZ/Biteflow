import { Component, inject, Inject, OnInit } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import {GetTableLayoutDto, TableStatus, UpdateDiningTableDto} from '../../table-layout/table-layout-model';
import { DiningTableGetListEndpoint } from '../../../endpoints/dining-table-endpoints/dining-table-get-list-endpoint';
import { TableLayoutGetPreviewEndpoint } from '../../../endpoints/table-layout-endpoints/table-layout-get-endpoint';
import {
  TableReservationGetListEndpoint
} from '../../../endpoints/table-reservation-endpoints/table-reservation-get-list-endpoint';
import {
  TableReservationCreateEndpoint
} from '../../../endpoints/table-reservation-endpoints/table-reservation-create-endpoint';
import {CreateTableReservationCommandDto, TableReservationStatus} from '../table-reservation-model';
import {
  TableReservationUpdateEndpoint
} from '../../../endpoints/table-reservation-endpoints/table-reservation-update-endpoint';
import {
  TableLayoutGetNameByIdDto, TableLayoutGetNameByIdEndpoint
} from '../../../endpoints/table-layout-endpoints/table-layout-get-name-by-id-endpoint';
import {
  DiningTablesGetStatusEndpoint
} from '../../../endpoints/dining-table-endpoints/dining-table-get-table-status-endpoint';

@Component({
  selector: 'app-table-reservation-form-dialog',
  standalone: false,
  templateUrl: './table-reservation-form-dialog.html',
  styleUrls: ['./table-reservation-form-dialog.css']
})
export class TableReservationFormDialog implements OnInit {
  private fb = inject(FormBuilder);
  private diningTableGetListEndpoint = inject(DiningTableGetListEndpoint);
  private tableLayoutGetPreviewEndpoint = inject(TableLayoutGetPreviewEndpoint);
  private tableReservationGetListEndpoint = inject(TableReservationGetListEndpoint);
  private tableReservationCreateEndpoint = inject(TableReservationCreateEndpoint);
  private tableReservationUpdateEndpoint = inject(TableReservationUpdateEndpoint);
  private tableLayoutGetNameByIdEndpoint = inject(TableLayoutGetNameByIdEndpoint)
  private diningTableGetTableStatusEndpoint = inject(DiningTablesGetStatusEndpoint);

  loading = false;
  filteredTables: UpdateDiningTableDto[] = [];
  timeSlots: string[] = [];

  form = this.fb.group({
    reservationDate: ['', Validators.required],          // Date picker
    reservationStartTime: ['', Validators.required],     // Time picker
    tableLayoutId: [null, Validators.required],
    diningTableId: [null, Validators.required],
    tableLayoutName: [''],
    diningTableNumber: [0],
    numberOfGuests: [1, Validators.required],
    notes: [''],
    firstName: ['', Validators.required],
    lastName: ['', Validators.required],
    email: ['', [Validators.required, Validators.email]],
    phoneNumber: [''],
    status: [0]
  });

  constructor(
    private dialogRef: MatDialogRef<TableReservationFormDialog>,
    @Inject(MAT_DIALOG_DATA) public data: any
  ) {}


  getMaxNumberOfGuests(): number | null {
    const tableId = this.form.controls.diningTableId.value;
    if (!tableId) return null;

    const table = this.filteredTables.find(t => t.id === tableId);
    return table ? table.numberOfSeats : null;
  }


  ngOnInit() {
    this.loading = true;


    if (this.data) {
      this.form.patchValue({
        reservationDate: this.data.reservationDate || '',
        reservationStartTime: this.data.reservationStartTime || '',
        tableLayoutId: this.data.tableLayoutId || null,
        diningTableId: this.data.diningTableId || null,
        numberOfGuests: this.data.numberOfGuests || 1,
        firstName: this.data.firstName || '',
        lastName: this.data.lastName || '',
        email: this.data.email || '',
        phoneNumber: this.data.phoneNumber || '',
        notes: this.data.notes || '',
        status: this.data.status || '',
      });


      console.log(this.data.diningTableId);


      if (this.data.mode === 'createForSelectedTable') {
        this.form.controls.tableLayoutId.disable();
        this.form.controls.diningTableId.disable();
      }

      if (this.data.mode === 'createForSelectedTable' && this.data.reservationDate) {
        this.generateAvailableTimeSlotsForSelectedTable();
      } else if (this.data.reservationDate) {
        this.timeSlots = this.generateTimeSlots('08:00', '21:30', 30);
      }


      if (this.data.tableLayoutId) {
        // delay refresh until form is fully ready
        setTimeout(() => {
          this.refreshAvailability();
        });
      }

    }

    this.form.controls.numberOfGuests.valueChanges.subscribe(() => {
      if (!this.loading) {
        this.refreshAvailability();
      }
    });



    this.loading = false;
  }


  private refreshAvailability() {
    if (this.loading || !this.data?.tables || !this.data?.layouts) return;

    const selectedLayoutId = this.form.controls.tableLayoutId.value;
    const selectedTableId = this.form.controls.diningTableId.value;
    const dateRange = this.getSelectedDateTimeRange();


      let validTables =  this.data.tables.filter(
        (t: UpdateDiningTableDto) => t.tableLayoutId === selectedLayoutId
      );

    // 3️⃣ Filter by time overlap
    const applyResult = (tables: UpdateDiningTableDto[]) => {
      this.filteredTables = this.sortTables(tables);

      // ✅ keep selection if still valid
      if (selectedTableId) {
        const stillValid = this.filteredTables.some(
          t => t.id === selectedTableId
        );

        if (!stillValid) {
          this.form.patchValue({
            tableLayoutId: null,
            diningTableId: null
          });
          this.filteredTables = [];
        }
      }
    };

    if (dateRange) {
      console.log('time',dateRange.start.getTime());
      this.tableReservationGetListEndpoint.handleAsync({
        requestedStart: new Date(dateRange.start.getTime()).toISOString(),
        requestedEnd: new Date(dateRange.end.getTime() + 0).toISOString()
      }).subscribe(reservations => {

        console.log(reservations);


        const bufferMs = 89 * 60 * 1000; // 90 min pre/post buffer

        const freeTables = validTables.filter((table: UpdateDiningTableDto) => {
          return !reservations.some(r => {
            if (r.id === this.data?.id) return false; // ignore current reservation
            if (r.diningTableId !== table.id) return false;

            const existingStart = new Date(r.reservationStart).getTime() - bufferMs; // 90 min before
            const existingEnd = new Date(r.reservationEnd ?? r.reservationStart).getTime() + bufferMs; // 90 min after

            const newStart = dateRange!.start.getTime();
            const newEnd = dateRange!.end.getTime();

            // Block if new reservation overlaps buffer zone
            return newStart < existingEnd && newEnd > existingStart;
          });
        });

        applyResult(freeTables);
      });
    } else {
      applyResult(validTables);
    }
  }






  private getSelectedDateTimeRange(): { start: Date; end: Date } | null {
    const date = this.form.controls.reservationDate.value;
    const time = this.form.controls.reservationStartTime.value;
    if (!date || !time) return null;

    const [hours, minutes] = time.split(':').map(Number);
    const startLocal = new Date(date);
    startLocal.setHours(hours, minutes, 0, 0);

    // Convert local time to UTC
    const startUtc = new Date(startLocal.getTime() - startLocal.getTimezoneOffset() * 60 * 1000);
    const endUtc = new Date(startUtc.getTime() + 90 * 60 * 1000);

    return { start: startUtc, end: endUtc };
  }

  private generateAvailableTimeSlotsForSelectedTable(): void {
    const date = this.form.controls.reservationDate.value;
    const tableId = this.form.controls.diningTableId.value;

    if (!date || !tableId) {
      this.timeSlots = [];
      return;
    }

    const allSlots = this.generateTimeSlots('08:00', '21:30', 30);

    const dayStart = new Date(date);
    dayStart.setHours(0, 0, 0, 0);

    const dayEnd = new Date(date);
    dayEnd.setHours(23, 59, 59, 999);

    this.tableReservationGetListEndpoint.handleAsync({
      requestedStart: dayStart.toISOString(),
      requestedEnd: dayEnd.toISOString()
    }).subscribe(reservations => {

      const reservationDurationMs = 90 * 60 * 1000;

      this.timeSlots = allSlots.filter(slot => {
        const [h, m] = slot.split(':').map(Number);

        const slotStart = new Date(date);
        slotStart.setHours(h, m, 0, 0);

        const slotEnd = new Date(slotStart.getTime() + reservationDurationMs);

        return !reservations.some(r => {
          if (r.diningTableId !== tableId) return false;
          if (this.data?.id && r.id === this.data.id) return false;

          const existingStart = new Date(r.reservationStart).getTime();
          const existingEnd = r.reservationEnd
            ? new Date(r.reservationEnd).getTime()
            : existingStart + reservationDurationMs;

          return slotStart.getTime() < existingEnd &&
            slotEnd.getTime() > existingStart;
        });
      });
    });
  }







  // Generate time slots
  private generateTimeSlots(start: string, end: string, intervalMinutes: number): string[] {
    const slots: string[] = [];
    let [h, m] = start.split(':').map(Number);
    const [endH, endM] = end.split(':').map(Number);

    while (h < endH || (h === endH && m <= endM)) {
      slots.push(`${h.toString().padStart(2,'0')}:${m.toString().padStart(2,'0')}`);
      m += intervalMinutes;
      if (m >= 60) { m -= 60; h++; }
    }

    return slots;
  }


  // When user selects a date
  onDateChange(date: Date) {
    this.form.patchValue({ reservationStartTime: null });

    if (!date) {
      this.timeSlots = [];
      return;
    }

    if (this.data.mode === 'createForSelectedTable') {
      this.generateAvailableTimeSlotsForSelectedTable();
    } else {
      this.timeSlots = this.generateTimeSlots('08:00', '21:30', 30);
      this.refreshAvailability();
    }
  }





  // When user selects a time slot
  selectTime(time: string) {
    this.form.controls.reservationStartTime.setValue(time);
    this.refreshAvailability();
  }




  private sortTables(tables: UpdateDiningTableDto[]): UpdateDiningTableDto[] {
    return [...tables].sort((a, b) => Number(a.number) - Number(b.number));
  }

  // When layout changes
  // When layout changes
  onLayoutChange(layoutId: number) {
    this.form.patchValue({ diningTableId: null });
    this.refreshAvailability();
  }




  save() {
    if (!this.form.valid) return;

    const dateRaw = this.form.get('reservationDate')!.value;
    const timeRaw = this.form.get('reservationStartTime')!.value;

    if (!dateRaw || !timeRaw) return;

    // --- Build dates ---
    const date = new Date(dateRaw);
    const [hours, minutes] = timeRaw.split(':').map(Number);

    const start = new Date(date);
    start.setHours(hours, minutes, 0, 0);

    // +1 hour offset (your logic)
    start.setTime(start.getTime() + 60 * 60 * 1000);

    const end = new Date(start.getTime() + 90 * 60 * 1000);

    // --- Base payload (shared) ---
    const baseReservation = {
      diningTableId: this.form.get('diningTableId')!.value!,
      tableLayoutId: this.form.get('tableLayoutId')!.value!,
      numberOfGuests: this.form.get('numberOfGuests')!.value!,
      reservationStart: start.toISOString(),
      reservationEnd: end.toISOString(),
      notes: this.form.get('notes')!.value || '',
      firstName: this.form.get('firstName')!.value!,
      lastName: this.form.get('lastName')!.value!,
      email: this.form.get('email')!.value || undefined,
      phoneNumber: this.form.get('phoneNumber')!.value!,
      applicationUserId: null
    };

    // =========================
    // ✏️ UPDATE
    // =========================
    if (this.data?.id) {

      const selectedTableId = this.form.get('diningTableId')!.value;

      const table = this.filteredTables.find(
        t => t.id === selectedTableId
      );


      if (!table) {
        console.error('Dining table not found');
        return;
      }

      this.tableLayoutGetNameByIdEndpoint
        .handleAsync(baseReservation.tableLayoutId)
        .subscribe({
          next: dto => {
            const updatePayload = {
              id: this.data.id,
              tableLayoutName: dto.name, // ✅ string
              diningTableNumber: table.number,
              status: this.data.status,
              ...baseReservation
            };


            console.log(updatePayload);

            this.tableReservationUpdateEndpoint
              .handleAsync(updatePayload)
              .subscribe({
                next: () => this.dialogRef.close(true),
                error: err => console.error('Update failed', err)
              });
          },
          error: err => console.error('Failed to load table layout name', err)
        });

      return;
    }

    // =========================
    // 🆕 CREATE
    // =========================
    this.tableReservationCreateEndpoint
      .handleAsync(baseReservation)
      .subscribe({
        next: () => this.dialogRef.close(true),
        error: err => console.error('Create failed', err)
      });
  }









  close() {
    this.dialogRef.close();
  }
}
