import { Routes } from '@angular/router';
import { authRoutes } from './features/auth/auth.routes';
import { authGuard } from './features/auth/auth.guard';
import { guestGuard } from './features/auth/guest.guard';
import { dashboardRoutes } from './features/dashboard/dasboard.routes';

export const routes: Routes = [
  { path: 'auth', canMatch: [guestGuard], children: authRoutes },
  { path: 'dashboard', canActivate: [authGuard], children: dashboardRoutes },
  { path: '', redirectTo: '/auth/login', pathMatch: 'full' },
];
