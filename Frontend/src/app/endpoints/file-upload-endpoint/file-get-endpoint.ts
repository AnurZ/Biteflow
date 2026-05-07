import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { MyConfig } from '../../my-config';

@Injectable({
  providedIn: 'root',
})
export class FileGetEndpoint {
  private base = `${MyConfig.api_address}/file`;

  constructor(private http: HttpClient) {}

  /**
   * Get the public URL of a file by its filename
   * @param fileName The file name returned by the upload endpoint
   */
  getFileUrl(fileName: string): Observable<{ url: string }> {
    return this.http.get<{ url: string }>(`${this.base}/${fileName}`);
  }
}
