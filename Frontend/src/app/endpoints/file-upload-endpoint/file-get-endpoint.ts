import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class FileGetEndpoint {
  private apiUrl = 'https://localhost:7260/api/File';

  constructor(private http: HttpClient) {}

  /**
   * Get the public URL of a file by its filename
   * @param fileName The file name returned by the upload endpoint
   */
  getFileUrl(fileName: string): Observable<{ url: string }> {
    return this.http.get<{ url: string }>(`${this.apiUrl}/${fileName}`);
  }
}
