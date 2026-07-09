import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, map } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { ApiResponse, OperationsDashboard } from '../models/operations-dashboard.model';

@Injectable({ providedIn: 'root' })
export class DashboardService {
  private readonly http = inject(HttpClient);
  private readonly endpoint = `${environment.apiBaseUrl}/dashboard`;

  getOperationsDashboard(): Observable<OperationsDashboard> {
    return this.http
      .get<ApiResponse<OperationsDashboard>>(`${this.endpoint}/operations`)
      .pipe(map((response) => this.unwrap(response)));
  }

  private unwrap<T>(response: ApiResponse<T>): T {
    if (!response.success || response.data === null) {
      throw new Error(response.errors.join('\n') || response.message);
    }

    return response.data;
  }
}
