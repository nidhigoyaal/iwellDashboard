import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';
import { BehaviorSubject, Observable, of } from 'rxjs';
import { tap } from 'rxjs/operators';
import { Login } from '../Models/login.model';
import { Register } from '../shared/account.models';

export interface DecodedToken {
  sub?: string;
  name?: string;
  role?: string | string[];
  exp?: number;
  [k: string]: any;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private tokenKey = 'iw_token';
  private currentUserSubject = new BehaviorSubject<DecodedToken | null>(this.decodeTokenFromStorage());
  public currentUser$ = this.currentUserSubject.asObservable();

  constructor(private http: HttpClient) {}

  login(loginData: Login): Observable<{ token: string }> {
    return this.http.post<{ token: string }>(`${environment.iwellApiUrl}/Account/login`, loginData)
      .pipe(tap(res => {
        if (res?.token) {
          localStorage.setItem(this.tokenKey, res.token);
          this.currentUserSubject.next(this.decodeJwt(res.token));
        }
      }));
  }

  register(registerData: Register) {
    console.log(registerData)
    return this.http.post(`${environment.iwellApiUrl}/Account/register`, registerData );
  }

  logout() {
    localStorage.removeItem(this.tokenKey);
    this.currentUserSubject.next(null);
  }

  getToken(): string | null {
    return localStorage.getItem(this.tokenKey);
  }

  isAuthenticated(): boolean {
    const t = this.getToken();
    if (!t) return false;
    const decoded = this.decodeJwt(t);
    if (!decoded || !decoded.exp) return false;
    return decoded.exp * 1000 > Date.now();
  }

  getRole(): string | null {
    const cur = this.currentUserSubject.value;
    if (!cur) return null;
    const r = cur['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];
    if (Array.isArray(r)) return (r as string[])[0] ?? null;

    return r as string;
  }

  getUserName(): string | null {
    const cur = this.currentUserSubject.value;
    if (!cur) return null;
    const r = cur['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'];
    if (Array.isArray(r)) return (r as string[])[0] ?? null;

    return r as string;
  }

  isTokenExpired(token?: string): boolean {
    if (!token) token = this.getToken() || '';
    if (!token) return true;

    const payload = JSON.parse(atob(token.split('.')[1]));
    const expiry = payload.exp;
    if (!expiry) return true;

    return (Math.floor(new Date().getTime() / 1000)) >= expiry;
  }

  /** Decode a JWT without external libs (no verification) */
  private decodeJwt(token: string): DecodedToken | null {
    try {
      const parts = token.split('.');
      if (parts.length !== 3) return null;
      const payload = parts[1].replace(/-/g, '+').replace(/_/g, '/');
      const json = decodeURIComponent(atob(payload).split('').map(c => {
        return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
      }).join(''));
      return JSON.parse(json);
    } catch {
      return null;
    }
  }

  private decodeTokenFromStorage(): DecodedToken | null {
    const t = this.getToken();
    return t ? this.decodeJwt(t) : null;
  }
}
