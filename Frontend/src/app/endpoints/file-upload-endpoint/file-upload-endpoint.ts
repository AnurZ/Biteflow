import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class FileUploadEndpoint {
  private readonly baseUrl = 'https://localhost:7260/api/File';

  constructor(private http: HttpClient) {}

  uploadFile(file: File): Observable<{ url: string }> {
    const formData = new FormData();
    formData.append('File', file);
    return this.http.post<{ url: string }>(`${this.baseUrl}/upload`, formData);
  }

  getImage(fileName: string): Observable<Blob> {
    return this.http.get(`${this.baseUrl}/${fileName}`, { responseType: 'blob' });
  }
}
