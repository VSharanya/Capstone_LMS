import { Router } from '@angular/router';
import { Component, OnInit, ViewChild, AfterViewInit, signal, effect } from '@angular/core';
import { CommonModule } from '@angular/common';

import { MatTableModule, MatTableDataSource } from '@angular/material/table';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatIconModule } from '@angular/material/icon';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';

import { CustomerLoanService } from '../../../services/customer-loan-service';
import { CustomerLoan } from '../../../models/customer-loan';
import { StatusTrackerDialogComponent } from '../status-tracker-dialog/status-tracker-dialog';
import { UploadDocumentDialogComponent } from '../upload-document-dialog/upload-document-dialog';


@Component({
  selector: 'app-my-loans',
  standalone: true,
  imports: [
    CommonModule,
    MatTableModule,
    MatPaginatorModule,
    MatSortModule,
    MatInputModule,
    MatSelectModule,
    MatSnackBarModule,
    MatIconModule,
    MatDialogModule
  ],
  templateUrl: './my-loans.html',
  styleUrls: ['./my-loans.css']
})
export class MyLoansComponent implements OnInit, AfterViewInit {

  loans = signal<CustomerLoan[]>([]); // All loans
  displayedColumns = [
    'loanType',
    'loanAmount',
    'tenureMonths',
    'status',
    'appliedDate',
    'actions',
    'remarks'
  ];

  dataSource = new MatTableDataSource<CustomerLoan>([]);
  selectedStatus = 'Active'; // ✅ Default to Active

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  loading = true;

  constructor(
    private service: CustomerLoanService,
    private router: Router,
    private snack: MatSnackBar,
    private dialog: MatDialog
  ) {
    // 🔁 Signal → Table sync (with Filter)
    effect(() => {
      const all = this.loans();

      // ✅ Filter logic
      const filtered = this.selectedStatus === 'All'
        ? all
        : all.filter(l => l.status === this.selectedStatus);

      this.dataSource.data = filtered;

      if (this.paginator) this.dataSource.paginator = this.paginator;
      if (this.sort) this.dataSource.sort = this.sort;
    });
  }

  ngOnInit() {
    this.loadLoans();
  }

  ngAfterViewInit() {
    this.dataSource.paginator = this.paginator;
    this.dataSource.sort = this.sort;

    // 🔍 Global Search Filter
    this.dataSource.filterPredicate = (data, filter) => {
      const value = filter.trim().toLowerCase();
      const type = data.loanType.toLowerCase();
      const status = data.status.toLowerCase();
      const amount = data.loanAmount.toString();
      const remarks = (data.remarks || '').toLowerCase();

      return type.includes(value) || status.includes(value) || amount.includes(value) || remarks.includes(value);
    };

    // 🔽 Custom Sorting
    this.dataSource.sortingDataAccessor = (item, property) => {
      switch (property) {
        case 'loanType': return item.loanType;
        case 'loanAmount': return item.loanAmount;
        case 'tenureMonths': return item.tenureMonths;
        case 'status': return item.status;
        case 'appliedDate': return item.appliedDate;
        case 'remarks': return item.remarks || '';
        default: return '';
      }
    };
  }

  loadLoans() {
    this.service.getMyLoans().subscribe({
      next: res => {
        // Newest loans first
        const sorted = res.sort((a, b) => b.loanId - a.loanId);
        this.loans.set(sorted);
        this.loading = false;
      },
      error: () => {
        this.snack.open('Failed to load loans', 'Close', { duration: 3000 });
        this.loading = false;
      }
    });
  }

  viewEmi(loanId: number) {
    // For now just navigate
    this.router.navigate(['/customer/emi', loanId]);
  }

  openTracker(loan: CustomerLoan) {
    this.dialog.open(StatusTrackerDialogComponent, {
      width: '600px',
      data: { loan },
      panelClass: 'tracker-dialog'
    });
  }

  openUploadDialog(loanId: number) {
    const ref = this.dialog.open(UploadDocumentDialogComponent, {
      width: '400px',
      data: { loanId }
    });

    ref.afterClosed().subscribe(result => {
      if (result) {
        // Optional: Maybe show a success message or refresh something
      }
    });
  }

  applyFilter(event: Event) {
    const value = (event.target as HTMLInputElement).value;
    this.dataSource.filter = value.trim().toLowerCase();
    this.dataSource.paginator?.firstPage();
  }

  // ✅ Status Filter Change
  onStatusChange(newStatus: string) {
    this.selectedStatus = newStatus;
    this.updateTableData();
  }

  updateTableData() {
    const all = this.loans();
    const filtered = this.selectedStatus === 'All'
      ? all
      : all.filter(l => l.status === this.selectedStatus);

    this.dataSource.data = filtered;

    if (this.paginator) this.dataSource.paginator = this.paginator;
    if (this.sort) this.dataSource.sort = this.sort;
  }

  getStatusClass(status: string): string {
    switch (status) {
      case 'Approved':
      case 'Active':
        return 'status-approved';
      case 'Rejected':
        return 'status-rejected';
      case 'Closed':
        return 'status-closed';
      case 'Under Review':
        return 'status-under-review'; // Added
      default:
        return 'status-pending'; // Applied
    }
  }
}