import { Component, OnInit, ViewChild, inject } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { MatPaginator, PageEvent } from '@angular/material/paginator';
import { DatePipe } from '@angular/common';

import { StaffListEndpoint, StaffListRequest } from '../../../../endpoints/staff-crud-endpoints/staff-list-endpoint';
import { StaffDeleteEndpoint } from '../../../../endpoints/staff-crud-endpoints/staff-delete-endpoint';
import { StaffListItem } from '../models';
import { StaffFormDialogComponent } from '../staff-form-dialog/staff-form-dialog';
import { ConfirmDialogComponent } from '../confirm-dialog/confirm-dialog-component';

@Component({
  selector: 'app-staff-list',
  templateUrl: './staff-list.html',
  standalone: false,
  styleUrls: ['./staff-list.css']
})
export class StaffList implements OnInit {
  private listEp = inject(StaffListEndpoint);
  private deleteEp = inject(StaffDeleteEndpoint);
  private dialog = inject(MatDialog);

  columns = ['displayName', 'email', 'position', 'firstName', 'lastName', 'hireDate', 'isActive', 'actions'];
  rows: StaffListItem[] = [];
  total = 0;

  pageNumber = 1;
  pageSize = 10;
  search = '';
  sort: string | undefined;

  @ViewChild(MatPaginator) paginator!: MatPaginator;

  ngOnInit(): void {
    this.load();
  }

  load() {
    const req: StaffListRequest = {
      pageNumber: this.pageNumber,
      pageSize: this.pageSize,
      search: this.search || undefined,
      sort: this.sort
    };
    this.listEp.handleAsync(req).subscribe(res => {
      this.rows = res.items;
      this.total = res.total;
    });
  }

  onPage(e: PageEvent) {
    this.pageNumber = e.pageIndex + 1;
    this.pageSize = e.pageSize;
    this.load();
  }

  onSearch(term: string) {
    this.search = term;
    this.pageNumber = 1;
    this.load();
  }

  onSort(key: string) {
    if (!this.sort || !this.sort.includes(key)) this.sort = key;
    else if (this.sort === key) this.sort = `-${key}`;
    else this.sort = undefined;
    this.load();
  }

  create() {
    const ref = this.dialog.open(StaffFormDialogComponent, { width: '720px', data: { mode: 'create' } });
    ref.afterClosed().subscribe(changed => changed && this.load());
  }

  edit(id: number) {
    const ref = this.dialog.open(StaffFormDialogComponent, { width: '720px', data: { mode: 'edit', id } });
    ref.afterClosed().subscribe(changed => changed && this.load());
  }

  delete(id: number) {
    const ref = this.dialog.open(ConfirmDialogComponent, {
      data: { title: 'Delete staff member?', message: 'This cannot be undone.' }
    });
    ref.afterClosed().subscribe(ok => {
      if (ok) this.deleteEp.handleAsync(id).subscribe(() => this.load());
    });
  }
}
