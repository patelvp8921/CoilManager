import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { map, Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { ApiPagedResponse, ApiResponse, InventoryTransaction, SlitCoil, SlitCoilDetails, SlitCoilQuery } from '../models/slit-coil.model';
@Injectable({ providedIn: 'root' })
export class SlitCoilService {
  private readonly http=inject(HttpClient); private readonly endpoint=`${environment.apiBaseUrl}/slit-coils`;
  getSlitCoils(query:SlitCoilQuery):Observable<ApiPagedResponse<SlitCoil>> { let params=new HttpParams().set('page',query.page).set('pageSize',query.pageSize); for(const [key,value] of Object.entries(query)){if(key!=='page'&&key!=='pageSize'&&value!==undefined&&value!==null&&value!=='')params=params.set(key,String(value));} return this.http.get<ApiPagedResponse<SlitCoil>>(this.endpoint,{params}); }
  getById(id:string):Observable<SlitCoilDetails>{return this.http.get<ApiResponse<SlitCoilDetails>>(`${this.endpoint}/${encodeURIComponent(id)}`).pipe(map(r=>r.data!));}
  getTransactions(number:string):Observable<readonly InventoryTransaction[]>{return this.http.get<ApiResponse<readonly InventoryTransaction[]>>(`${environment.apiBaseUrl}/coils/${encodeURIComponent(number)}/inventory-transactions`).pipe(map(r=>r.data??[]));}
}
