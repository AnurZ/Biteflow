import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-upload-overlay',
  templateUrl: './upload-overlay.component.html',
  styleUrls: ['./upload-overlay.component.css'],
  standalone: false
})
export class UploadOverlayComponent {

  @Input() visible = false;
  @Input() progress = 0;

}
