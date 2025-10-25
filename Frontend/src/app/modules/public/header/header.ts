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


  isOnLoginPage():boolean{
    return this.router.url.includes('auth/login');
  }

}
