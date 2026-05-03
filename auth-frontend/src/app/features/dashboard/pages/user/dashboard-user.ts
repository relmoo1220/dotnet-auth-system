import { Component } from '@angular/core';
import { JsonPipe, CommonModule } from '@angular/common';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { AuthService } from '../../../auth/auth.service';
import { Router } from '@angular/router';
import { environment } from '../../../../../environments/environment';
import { HttpClient, HttpResponse } from '@angular/common/http';

@Component({
  selector: 'app-dashboard-user',
  imports: [CommonModule, JsonPipe, ButtonModule, CardModule],
  templateUrl: './dashboard-user.html',
  styleUrl: './dashboard-user.css',
  standalone: true,
})
export class DashboardUser {
  constructor(
    private http: HttpClient,
    private authService: AuthService,
    private router: Router,
  ) {}

  private apiUrl = environment.apiUrl;

  responseStatus: number | null = null;
  responseBody: any = null;
  loading = false;

  logout() {
    this.authService.logout().subscribe({
      next: () => {
        this.router.navigate(['/auth/login']);
      },
      error: () => {
        this.router.navigate(['/auth/login']);
      },
    });
  }

  fetchUserBooks() {
    this.loading = true;
    this.responseStatus = null;
    this.responseBody = null;

    this.http.get(`${this.apiUrl}/books/user`, { observe: 'response' }).subscribe({
      next: (res: HttpResponse<any>) => {
        this.responseStatus = res.status;
        this.responseBody = res.body;
        this.loading = false;
      },
      error: (err) => {
        this.responseStatus = err.status;
        this.responseBody = err.error;
        this.loading = false;
      },
    });
  }

  fetchAdminBooks() {
    this.loading = true;
    this.responseStatus = null;
    this.responseBody = null;

    this.http.get(`${this.apiUrl}/books/admin`, { observe: 'response' }).subscribe({
      next: (res: HttpResponse<any>) => {
        this.responseStatus = res.status;
        this.responseBody = res.body;
        this.loading = false;
      },
      error: (err) => {
        this.responseStatus = err.status;
        this.responseBody = err.error;
        this.loading = false;
      },
    });
  }
}
