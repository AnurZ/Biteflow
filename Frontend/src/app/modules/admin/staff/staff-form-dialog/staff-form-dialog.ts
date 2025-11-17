import { Component, Inject, OnInit, inject } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { StaffCreateEndpoint } from '../../../../endpoints/staff-crud-endpoints/staff-create-endpoint';
import { StaffUpdateEndpoint } from '../../../../endpoints/staff-crud-endpoints/staff-update-endpoint';
import { StaffGetByIdEndpoint } from '../../../../endpoints/staff-crud-endpoints/staff-get-by-id--endpoint';
import { CreateStaffRequest, StaffDetails, UpdateStaffRequest } from '../models';

type DialogData = { mode: 'create' } | { mode: 'edit'; id: number };



@Component({
  selector: 'app-staff-form-dialog',
  templateUrl: './staff-form-dialog.html',
  standalone: false,
  styleUrls: ['./staff-form-dialog.css']
})
export class StaffFormDialogComponent implements OnInit {
  private fb = inject(FormBuilder);
  private createEp = inject(StaffCreateEndpoint);
  private updateEp = inject(StaffUpdateEndpoint);
  private getByIdEp = inject(StaffGetByIdEndpoint);


  generateRandomPassword():string {
    const chars = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789';
    let password = '';
    for (let i = 0; i < 10; i++) {
      password += chars.charAt(Math.floor(Math.random() * chars.length));
    }
    return password;
  }

  constructor(
    private ref: MatDialogRef<StaffFormDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: DialogData
  ) {}

  get title() {
    return this.data.mode === 'create' ? 'Add Staff Member' : 'Edit Staff Member';
  }

  loading = false;
  showPwd = true;

  form = this.fb.group({
    email: this.fb.control<string>('', { nonNullable: true, validators: [Validators.required, Validators.email] }),
    displayName: this.fb.control<string>('', { nonNullable: true, validators: [Validators.required] }),
    plainPassword: this.fb.control<string>(this.generateRandomPassword(), { nonNullable: true, validators: [Validators.required, Validators.minLength(8)] }),

    id: this.fb.control<number | null>(null),
    position: this.fb.control<string>('', { validators: [Validators.required], nonNullable: true }),
    firstName: this.fb.control<string>('', { validators: [Validators.required], nonNullable: true }),
    lastName: this.fb.control<string>('', { validators: [Validators.required], nonNullable: true }),
    phoneNumber: this.fb.control<string | null>(null),
    hireDate: this.fb.control<Date | null>(null),
    terminationDate: this.fb.control<Date | null>(null),
    salary: this.fb.control<number | null>(null),
    hourlyRate: this.fb.control<number | null>(null),
    employmentType: this.fb.control<string | null>(null),
    shiftType: this.fb.control<string | null>(null),
    shiftStart: this.fb.control<string | null>(null), // "HH:mm:ss"
    shiftEnd: this.fb.control<string | null>(null),
    isActive: this.fb.control<boolean>(true, { nonNullable: true }),
    notes: this.fb.control<string | null>(null)
  });

  ngOnInit(): void {
    if (this.data.mode === 'edit') {
      this.form.controls.email.disable();
      this.form.controls.plainPassword.disable();

      this.loading = true;
      this.getByIdEp.handleAsync(this.data.id).subscribe({
        next: (dto: StaffDetails) => {
          this.form.patchValue({
            id: dto.id,
            email: dto.email ?? '',
            displayName: dto.displayName ?? '',
            position: dto.position,
            firstName: dto.firstName,
            lastName: dto.lastName,
            phoneNumber: dto.phoneNumber ?? null,
            hireDate: dto.hireDate ? new Date(dto.hireDate) : null,
            terminationDate: dto.terminationDate ? new Date(dto.terminationDate) : null,
            salary: dto.salary ?? null,
            hourlyRate: dto.hourlyRate ?? null,
            employmentType: dto.employmentType ?? null,
            shiftType: dto.shiftType ?? null,
            shiftStart: dto.shiftStart ?? null,
            shiftEnd: dto.shiftEnd ?? null,
            isActive: dto.isActive,
            notes: dto.notes ?? null
          });
          this.loading = false;
        },
        error: () => (this.loading = false)
      });
    }
  }

  private toIso(d: Date | string | null | undefined): string | undefined {
    if (!d) return undefined;
    return d instanceof Date ? d.toISOString() : d;
  }

  togglePwd() {
    this.showPwd = !this.showPwd;
  }

  save() {
    if (this.form.invalid) return;
    this.loading = true;

    const raw = this.form.getRawValue();
    if (this.data.mode === 'create') {
      const normalizedPosition = raw.position?.toLowerCase() ?? '';
      const derivedRole = normalizedPosition === 'manager' ? 'admin' : 'staff';

      const body: CreateStaffRequest = {
        appUserId: 0,
        email: raw.email.trim(),
        displayName: raw.displayName.trim(),
        plainPassword: raw.plainPassword.trim(),
        role: derivedRole,

        position: raw.position.trim(),
        firstName: raw.firstName.trim(),
        lastName: raw.lastName.trim(),
        phoneNumber: raw.phoneNumber ?? undefined,
        hireDate: this.toIso(raw.hireDate),
        hourlyRate: raw.hourlyRate ?? undefined,
        employmentType: raw.employmentType ?? undefined,
        shiftType: raw.shiftType ?? undefined,
        shiftStart: raw.shiftStart ?? undefined,
        shiftEnd: raw.shiftEnd ?? undefined,
        isActive: raw.isActive ?? true,
        notes: raw.notes ?? undefined
      };

      this.createEp.handleAsync(body).subscribe({
        next: () => this.ref.close(true),
        error: () => (this.loading = false)
      });
    } else {
      const body: UpdateStaffRequest = {
        id: raw.id!,
        displayName: raw.displayName,
        position: raw.position.trim(),
        firstName: raw.firstName.trim(),
        lastName: raw.lastName.trim(),
        phoneNumber: raw.phoneNumber ?? undefined,
        hireDate: this.toIso(raw.hireDate),
        terminationDate: this.toIso(raw.terminationDate),
        salary: raw.salary ?? undefined,
        hourlyRate: raw.hourlyRate ?? undefined,
        employmentType: raw.employmentType ?? undefined,
        shiftType: raw.shiftType ?? undefined,
        shiftStart: raw.shiftStart ?? undefined,
        shiftEnd: raw.shiftEnd ?? undefined,
        isActive: raw.isActive ?? true,
        notes: raw.notes ?? undefined
      };

      this.updateEp.handleAsync(body).subscribe({
        next: () => this.ref.close(true),
        error: () => (this.loading = false)
      });
    }
  }

  cancel() {
    this.ref.close(false);
  }

  genPassword():string {
    return "rendomPASS";
  }
}
