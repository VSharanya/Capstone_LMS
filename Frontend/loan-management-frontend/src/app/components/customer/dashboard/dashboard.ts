import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

import { AuthService } from '../../../services/auth-service';
import { CustomerLoanService } from '../../../services/customer-loan-service';


@Component({
  selector: 'app-customer-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule
  ],
  templateUrl: './dashboard.html',
  styleUrls: ['./dashboard.css']
})
export class DashboardComponent implements OnInit {
  userName = signal('User');
  totalApplied = signal(0);
  activeLoans = signal(0);
  totalDebt = signal(0);
  totalPaid = signal(0);

  constructor(
    private auth: AuthService,
    private loanService: CustomerLoanService
  ) { }

  ngOnInit() {
    this.userName.set(this.auth.getUserName());
    this.loadStats();
  }

  loadStats() {
    this.loanService.getMyLoans().subscribe({
      next: (loans) => {
        this.totalApplied.set(loans.length);

        // Filter for active loans (case-insensitive)
        const active = loans.filter(l =>
          l.status && (l.status.toLowerCase() === 'active' || l.status.toLowerCase() === 'approved')
        );

        this.activeLoans.set(active.length);
        this.totalDebt.set(active.reduce((sum, l) => sum + (l.outstandingAmount || 0), 0));
        this.totalPaid.set(active.reduce((sum, l) => sum + (l.totalPaid || 0), 0));
      },
      error: (err) => {
        console.error('Failed to load loans', err);
      }
    });
  }
}