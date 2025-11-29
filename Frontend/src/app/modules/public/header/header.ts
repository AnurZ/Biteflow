import { Component } from '@angular/core';
import {AuthService} from '../../../services/auth-services/auth.service';
import {Router} from '@angular/router';

@Component({
  selector: 'app-header',
  standalone: false,
  templateUrl: './header.html',
  styleUrl: './header.css'
})
export class Header {

  constructor(public authService: AuthService, public router: Router) {
  }

  get isAdmin(): boolean {
    const roles = this.authService.getRoles();
    return roles.includes('admin') || roles.includes('superadmin');
  }

  get isStaff(): boolean {
    const roles = this.authService.getRoles();
    return roles.includes('staff') || this.isAdmin;
  }

  isOnLoginPage():boolean{
    return this.router.url.includes('auth/login');
  }

}
