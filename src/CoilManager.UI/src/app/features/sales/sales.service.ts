import {HttpClient,HttpParams} from '@angular/common/http';
import {inject,Injectable} from '@angular/core';
import {catchError,map} from 'rxjs';
import {environment} from '../../../environments/environment';
import {ApiPagedResponse,ApiResponse,Customer,CustomerRequest,SalesOrder,SalesOrderRequest} from './sales.model';
@Injectable({providedIn:'root'})
export class SalesService{
 private http=inject(HttpClient);private customersUrl=`${environment.apiBaseUrl}/customers`;private ordersUrl=`${environment.apiBaseUrl}/sales-orders`;
 customers(filters:Record<string,unknown>={}){return this.http.get<ApiPagedResponse<Customer>>(this.customersUrl,{params:this.params(filters)});}
 customer(id:string){return this.http.get<ApiResponse<Customer>>(`${this.customersUrl}/${id}`).pipe(map(r=>r.data!));}
 nextCustomerCode(){return this.http.get<ApiResponse<string>>(`${this.customersUrl}/next-code`).pipe(
  map(r=>r.data!),
  catchError(()=>this.customers({page:1,pageSize:100}).pipe(map(r=>{
   const next=r.data.reduce((highest,customer)=>{
    const match=/^CUS-(d+)$/.exec(customer.customerCode);
    return Math.max(highest,match?Number(match[1]):0);
   },0)+1;
   return `CUS-${String(next).padStart(5,'0')}`;
  })))
 );}
 saveCustomer(request:CustomerRequest,id?:string){return id?this.http.put<ApiResponse<Customer>>(`${this.customersUrl}/${id}`,request).pipe(map(r=>r.data!)):this.http.post<ApiResponse<Customer>>(this.customersUrl,request).pipe(map(r=>r.data!));}
 setCustomerActive(id:string,active:boolean){return this.http.post<ApiResponse<Customer>>(`${this.customersUrl}/${id}/${active?'activate':'deactivate'}`,{}).pipe(map(r=>r.data!));}
 orders(filters:Record<string,unknown>={}){return this.http.get<ApiPagedResponse<SalesOrder>>(this.ordersUrl,{params:this.params(filters)});}
 order(id:string){return this.http.get<ApiResponse<SalesOrder>>(`${this.ordersUrl}/${id}`).pipe(map(r=>r.data!));}
 nextOrderNumber(){return this.http.get<ApiResponse<string>>(`${this.ordersUrl}/next-number`).pipe(map(r=>r.data!));}
 saveOrder(request:SalesOrderRequest,id?:string){return id?this.http.put<ApiResponse<SalesOrder>>(`${this.ordersUrl}/${id}`,request).pipe(map(r=>r.data!)):this.http.post<ApiResponse<SalesOrder>>(this.ordersUrl,request).pipe(map(r=>r.data!));}
 uploadOrderLineDrawing(orderId:string,lineId:string,file:File){const data=new FormData();data.append('file',file);return this.http.post<ApiResponse<object>>(`${this.ordersUrl}/${orderId}/lines/${lineId}/drawing`,data);}
 deleteOrder(id:string){return this.http.delete(`${this.ordersUrl}/${id}`);}
 transition(id:string,action:'confirm'|'hold'|'release-hold',body:unknown={}){return this.http.post<ApiResponse<SalesOrder>>(`${this.ordersUrl}/${id}/${action}`,body).pipe(map(r=>r.data!));}
 cancel(id:string,reason:string,rowVersion?:string){return this.http.post<ApiResponse<SalesOrder>>(`${this.ordersUrl}/${id}/cancel`,{reason,rowVersion}).pipe(map(r=>r.data!));}
 private params(values:Record<string,unknown>){let p=new HttpParams();for(const[k,v]of Object.entries(values))if(v!==null&&v!==undefined&&v!=='')p=p.set(k,String(v));return p;}
}
