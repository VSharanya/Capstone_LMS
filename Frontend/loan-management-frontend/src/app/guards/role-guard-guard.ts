import { CanActivateFn, Router, ActivatedRouteSnapshot } from '@angular/router';
import { inject } from '@angular/core';
import { AuthService } from '../services/auth-service';

export const roleGuard: CanActivateFn = (route: ActivatedRouteSnapshot) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  const allowedRoles = (route.data['roles'] as string[])
    .map(role => role.toLowerCase());   

  const userRole = authService.getUserRole()?.toLowerCase();

  if (userRole && allowedRoles.includes(userRole)) {
    return true;
  }

  // Role not allowed → redirect safely
  if (userRole === 'admin') {
    router.navigate(['/admin/dashboard']);
  } else if (userRole === 'loanofficer') {
    router.navigate(['/loan-officer/dashboard']);
  } else if (userRole === 'customer') {
    router.navigate(['/customer/dashboard']);
  } else {
    router.navigate(['/auth/login']);
  }

  return false;
};
