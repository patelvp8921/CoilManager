import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, map } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { ApiPagedResponse, ApiResponse, MasterQuery, MasterRecord, MasterRequest, MasterType } from './master-data.model';

@Injectable({ providedIn: 'root' })
export class MasterDataService {
  private readonly http = inject(HttpClient);
  private readonly endpoint = `${environment.apiBaseUrl}/admin`;

  getAll(type: MasterType, query: MasterQuery): Observable<ApiPagedResponse<MasterRecord>> {
    let params = new HttpParams()
      .set('page', query.page)
      .set('pageSize', query.pageSize);

    params = this.setOptional(params, 'search', query.search);
    params = this.setOptional(params, 'sortBy', query.sortBy);
    params = this.setOptional(params, 'sortDirection', query.sortDirection);

    if (query.isActive !== undefined && query.isActive !== null) {
      params = params.set('isActive', query.isActive);
    }

    return this.http.get<ApiPagedResponse<MasterRecord>>(`${this.endpoint}/${type}`, { params });
  }

  getById(type: MasterType, id: string): Observable<MasterRecord> {
    return this.http.get<ApiResponse<MasterRecord>>(`${this.endpoint}/${type}/${id}`).pipe(map((response) => this.unwrap(response)));
  }

  create(type: MasterType, request: MasterRequest): Observable<MasterRecord> {
    return this.http.post<ApiResponse<MasterRecord>>(`${this.endpoint}/${type}`, request).pipe(map((response) => this.unwrap(response)));
  }

  update(type: MasterType, id: string, request: MasterRequest): Observable<MasterRecord> {
    return this.http.put<ApiResponse<MasterRecord>>(`${this.endpoint}/${type}/${id}`, request).pipe(map((response) => this.unwrap(response)));
  }

  setActive(type: MasterType, id: string, isActive: boolean): Observable<MasterRecord> {
    const action = isActive ? 'activate' : 'deactivate';
    return this.http.patch<ApiResponse<MasterRecord>>(`${this.endpoint}/${type}/${id}/${action}`, {}).pipe(map((response) => this.unwrap(response)));
  }

  private setOptional(params: HttpParams, key: string, value: unknown): HttpParams {
    return value === undefined || value === null || value === '' ? params : params.set(key, String(value));
  }

  private unwrap<T>(response: ApiResponse<T>): T {
    if (!response.success || response.data === null) {
      throw new Error(response.errors.join('\n') || response.message);
    }

    return response.data;
  }
}
