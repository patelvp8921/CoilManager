import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../../features/raw-coil/models/raw-coil.model';
import { LookupItem } from '../models/lookup-item.model';

@Injectable({ providedIn: 'root' })
export class LookupService {
  private readonly http = inject(HttpClient);
  private readonly endpoint = `${environment.apiBaseUrl}/lookups`;

  getSuppliers(): Observable<readonly LookupItem[]> {
    return this.getLookup('suppliers');
  }

  getManufacturers(): Observable<readonly LookupItem[]> {
    return this.getLookup('manufacturers');
  }

  getGrades(): Observable<readonly LookupItem[]> {
    return this.getLookup('grades');
  }

  private getLookup(path: string): Observable<readonly LookupItem[]> {
    return this.http
      .get<ApiResponse<readonly LookupItem[]>>(`${this.endpoint}/${path}`)
      .pipe(map((response) => response.data ?? []));
  }
}
