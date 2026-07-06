import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, map, shareReplay } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../../features/raw-coil/models/raw-coil.model';
import { LookupItem } from '../models/lookup-item.model';

@Injectable({ providedIn: 'root' })
export class LookupService {
  private readonly http = inject(HttpClient);
  private readonly endpoint = `${environment.apiBaseUrl}/lookups`;
  private readonly suppliers$ = this.http
    .get<ApiResponse<readonly LookupItem[]>>(`${this.endpoint}/suppliers`)
    .pipe(
      map((response) => response.data ?? []),
      shareReplay({ bufferSize: 1, refCount: true }),
    );
  private readonly manufacturers$ = this.http
    .get<ApiResponse<readonly LookupItem[]>>(`${this.endpoint}/manufacturers`)
    .pipe(
      map((response) => response.data ?? []),
      shareReplay({ bufferSize: 1, refCount: true }),
    );
  private readonly grades$ = this.http
    .get<ApiResponse<readonly LookupItem[]>>(`${this.endpoint}/grades`)
    .pipe(
      map((response) => response.data ?? []),
      shareReplay({ bufferSize: 1, refCount: true }),
    );

  getSuppliers(): Observable<readonly LookupItem[]> {
    return this.suppliers$;
  }

  getManufacturers(): Observable<readonly LookupItem[]> {
    return this.manufacturers$;
  }

  getGrades(): Observable<readonly LookupItem[]> {
    return this.grades$;
  }
}
