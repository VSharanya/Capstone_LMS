import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import { User } from '../models/user';

// ----------------------------------------------------------------------------
// SERVICE: Admin User Management
// ----------------------------------------------------------------------------

@Injectable({ providedIn: 'root' })
export class AdminUserService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = environment.apiBaseUrl;

  getUsers() {
    return this.http.get<User[]>(`${this.baseUrl}/users`);
  }

  updateUserStatus(id: number, isActive: boolean) {
    return this.http.put(
      `${this.baseUrl}/users/${id}/status`,
      { isActive }
    );
  }

  createUser(payload: any) {
    return this.http.post(`${this.baseUrl}/users`, payload);
  }
}
