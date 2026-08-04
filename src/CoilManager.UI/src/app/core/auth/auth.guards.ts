import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from './auth.service';
export const authGuard:CanActivateFn=(_,state)=>{const a=inject(AuthService);return a.authenticated()||inject(Router).createUrlTree(['/auth/login'],{queryParams:{returnUrl:state.url}});};
export const anonymousGuard:CanActivateFn=()=>!inject(AuthService).authenticated()||inject(Router).createUrlTree(['/dashboard']);
export const permissionGuard:CanActivateFn=route=>inject(AuthService).hasPermission(route.data['permission'])||inject(Router).createUrlTree(['/unauthorized']);
