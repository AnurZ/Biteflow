import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { MyConfig } from '../../my-config';
import { NotificationListRequest, NotificationListResponse } from './notification.model';

@Injectable({
  providedIn: 'root'
})
export class NotificationApiService {
  private http = inject(HttpClient);
  private base = `${MyConfig.api_address}/notifications`;

  list(request: NotificationListRequest): Observable<NotificationListResponse> {
    let params = new HttpParams();
    if (request.pageNumber) params = params.set('pageNumber', request.pageNumber);
    if (request.pageSize) params = params.set('pageSize', request.pageSize);
    if (request.unreadOnly !== undefined) params = params.set('unreadOnly', request.unreadOnly);
    if (request.type) params = params.set('type', request.type);

    return this.http.get<NotificationListResponse>(this.base, { params });
  }

  markRead(id: number): Observable<void> {
    return this.http.post<void>(`${this.base}/${id}/read`, {});
  }

  markAllRead(): Observable<void> {
    return this.http.post<void>(`${this.base}/read-all`, {});
  }
}
