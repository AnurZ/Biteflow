import {inject, Injectable} from '@angular/core';
import {HttpClient, HttpParams} from '@angular/common/http';
import {
  ActivationDraftDto,
  ConfirmActivationResult,
  CreateDraftCommand, PageResult,
  UpdateDraftCommand
} from '../../modules/public/models/activation.models';
import { MyConfig } from '../../my-config';

@Injectable({
  providedIn: 'root'
})
export class ActivationRequests {
  private http = inject(HttpClient);
  private base = `${MyConfig.api_address}/activation-requests`;



  createDraft(body: CreateDraftCommand) {
    return this.http.post<number>(this.base, body);
  }

  updateDraft(body: UpdateDraftCommand) {
    return this.http.put<void>(`${this.base}/${body.id}`, body);
  }

  getDraft(id: number) {
    return this.http.get<ActivationDraftDto>(`${this.base}/${id}`);
  }

  submit(id: number) {
    return this.http.post<void>(`${this.base}/${id}/submit`, {});
  }

  confirm(token: string) {
    return this.http.post<ConfirmActivationResult>(`${this.base}/confirm`, { token }, { responseType: 'json' as const });
  }

  list(status?: number) {
    let params = new HttpParams();
    if (status !== undefined) params = params.set('status', String(status));
    return this.http.get<PageResult<ActivationDraftDto>>(this.base, { params });
  }

  approve(id: number) {
    return this.http.post<string>(`${this.base}/${id}/approve`, {}, { responseType: 'text' as 'json' });
  }

  reject(id: number, reason: string) {
    return this.http.post<void>(`${this.base}/${id}/reject`, { reason });
  }
}
