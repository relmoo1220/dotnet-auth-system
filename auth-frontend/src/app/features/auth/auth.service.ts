import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of, map, tap, catchError } from 'rxjs';
import { LocalStorageService } from '../../storage/local-storage.service';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private accessTokenKey = 'access_token';
  private apiUrl = environment.apiUrl;

  constructor(
    private http: HttpClient,
    private localStorage: LocalStorageService,
  ) {}

  private parseToken(token: string): any | null {
    try {
      return JSON.parse(atob(token.split('.')[1]));
    } catch {
      return null;
    }
  }

  // ---------------- TOKEN ----------------
  getAccessToken(): string | null {
    return this.localStorage.getItem<string>(this.accessTokenKey);
  }

  setAccessToken(token: string): void {
    this.localStorage.setItem(this.accessTokenKey, token);
  }

  clear(): void {
    this.localStorage.removeItem(this.accessTokenKey);
  }

  // ---------------- LOGIN ----------------
  login(username: string, password: string): Observable<boolean> {
    return this.http
      .post<{ accessToken: string }>(`${this.apiUrl}/auth/login`, {
        username,
        password,
      })
      .pipe(
        tap((res) => this.setAccessToken(res.accessToken)),
        map(() => true),
      );
  }

  logout(): Observable<boolean> {
    return this.http.post(`${this.apiUrl}/auth/logout`, {}, { withCredentials: true }).pipe(
      tap(() => this.clear()),
      map(() => true),
      catchError(() => {
        this.clear();
        return of(true);
      }),
    );
  }

  // ---------------- JWT HELPERS ----------------
  getUserRole(): string | null {
    const token = this.getAccessToken();
    if (!token) return null;

    try {
      const payload = this.parseToken(token);
      return payload.role ?? null;
    } catch {
      return null;
    }
  }

  getDashboardRoute(): string {
    const role = this.getUserRole();
    return role === 'admin' ? '/dashboard/admin' : '/dashboard/user';
  }

  isTokenExpired(token: string): boolean {
    try {
      const payload = this.parseToken(token);
      return Date.now() >= payload.exp * 1000;
    } catch {
      return true;
    }
  }

  // ---------------- REFRESH ----------------
  refresh(): Observable<boolean> {
    return this.http
      .post<{ accessToken: string }>(`${this.apiUrl}/auth/refresh`, {}, { withCredentials: true })
      .pipe(
        tap((res) => this.setAccessToken(res.accessToken)),
        map(() => true),
        catchError(() => {
          this.clear();
          return of(false);
        }),
      );
  }
}
