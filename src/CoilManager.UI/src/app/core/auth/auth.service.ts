import { computed, inject, Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse, LoginChallenge, TokenResponse, UserIdentity } from './auth.models';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient); private readonly router = inject(Router);
  private readonly base = `${environment.apiBaseUrl}/auth`;
  private readonly token = signal<string | null>(sessionStorage.getItem('cm.access'));
  readonly user = signal<UserIdentity | null>(this.readUser());
  readonly authenticated = computed(() => !!this.token() && !!this.user());
  accessToken(): string | null { return this.token(); }
  hasPermission(permission: string): boolean { const u=this.user(); return !!u && (u.roles.includes('Administrator') || u.permissions.includes(permission)); }
  login(email:string,password:string,rememberMe:boolean): Observable<ApiResponse<TokenResponse>> { sessionStorage.setItem('cm.remember',String(rememberMe)); return this.http.post<ApiResponse<TokenResponse>>(`${this.base}/login`,{email,password,rememberMe}).pipe(tap(x=>this.accept(x.data))); }
  verify(challengeId:string,code:string): Observable<ApiResponse<TokenResponse>> { return this.http.post<ApiResponse<TokenResponse>>(`${this.base}/otp/verify`,{challengeId,code,deviceName:this.deviceName()}).pipe(tap(x=>this.accept(x.data))); }
  resend(challengeId:string): Observable<ApiResponse<LoginChallenge>> { return this.http.post<ApiResponse<LoginChallenge>>(`${this.base}/otp/resend`,{challengeId}); }
  forgotPassword(email:string): Observable<void> { return this.http.post<void>(`${this.base}/forgot-password`,{email}); }
  resetPassword(email:string,token:string,newPassword:string): Observable<void> { return this.http.post<void>(`${this.base}/reset-password`,{email,token,newPassword}); }
  refresh(): Observable<ApiResponse<TokenResponse>> { return this.http.post<ApiResponse<TokenResponse>>(`${this.base}/refresh`,{}).pipe(tap(x=>this.accept(x.data))); }
  logout(all=false): void { this.http.post<void>(`${this.base}/${all?'logout-all':'logout'}`,{}).subscribe({complete:()=>this.clear(),error:()=>this.clear()}); }
  clear(): void { this.token.set(null); this.user.set(null); ['cm.access','cm.user','cm.remember'].forEach(k=>sessionStorage.removeItem(k)); void this.router.navigateByUrl('/auth/login'); }
  private accept(value:TokenResponse):void { this.token.set(value.accessToken); this.user.set(value.user); sessionStorage.setItem('cm.access',value.accessToken); sessionStorage.setItem('cm.user',JSON.stringify(value.user)); }
  private readUser():UserIdentity|null { try { return JSON.parse(sessionStorage.getItem('cm.user')??'null'); } catch { return null; } }
  private deviceName():string { return `${navigator.platform || 'Web'} / ${navigator.userAgent.split(' ').slice(-2).join(' ')}`; }
}
