import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import {MyConfig} from '../../my-config';

export interface DashboardLayoutDto {
  layoutJson: string;
}

@Injectable({
  providedIn: 'root'
})
export class DashboardLayoutService {

  private baseUrl = `${MyConfig.api_address}/dashboard-layout`;

  constructor(private http: HttpClient) {}

  getLayout(): Observable<DashboardLayoutDto> {
    return this.http.get<DashboardLayoutDto>(this.baseUrl);
  }

  saveLayout(layoutJson: string): Observable<void> {
    return this.http.post<void>(this.baseUrl, {
      layoutJson
    });
  }
}
