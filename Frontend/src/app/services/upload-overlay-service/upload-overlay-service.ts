import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class UploadOverlayService {

  visible = false;
  progress = 0;

  show() {
    this.visible = true;
  }

  hide() {
    this.visible = false;
    this.progress = 0;
  }

  setProgress(value: number) {
    this.progress = value;
  }
}
