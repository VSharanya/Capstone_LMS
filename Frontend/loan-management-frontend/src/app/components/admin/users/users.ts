import { Component, OnInit, ViewChild, signal, computed, effect, AfterViewInit } from '@angular/core';
import { CommonModule } from '@angular/common';

import { MatTableModule, MatTableDataSource } from '@angular/material/table';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatTooltipModule } from '@angular/material/tooltip';

import { ConfirmationDialogComponent } from '../../shared/confirmation-dialog/confirmation-dialog';
import { CreateUserDialogComponent } from './create-user-dialog/create-user-dialog';
import { AdminUserService } from '../../../services/admin-user-service';
import { AuthService } from '../../../services/auth-service';
import { User } from '../../../models/user';

@Component({
  selector: 'app-admin-users',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatChipsModule,
    MatTooltipModule,
    MatTableModule,
    MatSlideToggleModule,
    MatSnackBarModule,
    MatDialogModule,
    MatPaginatorModule,
    MatInputModule,
    MatButtonModule,
    MatSortModule,
    MatFormFieldModule,
    MatIconModule
  ],
  templateUrl: './users.html',
  styleUrls: ['./users.css']
})
export class AdminUsersComponent implements OnInit, AfterViewInit {

  // Signals for State
  users = signal<User[]>([]);
  searchQuery = signal<string>('');
  currentUserId: number | null = null;

  // MatTable DataSource
  dataSource = new MatTableDataSource<User>([]);
  displayedColumns = ['name', 'role', 'income', 'status'];

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  // Computed Filtered Users
  filteredUsers = computed(() => {
    const query = this.searchQuery().toLowerCase();
    const allUsers = this.users();

    if (!query) return allUsers;

    return allUsers.filter(u =>
      u.fullName.toLowerCase().includes(query) ||
      u.email.toLowerCase().includes(query) ||
      u.role.toLowerCase().includes(query) ||
      String(u.annualIncome).includes(query)
    );
  });

  constructor(
    private readonly userService: AdminUserService,
    private readonly snackBar: MatSnackBar,
    private readonly dialog: MatDialog,
    private readonly authService: AuthService
  ) {
    this.currentUserId = this.authService.getUserId();

    //  Sync computed filtered data → table datasource
    effect(() => {
      this.dataSource.data = this.filteredUsers();
    });
  }

  ngOnInit() {
    this.loadUsers();

    // Custom sorting accessor
    this.dataSource.sortingDataAccessor = (item: User, property: string) => {
      switch (property) {
        case 'name': return item.fullName.toLowerCase();
        case 'email': return item.email.toLowerCase();
        case 'role': {
          const roleOrder: Record<string, number> = { 'Admin': 1, 'LoanOfficer': 2, 'Customer': 3 };
          return roleOrder[item.role] ?? 99;
        }
        case 'income': return item.annualIncome;
        default: return '';
      }
    };
  }

  ngAfterViewInit() {
    this.dataSource.paginator = this.paginator;
    this.dataSource.sort = this.sort;

    if (!this.paginator) console.error('Paginator not found in ViewChild');
  }

  loadUsers() {
    this.userService.getUsers().subscribe({
      next: (res) => this.users.set(res),
      error: (err) => this.showSnack('Failed to load users', 'error')
    });
  }

  // Signal-based filtering
  onSearch(event: Event) {
    const value = (event.target as HTMLInputElement).value;
    this.searchQuery.set(value);
  }

  showSnack(message: string, type: 'success' | 'error' = 'success') {
    this.snackBar.open(message, 'Close', {
      duration: 3000,
      panelClass: type === 'success' ? 'snack-success' : 'snack-error'
    });
  }

  openCreateDialog() {
    console.log('Opening Create User Dialog...');
    const dialogRef = this.dialog.open(CreateUserDialogComponent, {
      width: '420px',
      panelClass: 'premium-dialog'
    });

    dialogRef.afterClosed().subscribe(created => {
      if (created) {
        this.loadUsers();
      }
    });
  }

  exportToCsv() {
    const data = this.filteredUsers().map(u => ({
      Name: u.fullName,
      Email: u.email,
      Role: u.role,
      AnnualIncome: u.annualIncome,
      Status: u.isActive ? 'Active' : 'Inactive'
    }));

    if (data.length === 0) {
      this.showSnack('No users to export', 'error');
      return;
    }

    const headers = Object.keys(data[0]);
    const csvContent = [
      headers.join(','), // Header row
      ...data.map(row => headers.map(fieldName => {
        return JSON.stringify((row as any)[fieldName] || ''); // Handle commas and quotes
      }).join(','))
    ].join('\n');

    const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' });
    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.setAttribute('href', url);
    link.setAttribute('download', `users_export_${new Date().getTime()}.csv`);
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
  }

  toggleStatus(user: User) {
    // If user is currently active, we are deactivating them -> Ask for confirmation
    if (user.isActive) {
      const dialogRef = this.dialog.open(ConfirmationDialogComponent, {
        width: '400px',
        panelClass: 'premium-dialog',
        data: {
          title: 'Deactivate User?',
          message: `Are you sure you want to deactivate ${user.fullName}? They will lose access to the system.`,
          confirmText: 'Deactivate',
          confirmColor: 'warn'
        }
      });

      dialogRef.afterClosed().subscribe(confirmed => {
        if (confirmed) {
          this.performStatusUpdate(user, false);
        } else {
          // Revert the toggle in UI (since it might have visually toggled already if using (change))
          // Actually, since we are binding [checked]="user.isActive", calling loadUsers() or 
          // essentially doing nothing if we preventDefault in template is enough. 
          // If the toggle already flipped, we simply force reload/update signals to reset it.
          // A quick hack is to just re-set the signal to trigger CD.
          this.users.update(users => [...users]);
        }
      });
    } else {
      // Activating -> No confirmation needed (or add if preferred)
      this.performStatusUpdate(user, true);
    }
  }

  private performStatusUpdate(user: User, newStatus: boolean) {
    this.userService.updateUserStatus(user.userId, newStatus)
      .subscribe({
        next: (res: any) => {
          this.showSnack(res?.message || 'User status updated');
          this.loadUsers();
        },
        error: (err: any) => {
          const message = err?.error?.message || 'Action not allowed';
          this.showSnack(message, 'error');
          this.loadUsers(); // Revert on error
        }
      });
  }
}
