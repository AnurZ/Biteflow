import { Component, OnInit, inject, ViewChild } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { HttpClient } from '@angular/common/http';
import { CommonModule } from '@angular/common';
import { MatStepper, MatStepperModule } from '@angular/material/stepper';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MyConfig } from '../../../my-config';

@Component({
  selector: 'app-tenant-activation',
  templateUrl: './tenant-activation.html',
  styleUrls: ['./tenant-activation.css'],
  imports: [
    CommonModule, ReactiveFormsModule,
    MatStepperModule, MatFormFieldModule, MatInputModule,
    MatButtonModule, MatIconModule, MatSnackBarModule
  ]
})
export class TenantActivationComponent implements OnInit {
  private fb = inject(FormBuilder);
  private snack = inject(MatSnackBar);
  private http = inject(HttpClient);

  @ViewChild(MatStepper) stepper!: MatStepper;

  status = 0;
  isBusy = false;

  private base = `${MyConfig.api_address}/activation-requests`;

  restaurantForm = this.fb.group({
    restaurantName: ['', [Validators.required, Validators.minLength(2)]],
    domain: ['', [Validators.required, Validators.pattern(/^(?:[a-z0-9](?:[a-z0-9-]{0,61}[a-z0-9])?\.)+[a-z]{2,63}$/i)]],
    address: ['', Validators.required],
    city: ['', Validators.required],
    state: ['', Validators.required],
  });

  ownerForm = this.fb.group({
    ownerFullName: ['', Validators.required],
    ownerEmail: ['', [Validators.required, Validators.email]],
    ownerPhone: ['', Validators.required],
  });

  ngOnInit(): void {}

  private buildBody() {
    const r = this.restaurantForm.value;
    const o = this.ownerForm.value;
    return {
      restaurantName: (r.restaurantName ?? '').trim(),
      domain: (r.domain ?? '').trim(),
      address: (r.address ?? '').trim(),
      city: (r.city ?? '').trim(),
      state: (r.state ?? '').trim(),
      ownerFullName: (o.ownerFullName ?? '').trim(),
      ownerEmail: (o.ownerEmail ?? '').trim(),
      ownerPhone: (o.ownerPhone ?? '').trim()
    };
  }

  saveStep1() {
    if (this.restaurantForm.invalid) return;
    this.stepper.next();
  }

  saveStep2() {
    if (this.ownerForm.invalid || this.restaurantForm.invalid) return;
    this.stepper.next();
  }

  submit() {
    if (this.ownerForm.invalid || this.restaurantForm.invalid) return;

    this.isBusy = true;

    this.http.post<void>(this.base, this.buildBody()).subscribe({
      next: () => {
        this.status = 1;
        this.isBusy = false;
        this.snack.open('Submitted', 'Close', { duration: 1600 });
      },
      error: (err: any) => {
        this.isBusy = false;
        const msg = err?.error?.message || 'Submit failed';
        this.snack.open(msg, 'Close', { duration: 2200 });
      }
    });
  }
}
