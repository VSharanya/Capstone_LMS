import { Component, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { MatButtonModule } from '@angular/material/button';
import { MatSelectModule } from '@angular/material/select';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { Router, RouterModule } from '@angular/router';

import { LoanOfficerLoanService } from '../../../services/loan-officer-loan-service';
import { RejectDialogComponent } from '../reject-dialog/reject-dialog/reject-dialog';
import { ViewDocumentsDialogComponent } from '../view-documents-dialog/view-documents-dialog';

@Component({
  selector: 'app-loan-applications',
  standalone: true,
  imports: [
    CommonModule,
    MatTableModule,
    MatPaginatorModule,
    MatSortModule,
    MatButtonModule,
    MatSelectModule,
    MatFormFieldModule,
    MatSnackBarModule,
    MatDialogModule,
    MatInputModule,
    RouterModule,
    MatIconModule
  ],
  templateUrl: './loans.html',
  styleUrls: ['./loans.css']
})
export class LoansComponent implements OnInit {

  displayedColumns = [
    'loanId',
    'customer',
    'annualIncome',
    'loanType',
    'amount',
    'tenure',
    'status',
    'appliedOn',
    'actions'
  ];

  dataSource = new MatTableDataSource<any>([]);
  allLoans: any[] = [];

  selectedStatus = 'Applied';

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  constructor(
    private service: LoanOfficerLoanService,
    private snack: MatSnackBar,
    private dialog: MatDialog,
    private router: Router
  ) { }

  ngOnInit() {
    this.service.getAllLoans().subscribe(loans => {
      // Sort newest first
      this.allLoans = loans.sort((a: any, b: any) => b.loanId - a.loanId);
      this.applyStatusFilter();
    });
  }

  applyStatusFilter() {
    this.dataSource.data = this.allLoans.filter(
      loan => loan.status === this.selectedStatus
    );

    this.dataSource.paginator = this.paginator;
    this.dataSource.sort = this.sort;

    this.dataSource.sortingDataAccessor = (item, property) => {
      switch (property) {
        case 'loanId': return item.loanId;
        case 'customer': return item.customerName;
        case 'annualIncome': return item.annualIncome;
        case 'amount': return item.loanAmount;
        case 'tenure': return item.tenureMonths;
        case 'appliedOn': return new Date(item.appliedDate).getTime();
        default: return item[property];
      }
    };

    this.dataSource.filterPredicate = (data, filter) => {
      const value = filter.trim().toLowerCase();
      const date = new Date(data.appliedDate).toLocaleDateString().toLowerCase();

      return data.loanId.toString().includes(value) ||
        (data.customerName || '').toLowerCase().includes(value) ||
        (data.loanType || '').toLowerCase().includes(value) ||
        (data.status || '').toLowerCase().includes(value) ||
        data.loanAmount.toString().includes(value) ||
        date.includes(value);
    };
  }

  onStatusChange(status: string) {
    this.selectedStatus = status;
    this.applyStatusFilter();
  }

  applyFilter(event: Event) {
    const filterValue = (event.target as HTMLInputElement).value;
    this.dataSource.filter = filterValue.trim().toLowerCase();

    if (this.dataSource.paginator) {
      this.dataSource.paginator.firstPage();
    }
  }

  viewDocuments(loanId: number) {
    this.dialog.open(ViewDocumentsDialogComponent, {
      width: '500px',
      data: { loanId }
    });
  }

  verifyLoan(loanId: number) {
    this.service.markUnderReview(loanId).subscribe({
      next: () => {
        this.snack.open('Loan Verified (moved to Under Review)', 'Close', { duration: 3000 });
        this.ngOnInit();
      },
      error: err => {
        const errorMsg = err.error?.error || err.error || 'Failed to verify loan';
        this.snack.open(errorMsg, 'Close', { duration: 4000 });
      }
    });
  }

  approve(loanId: number) {
    this.service.approveLoan(loanId).subscribe({
      next: () => {
        this.snack.open('Loan approved successfully', 'Close', { duration: 3000 });
        this.ngOnInit();
      },
      error: err => {
        const errorMsg = err.error?.error || err.error || 'Failed to approve loan';
        this.snack.open(errorMsg, 'Close', { duration: 4000 });
      }
    });
  }

  reject(loanId: number) {
    const dialogRef = this.dialog.open(RejectDialogComponent, { width: '400px' });

    dialogRef.afterClosed().subscribe((remarks: string | undefined) => {
      if (!remarks) return;

      this.service.rejectLoan(loanId, remarks).subscribe({
        next: () => {
          this.snack.open('Loan rejected successfully', 'Close', { duration: 3000 });
          this.ngOnInit();
        },
        error: err => {
          const errorMsg = err.error?.error || err.error || 'Failed to reject loan';
          this.snack.open(errorMsg, 'Close', { duration: 4000 });
        }
      });
    });
  }

  viewEMI(loanId: number) {
    this.router.navigate(['/loan-officer/emi', loanId]);
  }
}