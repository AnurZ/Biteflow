import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { ActivationRequestsListComponent } from '../admin/activation-requests/activation-requests-list.component';

@Component({
  selector: 'app-superadmin-activation-requests',
  standalone: true,
  imports: [CommonModule, ActivationRequestsListComponent],
  templateUrl: './superadmin-activation-requests.component.html',
  styleUrl: './superadmin-activation-requests.component.css',
})
export class SuperAdminActivationRequestsComponent {}
