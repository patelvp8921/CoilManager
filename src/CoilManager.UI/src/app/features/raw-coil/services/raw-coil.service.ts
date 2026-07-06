import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, map } from 'rxjs';
import { environment } from '../../../../environments/environment';
import {
  ApiPagedResponse,
  ApiResponse,
  CreateRawCoilRequest,
  RawCoil,
  UpdateRawCoilRequest,
} from '../models/raw-coil.model';
import { RawCoilQuery } from '../models/raw-coil-query.model';

@Injectable({ providedIn: 'root' })
export class RawCoilService {
  private readonly http = inject(HttpClient);
  private readonly endpoint = `${environment.apiBaseUrl}/rawcoils`;

  getRawCoils(query: RawCoilQuery): Observable<ApiPagedResponse<RawCoil>> {
    let params = new HttpParams()
      .set('page', query.page)
      .set('pageSize', query.pageSize);

    params = this.setOptional(params, 'search', query.search);
    params = this.setOptional(params, 'grade', query.grade);
    params = this.setOptional(params, 'manufacturer', query.manufacturer);
    params = this.setOptional(params, 'sortBy', query.sortBy);
    params = this.setOptional(params, 'sortDirection', query.sortDirection);

    if (query.status) {
      params = params.set('status', query.status);
    }

    return this.http.get<ApiPagedResponse<RawCoil>>(this.endpoint, { params });
  }

  getRawCoilById(id: string): Observable<RawCoil> {
    return this.http.get<ApiResponse<RawCoil>>(`${this.endpoint}/${id}`).pipe(map((response) => this.unwrap(response)));
  }

  getNextCoilId(): Observable<string> {
    return this.http.get<ApiResponse<string>>(`${this.endpoint}/next-coil-id`).pipe(map((response) => this.unwrap(response)));
  }

  createRawCoil(request: CreateRawCoilRequest): Observable<RawCoil> {
    return this.http.post<ApiResponse<RawCoil>>(this.endpoint, request).pipe(map((response) => this.unwrap(response)));
  }

  updateRawCoil(id: string, request: UpdateRawCoilRequest): Observable<RawCoil> {
    return this.http.put<ApiResponse<RawCoil>>(`${this.endpoint}/${id}`, request).pipe(map((response) => this.unwrap(response)));
  }

  deleteRawCoil(id: string): Observable<void> {
    return this.http.delete<ApiResponse<unknown>>(`${this.endpoint}/${id}`).pipe(map(() => undefined));
  }

  private setOptional(params: HttpParams, key: string, value: unknown): HttpParams {
    if (value === undefined || value === null || value === '') {
      return params;
    }

    return params.set(key, String(value));
  }

  private unwrap<T>(response: ApiResponse<T>): T {
    if (!response.success || response.data === null) {
      throw new Error(response.errors.join('\n') || response.message);
    }

    return response.data;
  }
}
