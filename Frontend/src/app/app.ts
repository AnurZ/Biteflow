import { Component, signal } from '@angular/core';
import {UploadOverlayService} from './services/upload-overlay-service/upload-overlay-service';

@Component({
  selector: 'app-root',
  templateUrl: './app.html',
  standalone: false,
  styleUrl: './app.css'
})
export class App {
  protected readonly title = signal('Frontend');
  constructor(public uploadOverlay: UploadOverlayService) {
  }
}
