import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, switchMap, throwError } from 'rxjs';
import { AuthService } from './auth.service';

export const authInterceptor: HttpInterceptorFn = (request,next) => {
  const auth=inject(AuthService); const token=auth.accessToken();
  const credentialed=request.clone({withCredentials:true});
  const authorized=token ? credentialed.clone({setHeaders:{Authorization:`Bearer ${token}`}}) : credentialed;
  return next(authorized).pipe(catchError((error:HttpErrorResponse)=>{
    if(error.status!==401 || request.url.includes('/auth/')) return throwError(()=>error);
    return auth.refresh().pipe(switchMap(()=>next(credentialed.clone({setHeaders:{Authorization:`Bearer ${auth.accessToken()}`}}))),catchError(e=>{auth.clear();return throwError(()=>e);}));
  }));
};
