import { Component, OnInit, inject, ViewChild } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { ActivatedRoute, Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { CommonModule } from '@angular/common';
import { MatStepper, MatStepperModule } from '@angular/material/stepper';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MyConfig } from '../../../my-config';
import { map } from 'rxjs/operators';

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
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private http = inject(HttpClient);

  @ViewChild(MatStepper) stepper!: MatStepper;

  draftId: number | null = null;
  status = 0; // Draft
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

  ngOnInit(): void {
    const id = Number(this.route.snapshot.queryParamMap.get('id'));
    if (id && !Number.isNaN(id)) {
      this.draftId = id;
      this.load(id);
    }
  }

  private load(id: number) {
    this.isBusy = true;
    this.http.get<any>(`${this.base}/${id}`).subscribe({
      next: dto => {
        this.restaurantForm.patchValue({
          restaurantName: dto.restaurantName,
          domain: dto.domain,
          address: dto.address,
          city: dto.city,
          state: dto.state
        });
        this.ownerForm.patchValue({
          ownerFullName: dto.ownerFullName,
          ownerEmail: dto.ownerEmail,
          ownerPhone: dto.ownerPhone
        });
        this.status = dto.status ?? 0;
        this.isBusy = false;
      },
      error: (_err: any) => {
        this.isBusy = false;
        this.snack.open('Draft not found', 'Close', { duration: 2200 });
      }
    });
  }

  private buildBody() {
    const r = this.restaurantForm.value;
    const o = this.ownerForm.value;
    return {
      restaurantName: (r.restaurantName ?? '').trim(),
      domain:         (r.domain ?? '').trim(),
      address:        (r.address ?? '').trim(),
      city:           (r.city ?? '').trim(),
      state:          (r.state ?? '').trim(),
      ownerFullName:  (o.ownerFullName ?? '').trim(),
      ownerEmail:     (o.ownerEmail ?? '').trim(),
      ownerPhone:     (o.ownerPhone ?? '').trim()
    };
  }

  private createOrUpdateDraft() {
    const body = this.buildBody();
    if (!this.draftId) {
      return this.http.post<number>(this.base, body); // Create vraća id
    } else {
      return this.http
        .put<void>(`${this.base}/${this.draftId}`, { id: this.draftId, ...body })
        .pipe(map(() => this.draftId!));
    }
  }

  saveStep1() {
    if (this.restaurantForm.invalid) return;
    this.stepper.next();
  }

  saveStep2() {
    if (this.ownerForm.invalid || this.restaurantForm.invalid) return;
    this.isBusy = true;

    this.createOrUpdateDraft().subscribe({
      next: (id: number) => {
        this.draftId = id;
        this.isBusy = false;
        this.stepper.next();
        this.snack.open('Saved', 'Close', { duration: 1200 });
      },
      error: (err: any) => {
        this.isBusy = false;
        const msg = err?.error?.message || 'Save failed';
        this.snack.open(msg, 'Close', { duration: 2200 });
      }

    });
  }

  submit() {
    const doSubmit = (id: number) => {
      this.http.post<void>(`${this.base}/${id}/submit`, {}).subscribe({
        next: () => {
          this.status = 1;
          this.isBusy = false;
          this.snack.open('Submitted', 'Close', { duration: 1600 });
        },
        error: (err: any) => {
          this.isBusy = false;
          const msg = err?.error?.message || 'Save failed';
          this.snack.open(msg, 'Close', { duration: 2200 });
        }

      });
    };

    this.isBusy = true;

    this.createOrUpdateDraft().subscribe({
      next: (id: number) => doSubmit(id),
      error: (err: any) => {
        this.isBusy = false;
        this.snack.open('Create/Save failed', 'Close', { duration: 2200 });
      }
    });
  }
}
