import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { RouterModule } from '@angular/router';

import { LoanOfficerLoanService } from '../../../services/loan-officer-loan-service';
import { AuthService } from '../../../services/auth-service';


@Component({
  selector: 'app-loan-officer-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    RouterModule,
    MatIconModule
  ],
  templateUrl: './dashboard.html',
  styleUrls: ['./dashboard.css']
})
export class DashboardComponent implements OnInit {

  // ✅ Signals
  userName = signal('Loan Officer'); // Added default
  total = signal(0);
  applied = signal(0);
  underReview = signal(0);
  active = signal(0);
  rejected = signal(0);
  //closed = signal(0);
  loading = signal(true);

  constructor(
    private service: LoanOfficerLoanService,
    private auth: AuthService // Injected
  ) { }

  ngOnInit() {
    this.userName.set(this.auth.getUserName()); // Set name
    this.loadStats();
  }

  loadStats() {
    this.service.getOfficerDashboardLoans().subscribe({
      next: loans => {
        this.total.set(loans.length);

        this.applied.set(
          loans.filter(l => l.status === 'Applied').length
        );

        this.underReview.set(
          loans.filter(l => l.status === 'Under Review').length
        );

        this.active.set(
          loans.filter(l => l.status === 'Active').length
        );

        this.rejected.set(
          loans.filter(l => l.status === 'Rejected').length
        );

        // this.closed.set(
        //   loans.filter(l => l.status === 'Closed').length
        // );

        this.loading.set(false);
      },
      error: err => {
        console.error('Dashboard API error:', err);
        this.loading.set(false);
      }
    });
  }
}