import { Routes } from '@angular/router';
import { DashboardUser } from './pages/user/dashboard-user';
import { DashboardAdmin } from './pages/admin/dashboard-admin';

export const dashboardRoutes: Routes = [
  {
    path: 'user',
    data: { roles: ['user', 'admin'] },
    component: DashboardUser,
  },
  {
    path: 'admin',
    data: { roles: ['admin'] },
    component: DashboardAdmin,
  },
];
