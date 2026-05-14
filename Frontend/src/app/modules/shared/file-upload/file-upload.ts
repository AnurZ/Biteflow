import { Component, EventEmitter, Input, Output, OnChanges } from '@angular/core';
import { HttpEventType } from '@angular/common/http';

import { FileUploadEndpoint } from '../../../endpoints/file-upload-endpoint/file-upload-endpoint';
import { UploadOverlayService } from '../../../services/upload-overlay-service/upload-overlay-service';

@Component({
  standalone: false,
  selector: 'app-file-upload',
  templateUrl: './file-upload.html'
})
export class FileUploadComponent implements OnChanges {

  constructor(
    private uploadEp: FileUploadEndpoint,
    private overlay: UploadOverlayService
  ) {}

  @Input() file: File | null = null;

  @Output() uploaded = new EventEmitter<string>();

  ngOnChanges() {

    if (!this.file) return;

    this.overlay.show();

    this.uploadEp.uploadFile(this.file).subscribe({
      next: (event) => {

        console.log(event);

        // 1. UPLOAD PROGRESS
        if (event.type === HttpEventType.UploadProgress) {

          const progress = event.total
            ? Math.round((100 * event.loaded) / event.total)
            : 0;

          this.overlay.setProgress(progress);
        }

        // 2. DOWNLOAD PROGRESS (BITNO - kod tebe se dešava!)
        if (event.type === HttpEventType.DownloadProgress) {

          const progress = event.total
            ? Math.round((100 * event.loaded) / event.total)
            : 0;

          this.overlay.setProgress(progress);
        }

        // 3. RESPONSE
        if (event.type === HttpEventType.Response) {

          const body = event.body as any;

          this.overlay.setProgress(100);

          this.uploaded.emit(body.url);

          setTimeout(() => this.overlay.hide(), 300);
        }
      },

      error: () => this.overlay.hide()
    });
  }
}
