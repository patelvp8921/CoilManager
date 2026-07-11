import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { ApiPagedResponse, SlitCoil, SlitCoilQuery } from '../models/slit-coil.model';

@Injectable({ providedIn: 'root' })
export class SlitCoilService {
  private readonly http = inject(HttpClient);
  private readonly endpoint = `${environment.apiBaseUrl}/slit-coils`;

  getSlitCoils(query: SlitCoilQuery): Observable<ApiPagedResponse<SlitCoil>> {
    let params = new HttpParams()
      .set('page', query.page)
      .set('pageSize', query.pageSize);

    params = this.setOptional(params, 'search', query.search);
    params = this.setOptional(params, 'sortBy', query.sortBy);
    params = this.setOptional(params, 'sortDirection', query.sortDirection);

    if (query.status !== undefined && query.status !== null) {
      params = params.set('status', query.status);
    }

    return this.http.get<ApiPagedResponse<SlitCoil>>(this.endpoint, { params });
  }

  private setOptional(params: HttpParams, key: string, value: unknown): HttpParams {
    if (value === undefined || value === null || value === '') {
      return params;
    }

    return params.set(key, String(value));
  }
}
