import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';
import { AdminRoutingModule } from './admin-routing-module';
import {
  MatTable,
  MatHeaderRow, MatRow,
  MatColumnDef, MatHeaderCell, MatCell,
  MatHeaderRowDef, MatRowDef,
  MatHeaderCellDef, MatCellDef,
} from '@angular/material/table';
import { MatPaginator } from '@angular/material/paginator';
import {MatDialogActions, MatDialogContent, MatDialogModule, MatDialogTitle} from '@angular/material/dialog';
import {MatError, MatFormField, MatLabel} from '@angular/material/form-field';
import { MatInput } from '@angular/material/input';
import {MatButton, MatButtonModule, MatIconButton} from '@angular/material/button';
import { MatIcon } from '@angular/material/icon';
import { MatSelect, MatOption } from '@angular/material/select';
import { MatCheckbox } from '@angular/material/checkbox';
import { MatTooltip } from '@angular/material/tooltip';
import {MatDatepicker, MatDatepickerInput, MatDatepickerToggle} from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';

import { StaffList } from './staff/staff-list/staff-list';
import { StaffFormDialogComponent } from './staff/staff-form-dialog/staff-form-dialog';
import {MatDivider} from '@angular/material/divider';
import {AdminLayout} from './admin-layout';

@NgModule({
  declarations: [
    StaffList,
    StaffFormDialogComponent,
  ],
  imports: [
    CommonModule,
    ReactiveFormsModule,
    AdminRoutingModule,
    MatTable,
    MatHeaderRow, MatRow,
    MatColumnDef, MatHeaderCell, MatCell,
    MatHeaderRowDef, MatRowDef,
    MatHeaderCellDef, MatCellDef,
    MatPaginator,
    MatDialogModule,
    MatFormField, MatLabel,
    MatInput,
    MatDialogTitle,
    MatButton, MatIconButton,
    MatIcon, MatDialogActions, MatDialogContent,
    MatSelect, MatOption,
    MatCheckbox,
    MatTooltip,
    MatDatepicker, MatDatepickerToggle,
    MatNativeDateModule, MatDatepickerInput, MatError, MatDivider,
  ]
})
export class AdminModule {}
