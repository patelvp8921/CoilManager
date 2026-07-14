import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { map } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Allocation, ApiPagedResponse, ApiResponse, AvailableCoil, CoilType, WorkOrder, WorkOrderListItem, WorkOrderRequest } from './work-order.model';

@Injectable({ providedIn: 'root' })
export class WorkOrderService {
  private readonly http = inject(HttpClient); private readonly url = `${environment.apiBaseUrl}/work-orders`;
  list(query: Record<string, unknown>) { let params = new HttpParams(); Object.entries(query).forEach(([k,v]) => { if (v !== null && v !== undefined && v !== '') params = params.set(k, String(v)); }); return this.http.get<ApiPagedResponse<WorkOrderListItem>>(this.url, { params }); }
  nextNumber() { return this.http.get<ApiResponse<string>>(`${this.url}/next-number`).pipe(map(r => this.unwrap(r))); }
  get(id: string) { return this.http.get<ApiResponse<WorkOrder>>(`${this.url}/${id}`).pipe(map(r => this.unwrap(r))); }
  create(request: WorkOrderRequest) { return this.http.post<ApiResponse<WorkOrder>>(this.url, request).pipe(map(r => this.unwrap(r))); }
  update(id: string, request: WorkOrderRequest) { return this.http.put<ApiResponse<WorkOrder>>(`${this.url}/${id}`, request).pipe(map(r => this.unwrap(r))); }
  action(id: string, action: string) { return this.http.post<ApiResponse<WorkOrder>>(`${this.url}/${id}/${action}`, {}).pipe(map(r => this.unwrap(r))); }
  available(id: string, type: CoilType, search = '') { const segment = type === CoilType.MotherCoil ? 'available-mother-coils' : 'available-slit-coils'; const params = search ? new HttpParams().set('search', search) : undefined; return this.http.get<ApiResponse<readonly AvailableCoil[]>>(`${this.url}/${id}/${segment}`, { params }).pipe(map(r => this.unwrap(r))); }
  allocate(id: string, coilType: CoilType, coilId: string, allocatedWeight: number, remarks?: string) { return this.http.post<ApiResponse<Allocation>>(`${this.url}/${id}/allocations`, { coilType, coilId, allocatedWeight, remarks }).pipe(map(r => this.unwrap(r))); }
  releaseAllocation(id: string, allocationId: string) { return this.http.post(`${this.url}/${id}/allocations/${allocationId}/release`, {}); }
  private unwrap<T>(r: ApiResponse<T>): T { if (!r.success || r.data === null) throw new Error(r.errors.join('\n') || r.message); return r.data; }
}
