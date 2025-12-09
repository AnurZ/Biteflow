import {Component, ElementRef, inject, OnInit, ViewChild} from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { TableLayoutCreateDialog } from './table-layout-create-dialog/table-layout-create-dialog';
import { TableLayoutGetPreviewEndpoint } from '../../endpoints/table-layout-endpoints/table-layout-get-endpoint';
import { TableLayoutGetListEndpoint } from '../../endpoints/table-layout-endpoints/table-layout-get-list-endpoint';
import {GetTableLayoutDto, TableStatus, UpdateDiningTableDto} from './table-layout-model';
import { ConfirmDialogComponent } from '../admin/staff/confirm-dialog/confirm-dialog-component';
import { TableLayoutDeleteEndpoint } from '../../endpoints/table-layout-endpoints/table-layout-delete-endpoint';
import { DiningTableGetListEndpoint } from '../../endpoints/dining-table-endpoints/dining-table-get-list-endpoint';
import { DiningTableCreateEndpoint } from '../../endpoints/dining-table-endpoints/dining-table-create-endpoint';
import { DiningTableDeleteEndpoint } from '../../endpoints/dining-table-endpoints/dining-table-delete-endpoint';
import { DiningTableUpdateEndpoint } from '../../endpoints/dining-table-endpoints/dining-table-update-endpoint';
import {firstValueFrom} from 'rxjs';
import {forkJoin, tap} from 'rxjs';


@Component({
  selector: 'app-table-layout',
  standalone: false,
  templateUrl: './table-layout.html',
  styleUrls: ['./table-layout.css']
})
export class TableLayoutComponent implements OnInit {

  private dialog = inject(MatDialog);
  private tableLayoutGetListEndpoint = inject(TableLayoutGetListEndpoint);
  private tableLayoutGetPreviewEndpoint = inject(TableLayoutGetPreviewEndpoint);
  private deleteEp = inject(TableLayoutDeleteEndpoint);

  private diningTableGetListEndpoint = inject(DiningTableGetListEndpoint);
  private diningTableCreateEndpoint = inject(DiningTableCreateEndpoint);
  private diningTableDeleteEndpoint = inject(DiningTableDeleteEndpoint);
  private diningTableUpdateEndpoint = inject(DiningTableUpdateEndpoint);

  @ViewChild('textFilterInput') textFilterInput!: ElementRef<HTMLInputElement>;
  @ViewChild('statusSelectInput') statusSelectInput!: ElementRef<HTMLSelectElement>;
  @ViewChild('seatsInput') seatsInput!: ElementRef<HTMLInputElement>;

  layouts: GetTableLayoutDto[] = [];
  selectedLayout?: GetTableLayoutDto;
  otherLayouts: GetTableLayoutDto[] = [];

  diningTables: UpdateDiningTableDto[] = [];

  selectedTable?: UpdateDiningTableDto;

  statusColors: Record<TableStatus, string> = {
    [TableStatus.Free]: '#4CAF50',        // green
    [TableStatus.Occupied]: '#F44336',    // red
    [TableStatus.Reserved]: '#2196F3',    // orange
    [TableStatus.Cleaning]: '#eae230',    // blue
    [TableStatus.OutOfService]: '#9E9E9E' // gray
  };


  // -------------------------------------------------------
  // COLOR NORMALIZER - THE FIX
  // -------------------------------------------------------
  private normalizeColor(c: string | null | undefined): string {
    return /^#[0-9A-Fa-f]{6}$/.test(c || "") ? c! : "#000000";
  }

  extraSearchEnabled = false;

  applySelectedTableChanges() {
    if (!this.selectedTable) return;

    const table = this.selectedTable;

    // Update basic table properties
    table.sectionName   = (document.getElementById('inputSectionName') as HTMLInputElement).value;
    table.number        = parseInt((document.getElementById('inputNumber') as HTMLInputElement).value || "0");
    table.numberOfSeats = parseInt((document.getElementById('inputSeats') as HTMLInputElement).value || "0");
    table.x             = parseInt((document.getElementById('inputX') as HTMLInputElement).value || "0");
    table.y             = parseInt((document.getElementById('inputY') as HTMLInputElement).value || "0");
    table.tableSize     = parseInt((document.getElementById('inputTableSize') as HTMLInputElement).value || "100");

    // Update table status from dropdown
    const statusEl = document.getElementById('inputItem') as HTMLSelectElement | null;
    if (statusEl) {
      switch (statusEl.value) {
        case 'item1': table.status = TableStatus.Free; break;
        case 'item2': table.status = TableStatus.Occupied; break;
        case 'item3': table.status = TableStatus.Reserved; break;
        case 'item4': table.status = TableStatus.Cleaning; break;
        case 'item5': table.status = TableStatus.OutOfService; break;
        default:      table.status = TableStatus.Free; break;
      }
    }

    table.color = this.statusColors[table.status] || '#000000';

    this.clearInputsAndSelection();
  }





  async onAddTableClicked() {
    if (!this.isTableValid()) return;

    const ok = await this.validateTableBeforeAction();
    if (!ok) return;

    this.spawnTable();
    this.filteredTables = this.diningTables;
    this.applyFilters()
    this.sortTables()
  }

  async onCheckTableClicked() {
    if (!this.isTableEdited() || !this.isTableValid()) return;

    const ok = await this.validateTableBeforeAction();
    if (!ok) return;

    this.applySelectedTableChanges();
    this.filteredTables = this.diningTables;
    this.applyFilters()
  }

  deletedTables: UpdateDiningTableDto[] = [];


  deleteDiningTable() {
    const ref = this.dialog.open(ConfirmDialogComponent, {
      data: { title: 'Delete table?', message: 'This cannot be undone.' }
    });

    ref.afterClosed().subscribe(ok => {
      if (!ok || !this.selectedTable) return;

      const table = this.selectedTable;

      // 1. If it has an ID, mark it for deletion
      if (table.id && table.id > 0) {
        this.deletedTables.push(table);
      }

      // 2. Remove from UI
      this.diningTables = this.diningTables.filter(t => t !== table);
      this.filteredTables = this.filteredTables.filter(t => t !== table);
      this.selectedTable = undefined;
      this.clearInputsAndSelection();


    });
  }





  cancelTableEdit() {
    if (!this.selectedTable || !this.originalTableValues) return;

    // Restore table to last saved values
    Object.assign(this.selectedTable, this.originalTableValues);

    // Update form inputs and icon
    this.setTableEditValues(this.selectedTable);
  }



  newTableSize: number = 100;
  readonly LAYOUT_USABLE_WIDTH = 640;
  readonly LAYOUT_USABLE_HEIGHT = 582;

  getMaxX(tableSize: number): number {
    const wrapperWidth = 186 * tableSize / 220;
    return Math.round(this.LAYOUT_USABLE_WIDTH - wrapperWidth);
  }

  getMaxY(tableSize: number): number {
    const wrapperHeight = 148 * tableSize / 220;
    return Math.round(this.LAYOUT_USABLE_HEIGHT - wrapperHeight) - 2;
  }


  async saveAllTables() {
    if (!this.selectedLayout) return;

    try {
      // 1) Delete all tables marked for deletion
      await Promise.all(this.deletedTables.map(t =>
        firstValueFrom(this.diningTableDeleteEndpoint.handleAsync(t.id!))
      ));

      // 2) Create or update remaining tables
      const requests = this.diningTables.map(t => {
        if (t.id && t.id > 0) {
          return this.diningTableUpdateEndpoint.handleAsync(t);
        } else {
          const createDto = { ...t, tableLayoutId: this.selectedLayout!.id };

          return this.diningTableCreateEndpoint.handleAsync(createDto).pipe(
            tap(created => {
              const match = this.diningTables.find(x => x === t);
              if (match) match.id = created.id;
            })
          );
        }
      });

      if (requests.length > 0) {
        await firstValueFrom(forkJoin(requests));
      }

      // 3) Clear deleted tables array
      this.deletedTables = [];

      // 4) Reload tables
      this.loadTables(this.selectedLayout.id);

      this.clearAllFilters();

      console.log('All changes saved successfully!');
    } catch (err) {
      console.error('Error saving tables:', err);
    }
  }



  openLayoutBackgroundDialog() {
    if (!this.selectedLayout) return;

    const ref = this.dialog.open(TableLayoutCreateDialog, {
      width: '600px',
      height: '590px',
      data: {
        layout: this.selectedLayout
      }
    });

    ref.afterClosed().subscribe(updated => {
      if (!updated) return;

      // update the selectedLayout directly so background updates
      this.selectedLayout = { ...this.selectedLayout, ...updated };

      // also update the layout in the list so UI lists are consistent
      const index = this.layouts.findIndex(l => l.id === updated.id);
      if (index !== -1) {
        // replace the object reference to trigger change detection
        this.layouts[index] = { ...this.layouts[index], ...updated };
      }
    });
  }

  onItemChanged(event: Event) {
    const value = (event.target as HTMLSelectElement).value;
    const icon = document.getElementById('tableIcon') as HTMLElement;
    if (!icon) return;

    let status: TableStatus = TableStatus.Free;
    switch (value) {
      case 'item1': status = TableStatus.Free; break;
      case 'item2': status = TableStatus.Occupied; break;
      case 'item3': status = TableStatus.Reserved; break;
      case 'item4': status = TableStatus.Cleaning; break;
      case 'item5': status = TableStatus.OutOfService; break;
    }

    icon.style.color = this.statusColors[status];
  }










  setTableEditValues(table: UpdateDiningTableDto) {
    setTimeout(() => {
      (document.getElementById('inputSectionName') as HTMLInputElement).value = table.sectionName || "";
      (document.getElementById('inputNumber') as HTMLInputElement).value = String(table.number);
      (document.getElementById('inputSeats') as HTMLInputElement).value = String(table.numberOfSeats);
      (document.getElementById('inputX') as HTMLInputElement).value = String(table.x);
      (document.getElementById('inputY') as HTMLInputElement).value = String(table.y);
      (document.getElementById('inputTableSize') as HTMLInputElement).value = String(table.tableSize);

      // Set table status
      const statusEl = document.getElementById('inputItem') as HTMLSelectElement | null;
      if (statusEl) {
        switch (table.status) {
          case TableStatus.Free:         statusEl.value = 'item1'; break;
          case TableStatus.Occupied:     statusEl.value = 'item2'; break;
          case TableStatus.Reserved:     statusEl.value = 'item3'; break;
          case TableStatus.Cleaning:     statusEl.value = 'item4'; break;
          case TableStatus.OutOfService: statusEl.value = 'item5'; break;
          default:                       statusEl.value = ''; break;
        }
      }

      const icon = document.getElementById('tableIcon') as HTMLElement | null;
      if (icon) {
        icon.style.color = this.statusColors[table.status] || '#4CAF50';
      }

    });
    this.filteredTables = this.diningTables;
    this.applyFilters()
  }




  originalTableValues?: UpdateDiningTableDto;


  isTableEdited(): boolean {
    if (!this.selectedTable) return false;

    const table = this.selectedTable;

    const sectionName = (document.getElementById('inputSectionName') as HTMLInputElement)?.value || '';
    const number = parseInt((document.getElementById('inputNumber') as HTMLInputElement)?.value || '0');
    const seats = parseInt((document.getElementById('inputSeats') as HTMLInputElement)?.value || '0');
    const x = parseInt((document.getElementById('inputX') as HTMLInputElement)?.value || '0');
    const y = parseInt((document.getElementById('inputY') as HTMLInputElement)?.value || '0');
    const tableSize = parseInt((document.getElementById('inputTableSize') as HTMLInputElement)?.value || '0');
    const color = (document.getElementById('inputColor') as HTMLInputElement)?.value || '';

    return (
      sectionName !== table.sectionName ||
      number !== table.number ||
      seats !== table.numberOfSeats ||
      x !== table.x ||
      y !== table.y ||
      tableSize !== table.tableSize ||
      color !== table.color
    );
  }





  selectThenDrag(event: PointerEvent, table: UpdateDiningTableDto) {
    if (!this.selectedLayout) return;

    this.selectedTable = table;

    // Keep a copy for rollback
    this.originalTableValues = JSON.parse(JSON.stringify(table));

    this.setTableEditValues(table);

    const tableEl = event.currentTarget as HTMLElement;
    tableEl.setPointerCapture(event.pointerId);
    const container = document.querySelector('.table-layout-full') as HTMLElement;
    const containerRect = container.getBoundingClientRect();
    const tableRect = tableEl.getBoundingClientRect();

    const startX = event.clientX;
    const startY = event.clientY;
    const offsetX = startX - tableRect.left;
    const offsetY = startY - tableRect.top;

    const originalX = table.x;
    const originalY = table.y;

    let isDragging = false;
    const DRAG_THRESHOLD = 10;

    const moveHandler = (e: PointerEvent) => {
      const deltaX = e.clientX - startX;
      const deltaY = e.clientY - startY;

      if (!isDragging && Math.abs(deltaX) + Math.abs(deltaY) > DRAG_THRESHOLD) {
        isDragging = true;
        tableEl.classList.add('dragging');
        tableEl.setPointerCapture(e.pointerId);
      }

      if (isDragging) {
        let newX = e.clientX - containerRect.left - offsetX;
        let newY = e.clientY - containerRect.top - offsetY;

        newX = Math.max(0, Math.min(newX, container.clientWidth - tableRect.width));
        newY = Math.max(0, Math.min(newY, container.clientHeight - tableRect.height));

        table.x = Math.round(newX);
        table.y = Math.round(newY);
      }
    };

    const upHandler = (e: PointerEvent) => {
      if (isDragging) {
        tableEl.classList.remove('dragging');

        const overlapping = this.diningTables.some(t => {
          if (t === table) return false;
          const tableWidth = table.tableSize * 186 / 220 + 2;
          const tableHeight = table.tableSize * 148 / 220;
          const otherWidth = t.tableSize * 186 / 220 + 2;
          const otherHeight = t.tableSize * 148 / 220;
          return !(table.x + tableWidth <= t.x || table.x >= t.x + otherWidth || table.y + tableHeight <= t.y || table.y >= t.y + otherHeight);
        });

        if (overlapping) {
          table.x = originalX;
          table.y = originalY;
        }

        this.setTableEditValues(table);
      }

      tableEl.removeEventListener('pointermove', moveHandler);
      tableEl.removeEventListener('pointerup', upHandler);
    };

    tableEl.addEventListener('pointermove', moveHandler);
    tableEl.addEventListener('pointerup', upHandler);
  }

  TableStatusEnum = TableStatus;

  clearInputsAndSelection() {
    this.selectedTable = undefined;

    const inputsToClear = [
      'inputSectionName',
      'inputNumber',
      'inputSeats',
      'inputX',
      'inputY',
      'inputTableSize',
      'inputItem'
    ];

    inputsToClear.forEach(id => {
      const el = document.getElementById(id) as HTMLInputElement | HTMLSelectElement | null;
      if (el) {
        if (el.tagName === 'INPUT') {
          (el as HTMLInputElement).value = '';
        } else if (el.tagName === 'SELECT') {
          (el as HTMLSelectElement).selectedIndex = 0;
        }
      }
    });

    const colorInput = document.getElementById('inputColor') as HTMLInputElement | null;
    if (colorInput) colorInput.value = '#000000';

    const tableIcon = document.getElementById('tableIcon') as HTMLElement | null;
    if (tableIcon) tableIcon.style.color = '#4CAF50';
  }


  deselectTable(event: MouseEvent) {
    if ((event.target as HTMLElement).classList.contains('table-layout-full')) {
      this.clearInputsAndSelection();
    }
  }




  isTableValid(): boolean {
    const sectionName = (document.getElementById('inputSectionName') as HTMLInputElement)?.value.trim();
    const numberStr = (document.getElementById('inputNumber') as HTMLInputElement)?.value.trim();

    // Only ensure required fields are filled
    return !!sectionName && !!numberStr;
  }

  duplicateApiCheck:boolean = false;

  async validateTableBeforeAction(): Promise<boolean> {
    this.duplicateApiCheck = false;

    const sectionName = (document.getElementById('inputSectionName') as HTMLInputElement)?.value.trim();
    const number = parseInt((document.getElementById('inputNumber') as HTMLInputElement)?.value.trim() || "0");

    // 1. Load all tables from API - WAIT here
    const tables = await firstValueFrom(
      this.diningTableGetListEndpoint.handleAsync()
    );

    // 2. Check duplicates in API
    const duplicateApi = tables.some(t =>
      !(this.selectedTable && t.id === this.selectedTable.id) &&
      t.sectionName === sectionName &&
      t.number === number && t.tableLayoutId !== this.selectedLayout?.id
    );

    const duplicateLocal = this.diningTables.some(t =>
      !(this.selectedTable && t.id === this.selectedTable.id) &&
      t.sectionName === sectionName &&
      t.number === number
    );

    if (duplicateApi || duplicateLocal) {
      alert(`A table with Section "${sectionName}" and Number "${number}" already exists!`);
      return false;
    }

    return true;
  }



  ngOnInit() {
    this.loadLayouts();
  }



  loadLayouts() {
    this.tableLayoutGetPreviewEndpoint.handleAsync().subscribe((res) => {
      this.layouts = res;
      this.otherLayouts = res;
    });
  }

  isLoadingTables = false;


  sortTables() {
    this.filteredTables = this.filteredTables
      .slice() // avoid mutating original array
      .sort((a, b) => {
        // First sort by sectionName alphabetically
        const sectionCompare = a.sectionName.localeCompare(b.sectionName);
        if (sectionCompare !== 0) return sectionCompare;

        // Then by number ascending
        return a.number - b.number;
      });
  }


  loadTables(layoutId: number) {
    this.isLoadingTables = true;

    this.diningTableGetListEndpoint.handleAsync({ tableLayoutId: layoutId })
      .subscribe({
        next: (tables) => {
          const normalized = (tables || []).map(t => ({
            ...t,
            color: this.normalizeColor(t.color)
          }));

          this.diningTables = normalized;
          this.filteredTables = this.diningTables;
          this.savedTables = JSON.parse(JSON.stringify(normalized));
          this.sortTables();
          this.isLoadingTables = false;

        },
        error: (err) => {
          console.error("Error loading tables:", err);
          this.isLoadingTables = false;
        }
      });
  }





  savedTables: UpdateDiningTableDto[] = [];

  animateSelect(layout: GetTableLayoutDto, event: Event) {
    this.selectedLayout = layout;
    this.otherLayouts = this.layouts.filter(l => l.id !== layout.id);

    // Clear the UI and tables IMMEDIATELY
    this.diningTables = [];
    this.savedTables = [];
    this.clearInputsAndSelection();
    this.clearAllFilters();


    // Now load new tables
    this.loadTables(layout.id);
  }




  resetLayout() {
    this.selectedLayout = undefined;
    this.otherLayouts = this.layouts;
    this.cancelTableEdit()
    this.diningTables = [];
  }



  spawnTable() {
    if (!this.selectedLayout) return;

    const x = parseInt((document.getElementById('inputX') as HTMLInputElement).value || '0');
    const y = parseInt((document.getElementById('inputY') as HTMLInputElement).value || '0');
    const tableSize = parseInt((document.getElementById('inputTableSize') as HTMLInputElement).value || '120');
    const number = parseInt((document.getElementById('inputNumber') as HTMLInputElement).value || '1');
    const numberOfSeats = parseInt((document.getElementById('inputSeats') as HTMLInputElement).value || '1');
    const sectionName = (document.getElementById('inputSectionName') as HTMLInputElement).value;

    // Get status from dropdown
    const statusSelect = document.getElementById('inputItem') as HTMLSelectElement | null;
    let status: TableStatus = TableStatus.Free; // default
    if (statusSelect) {
      switch (statusSelect.value) {
        case 'item1': status = TableStatus.Free; break;
        case 'item2': status = TableStatus.Occupied; break;
        case 'item3': status = TableStatus.Reserved; break;
        case 'item4': status = TableStatus.Cleaning; break;
        case 'item5': status = TableStatus.OutOfService; break;
        default: status = TableStatus.Free;
      }
    }

    // Determine color based on status
    const color = this.statusColors[status] || '#000000';

    const tableWidth = tableSize * 186 / 220 + 2;
    const tableHeight = tableSize * 148 / 220 + 4;

    const overlapping = this.diningTables.some(t => {
      const otherWidth = t.tableSize * 186 / 220 + 2;
      const otherHeight = t.tableSize * 148 / 220 + 2;
      return !(
        x + tableWidth <= t.x ||
        x >= t.x + otherWidth ||
        y + tableHeight <= t.y ||
        y >= t.y + otherHeight
      );
    });

    if (overlapping) {
      alert('Cannot spawn table here! Another table occupies this position.');
      return;
    }

    const dto: UpdateDiningTableDto = {
      id: 0,
      tableLayoutId: this.selectedLayout.id,
      x,
      y,
      tableSize,
      color,
      shape: 'square',
      tableType: 1,
      status,
      number,
      numberOfSeats,
      sectionName,
      isActive: true,
      lastUsedAt: new Date()
    };

    this.diningTables.push(dto);
  }





  createTableLayout() {
    const dialogRef = this.dialog.open(TableLayoutCreateDialog, {
      width: '600px',
      height: '590px'
    });
    dialogRef.afterClosed().subscribe(() => this.loadLayouts());
  }

  deleteTableLayout(id: number) {
    const ref = this.dialog.open(ConfirmDialogComponent, {
      data: { title: 'Delete table layout?', message: 'This cannot be undone.' }
    });
    ref.afterClosed().subscribe(ok => {
      if (ok) this.deleteEp.handleAsync(id).subscribe(() => this.loadLayouts());
    });
  }

  clearAllTables() {
    const ref = this.dialog.open(ConfirmDialogComponent, {
      data: { title: 'Clear all tables?', message: 'This cannot be undone.' }
    });

    ref.afterClosed().subscribe(ok => {
      if (!ok) return;

      // 1. Mark all tables with an ID for deletion
      this.deletedTables.push(...this.diningTables.filter(t => t.id && t.id > 0));

      // 2. Clear UI
      this.diningTables = [];
      this.filteredTables = [];
      this.selectedTable = undefined;
      this.clearInputsAndSelection();

    });
  }

  textFilter: string = '';
  statusFilter: string | null = null;
  seatsFilter: number | null = null;
  onTextFilterChange(event: Event) {
    this.textFilter = (event.target as HTMLInputElement).value;
    this.applyFilters();
  }

  onStatusChange(event: Event) {
    const value = (event.target as HTMLSelectElement).value;
    this.statusFilter = value || null;
    this.applyFilters();
  }

  onSeatsChange(event: Event) {
    const value = (event.target as HTMLInputElement).value;
    this.seatsFilter = value ? Number(value) : null;
    this.applyFilters();
  }

  filteredTables:UpdateDiningTableDto[] = [];

  applyFilters() {
    let filtered = [...this.diningTables];

    // TEXT filter (sectionName + number)
    if (this.textFilter) {
      const search = this.textFilter.toLowerCase();

      filtered = filtered.filter(t => {
        const sectionMatch = t.sectionName.toLowerCase().includes(search);
        const numberMatch = t.number.toString().includes(search);
        return sectionMatch || numberMatch;
      });
    }

    // STATUS filter
    if (this.statusFilter && this.statusFilter !== "All") {
      filtered = filtered.filter(t => TableStatus[t.status] === this.statusFilter);
    }


    // SEATS filter
    if (this.seatsFilter) {
      filtered = filtered.filter(t => t.numberOfSeats >= this.seatsFilter!);
    }


    this.filteredTables = filtered;
    this.sortTables()
  }

  clearAllFilters() {
    // Reset values
    this.textFilterInput.nativeElement.value = '';
    this.seatsInput.nativeElement.value = '';
    this.statusSelectInput.nativeElement.value = 'All';

    // Create new 'input' and 'change' events
    const inputEvent = new Event('input');
    const changeEvent = new Event('change');

    // Dispatch events so your handlers receive real Event objects
    this.textFilterInput.nativeElement.dispatchEvent(inputEvent);
    this.seatsInput.nativeElement.dispatchEvent(inputEvent);
    this.statusSelectInput.nativeElement.dispatchEvent(changeEvent);
  }


  openFilter(){
    this.extraSearchEnabled = !this.extraSearchEnabled;
  }
}
