import { Component, OnInit, ViewChild, AfterViewInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';

import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { MatButtonModule } from '@angular/material/button';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTabsModule } from '@angular/material/tabs';

import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { ConfirmationDialogComponent } from '../../shared/confirmation-dialog/confirmation-dialog';
import { PaymentDialogComponent } from '../payment-dialog/payment-dialog';

import { CustomerLoanService } from '../../../services/customer-loan-service';
import { Emi } from '../../../models/emi';

@Component({
  selector: 'app-emi',
  standalone: true,
  imports: [
    CommonModule,
    MatTableModule,
    MatPaginatorModule,
    MatSortModule,
    MatButtonModule,
    MatSnackBarModule,
    MatInputModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatTabsModule
  ],
  templateUrl: './emi.html',
  styleUrls: ['./emi.css']
})
export class EmiComponent implements OnInit, AfterViewInit {

  loanId!: number;
  loading = true;

  displayedColumns = [
    'installment',
    'dueDate',
    'amount',
    'status',
    'action'
  ];

  dataSource = new MatTableDataSource<Emi>([]);

  // ViewChildren for EMI Tab
  @ViewChild('emiPaginator') emiPaginator!: MatPaginator;
  @ViewChild('emiSort') emiSort!: MatSort;

  // History Data
  historyDataSource = new MatTableDataSource<any>([]);
  historyColumns = ['id', 'installment', 'amount', 'date', 'mode'];

  // ViewChildren for History Tab
  @ViewChild('historyPaginator') historyPaginator!: MatPaginator;
  @ViewChild('historySort') historySort!: MatSort;

  payingId: number | null = null;
  payingFull = false;

  constructor(
    private route: ActivatedRoute,
    private service: CustomerLoanService,
    private snack: MatSnackBar,
    private dialog: MatDialog
  ) { }

  ngOnInit() {
    this.loanId = Number(this.route.snapshot.paramMap.get('loanId'));
    this.loadEmis();

    this.setupEmiFilter();
    this.setupHistoryFilter();
  }

  ngAfterViewInit() {
    // EMI Table Config
    this.dataSource.paginator = this.emiPaginator;
    this.dataSource.sort = this.emiSort;
    this.setupEmiSorting();

    // History Table Config
    this.historyDataSource.paginator = this.historyPaginator;
    this.historyDataSource.sort = this.historySort;
    this.setupHistorySorting();
  }

  // --- EMI SETUP ---
  setupEmiFilter() {
    this.dataSource.filterPredicate = (data: Emi, filter: string) => {
      const value = filter.trim().toLowerCase();
      const date = new Date(data.dueDate).toLocaleDateString().toLowerCase();

      return (
        data.installmentNumber.toString().includes(value) ||
        data.emiAmount.toString().includes(value) ||
        (data.isPaid ? 'paid' : 'unpaid').includes(value) ||
        date.includes(value)
      );
    };
  }

  setupEmiSorting() {
    this.dataSource.sortingDataAccessor = (item, property) => {
      switch (property) {
        case 'installment': return item.installmentNumber;
        case 'amount': return Number(item.emiAmount);
        case 'status': return item.isPaid ? 'Paid' : 'Unpaid';
        case 'dueDate': return new Date(item.dueDate).getTime();
        default: return '';
      }
    };
  }

  // --- HISTORY SETUP ---
  setupHistoryFilter() {
    this.historyDataSource.filterPredicate = (data: any, filter: string) => {
      const value = filter.trim().toLowerCase();
      const date = new Date(data.paymentDate).toLocaleDateString().toLowerCase();

      return (
        data.paymentId.toString().includes(value) ||
        data.installmentNumber.toString().includes(value) ||
        data.paidAmount.toString().includes(value) ||
        data.paymentMode.toLowerCase().includes(value) ||
        date.includes(value)
      );
    };
  }

  setupHistorySorting() {
    this.historyDataSource.sortingDataAccessor = (item, property) => {
      switch (property) {
        case 'id': return item.paymentId;
        case 'installment': return item.installmentNumber;
        case 'amount': return Number(item.paidAmount);
        case 'date': return new Date(item.paymentDate).getTime();
        case 'mode': return item.paymentMode;
        default: return '';
      }
    };
  }

  loadEmis() {
    this.service.getEmisByLoan(this.loanId).subscribe({
      next: res => {
        this.dataSource.data = res;
        this.dataSource.paginator = this.emiPaginator;
        this.dataSource.sort = this.emiSort;
        this.loading = false;
      },
      error: () => {
        this.snack.open('Failed to load EMI schedule', 'Close', { duration: 3000 });
        this.loading = false;
      }
    });

    this.loadHistory();
  }

  loadHistory() {
    this.service.getPaymentHistory(this.loanId).subscribe({
      next: res => {
        this.historyDataSource.data = res;
        this.historyDataSource.paginator = this.historyPaginator;
        this.historyDataSource.sort = this.historySort;
      }
    });
  }

  applyEmiFilter(event: Event) {
    const value = (event.target as HTMLInputElement).value;
    this.dataSource.filter = value.trim().toLowerCase();

    if (this.dataSource.paginator) {
      this.dataSource.paginator.firstPage();
    }
  }

  applyHistoryFilter(event: Event) {
    const value = (event.target as HTMLInputElement).value;
    this.historyDataSource.filter = value.trim().toLowerCase();

    if (this.historyDataSource.paginator) {
      this.historyDataSource.paginator.firstPage();
    }
  }

  isLoanClosed(): boolean {
    return this.dataSource.data.length > 0 &&
      this.dataSource.data.every(e => e.isPaid);
  }

  canPay(emi: Emi) {
    return !emi.isPaid;
  }

  canShowForeclosure() {
    return this.dataSource.data.some(e => !e.isPaid);
  }

  payEmi(emi: Emi) {
    this.payingId = emi.emiId;

    const dialogRef = this.dialog.open(PaymentDialogComponent, {
      width: '400px',
      data: {
        amount: emi.emiAmount,
        type: 'EMI',
        title: 'Pay EMI',
        subtitle: 'EMI Amount Due'
      },
      panelClass: 'premium-dialog'
    });
    // Force Rebuild 123

    dialogRef.afterClosed().subscribe(result => {
      if (result && result.paid) {
        // Proceed with payment
        this.service.payEmi(emi.emiId, result.mode).subscribe({
          next: () => {
            this.snack.open(`Payment of ₹${result.amount} via ${result.mode} successful!`, 'Close', { duration: 3000 });
            this.loadEmis();
            this.payingId = null;
          },
          error: err => {
            this.snack.open(err?.error || 'Failed to pay EMI', 'Close', { duration: 4000 });
            this.payingId = null;
          }
        });
      } else {
        this.payingId = null; // Reset if cancelled
      }
    });
  }

  payFullOutstanding() {
    this.payingFull = true;

    // 1. Get Amount
    this.service.getForeclosureAmount(this.loanId).subscribe({
      next: (res) => {
        const amount = res.foreclosureAmount;

        const dialogRef = this.dialog.open(PaymentDialogComponent, {
          width: '400px',
          data: {
            amount: amount,
            type: 'FORECLOSURE',
            title: 'Close Loan',
            subtitle: 'Outstanding Amount'
          },
          panelClass: 'premium-dialog'
        });

        dialogRef.afterClosed().subscribe(result => {
          if (result && result.paid) {
            this.proceedForeclosure(result.mode);
          } else {
            this.payingFull = false;
          }
        });
      },
      error: () => {
        this.snack.open('Failed to calculate foreclosure amount', 'Close', { duration: 3000 });
        this.payingFull = false;
      }
    });

  }

  proceedForeclosure(mode: string) {
    this.service.payFullOutstanding(this.loanId, mode).subscribe({
      next: () => {
        this.snack.open('Loan closed successfully.', 'Close', { duration: 4000 });
        this.loadEmis();
        this.payingFull = false;
      },
      error: err => {
        this.snack.open(err?.error || 'Failed to close loan', 'Close', { duration: 4000 });
        this.payingFull = false;
      }
    });
  }
}