import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { MyConfig } from '../../my-config';

@Injectable({
  providedIn: 'root'
})
export class FileUploadEndpoint {
  private readonly base = `${MyConfig.api_address}/file`;

  constructor(private http: HttpClient) {}

  uploadFile(file: File): Observable<{ url: string }> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post<{ url: string }>(`${this.base}/upload`, formData);
  }

  getImage(fileName: string): Observable<Blob> {
    return this.http.get(`${this.base}/${fileName}`, { responseType: 'blob' });
  }
}
