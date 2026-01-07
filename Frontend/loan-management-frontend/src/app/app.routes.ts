import { Routes } from '@angular/router';
import { LoginComponent } from './components/auth/login/login';
import { RegisterComponent } from './components/auth/register/register';


import { authGuard } from './guards/auth-guard-guard';
import { roleGuard } from './guards/role-guard-guard';
import { AdminUsersComponent } from './components/admin/users/users';

export const routes: Routes = [
  { path: 'auth/login', component: LoginComponent },
  { path: 'auth/register', component: RegisterComponent },

  {
    path: 'customer/dashboard',
    loadComponent: () =>
      import('./components/customer/dashboard/dashboard')
        .then(m => m.DashboardComponent),
    canActivate: [authGuard, roleGuard],
    data: { roles: ['Customer'] }
  },

  {
    path: 'loan-officer/dashboard',
    loadComponent: () =>
      import('./components/loan-officer/dashboard/dashboard')
        .then(m => m.DashboardComponent),
    canActivate: [authGuard, roleGuard],
    data: { roles: ['LoanOfficer'] }
  },

  {
    path: 'admin/dashboard',
    loadComponent: () =>
      import('./components/admin/dashboard/dashboard')
        .then(m => m.DashboardComponent),
    canActivate: [authGuard, roleGuard],
    data: { roles: ['Admin'] }
  },
  {
    path: 'admin/loans',
    loadComponent: () =>
      import('./components/admin/loans/loans')
        .then(m => m.AdminLoansComponent),
    canActivate: [authGuard, roleGuard],
    data: { roles: ['Admin'] }
  },

  {
    path: 'admin/users',
    component: AdminUsersComponent,
    canActivate: [authGuard, roleGuard],
    data: { roles: ['admin'] }
  },
  {
    path: 'admin/loan-types',
    loadComponent: () =>
      import('./components/admin/loan-types/loan-types')
        .then(m => m.AdminLoanTypesComponent),
    canActivate: [authGuard, roleGuard],
    data: { roles: ['admin'] }
  },

  {
    path: 'customer/apply-loan',
    loadComponent: () =>
      import('./components/customer/apply-loan/apply-loan')
        .then(m => m.ApplyLoanComponent),
    canActivate: [authGuard, roleGuard],
    data: { roles: ['Customer'] }
  },
  {
    path: 'customer/my-loans',
    loadComponent: () =>
      import('./components/customer/my-loans/my-loans')
        .then(m => m.MyLoansComponent),
    canActivate: [authGuard, roleGuard],
    data: { roles: ['Customer'] }
  },
  {
    path: 'customer/reports',
    loadComponent: () =>
      import('./components/customer/reports/reports')
        .then(m => m.CustomerReportsComponent),
    canActivate: [authGuard, roleGuard],
    data: { roles: ['Customer'] }
  },
  {
    path: 'loan-officer/customer-reports',
    loadComponent: () =>
      import('./components/loan-officer/customer-reports/customer-reports')
        .then(m => m.CustomerReportsComponent),
    canActivate: [authGuard, roleGuard],
    data: { roles: ['LoanOfficer'] }
  },

  {
    path: 'customer/emi/:loanId',
    loadComponent: () =>
      import('./components/customer/emi/emi')
        .then(m => m.EmiComponent),
    canActivate: [authGuard, roleGuard],
    data: { roles: ['Customer'] }
  },
  {
    path: 'loan-officer/loans',
    loadComponent: () =>
      import('./components/loan-officer/loans/loans')
        .then(m => m.LoansComponent),
    canActivate: [authGuard, roleGuard],
    data: { roles: ['LoanOfficer'] }
  },
  {
    path: 'loan-officer/emi/:loanId',
    loadComponent: () =>
      import('./components/loan-officer/emi/officer-emi')
        .then(m => m.OfficerEmiComponent),
    canActivate: [authGuard, roleGuard],
    data: { roles: ['LoanOfficer'] }
  },
  {
    path: 'reports',
    loadComponent: () =>
      import('./components/loan-officer/reports/reports').then(m => m.ReportsComponent),
    canActivate: [authGuard, roleGuard],
    data: { roles: ['LoanOfficer'] }
  },

  {
    path: 'reports',
    loadComponent: () =>
      import('./components/loan-officer/reports/reports').then(m => m.ReportsComponent),
    canActivate: [authGuard, roleGuard],
    data: { roles: ['LoanOfficer'] }
  },

  {
    path: '',
    loadComponent: () => import('./components/home/home').then(m => m.HomeComponent)
  },
  { path: '**', redirectTo: '' }
];

