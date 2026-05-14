import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { MyConfig } from '../../my-config';
import { HttpEvent, HttpEventType, HttpRequest } from '@angular/common/http';
import { map } from 'rxjs/operators';

@Injectable({
  providedIn: 'root'
})
export class FileUploadEndpoint {
  private readonly base = `${MyConfig.api_address}/file`;

  constructor(private http: HttpClient) {}

  uploadFile(file: File): Observable<HttpEvent<any>> {

    const formData = new FormData();
    formData.append('file', file);

    return this.http.post(`${this.base}/upload`, formData, {
      reportProgress: true,
      observe: 'events',
      responseType: 'json'
    });
  }

  getImage(fileName: string): Observable<Blob> {
    return this.http.get(`${this.base}/${fileName}`, { responseType: 'blob' });
  }
}
