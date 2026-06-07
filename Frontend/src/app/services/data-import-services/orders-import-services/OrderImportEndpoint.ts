import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { MyConfig } from '../../../my-config';

@Injectable({
  providedIn: 'root'
})
export class OrderImportEndpoint {

  private base = `${MyConfig.api_address}/OrderImport`;

  constructor(private http: HttpClient) {}

  importCsv(file: File) {
    const formData = new FormData();
    formData.append('file', file);

    return this.http.post(
      `${this.base}/csv`,
      formData
    );
  }

  importExcel(file: File) {
    const formData = new FormData();
    formData.append('file', file);

    return this.http.post(
      `${this.base}/xlsx`,
      formData
    );
  }
}
