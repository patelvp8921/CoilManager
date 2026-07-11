import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, map } from 'rxjs';
import { environment } from '../../../../environments/environment';
import {
  ApiPagedResponse,
  ApiResponse,
  CompleteSlittingRequest,
  CompleteSlittingResponse,
  CreateSlittingJobRequest,
  SlittingJob,
  SlittingJobQuery,
  SlittingMotherCoilLookup,
  StartSlittingRequest,
  StartSlittingResponse,
  UpdateSlittingJobRequest,
} from '../models/slitting-job.model';

@Injectable({ providedIn: 'root' })
export class SlittingJobService {
  private readonly http = inject(HttpClient);
  private readonly endpoint = `${environment.apiBaseUrl}/slitting-jobs`;

  getSlittingJobs(query: SlittingJobQuery): Observable<ApiPagedResponse<SlittingJob>> {
    let params = new HttpParams()
      .set('page', query.page)
      .set('pageSize', query.pageSize);

    params = this.setOptional(params, 'search', query.search);
    params = this.setOptional(params, 'sortBy', query.sortBy);
    params = this.setOptional(params, 'sortDirection', query.sortDirection);

    if (query.status !== undefined && query.status !== null) {
      params = params.set('status', query.status);
    }

    return this.http.get<ApiPagedResponse<SlittingJob>>(this.endpoint, { params });
  }

  getNextJobNumber(): Observable<string> {
    return this.http.get<ApiResponse<string>>(`${this.endpoint}/next-job-number`).pipe(map((response) => this.unwrap(response)));
  }

  getSlittingJobById(id: string): Observable<SlittingJob> {
    return this.http.get<ApiResponse<SlittingJob>>(`${this.endpoint}/${id}`).pipe(map((response) => this.unwrap(response)));
  }

  searchMotherCoils(search: string): Observable<readonly SlittingMotherCoilLookup[]> {
    const params = search ? new HttpParams().set('search', search) : undefined;
    return this.http
      .get<ApiResponse<readonly SlittingMotherCoilLookup[]>>(`${this.endpoint}/mother-coils`, { params })
      .pipe(map((response) => this.unwrap(response)));
  }

  createSlittingJob(request: CreateSlittingJobRequest): Observable<SlittingJob> {
    return this.http.post<ApiResponse<SlittingJob>>(this.endpoint, request).pipe(map((response) => this.unwrap(response)));
  }

  updateSlittingJob(id: string, request: UpdateSlittingJobRequest): Observable<SlittingJob> {
    return this.http.put<ApiResponse<SlittingJob>>(`${this.endpoint}/${id}`, request).pipe(map((response) => this.unwrap(response)));
  }

  releaseSlittingJob(id: string): Observable<SlittingJob> {
    return this.http.post<ApiResponse<SlittingJob>>(`${this.endpoint}/${id}/release`, {}).pipe(map((response) => this.unwrap(response)));
  }

  cancelSlittingJob(id: string): Observable<SlittingJob> {
    return this.http.post<ApiResponse<SlittingJob>>(`${this.endpoint}/${id}/cancel`, {}).pipe(map((response) => this.unwrap(response)));
  }

  startSlittingJob(id: string, request: StartSlittingRequest): Observable<StartSlittingResponse> {
    return this.http.post<ApiResponse<StartSlittingResponse>>(`${this.endpoint}/${id}/start`, request)
      .pipe(map((response) => this.unwrap(response)));
  }

  completeSlittingJob(id: string, request: CompleteSlittingRequest): Observable<CompleteSlittingResponse> {
    return this.http.post<ApiResponse<CompleteSlittingResponse>>(`${this.endpoint}/${id}/complete`, request)
      .pipe(map((response) => this.unwrap(response)));
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
