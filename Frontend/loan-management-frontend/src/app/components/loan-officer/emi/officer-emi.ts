import { Component, OnInit, ViewChild, AfterViewInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';

import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { MatInputModule } from '@angular/material/input';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';

import { CustomerLoanService } from '../../../services/customer-loan-service';
import { Emi } from '../../../models/emi';

@Component({
  selector: 'app-officer-emi',
  standalone: true,
  imports: [
    CommonModule,
    MatTableModule,
    MatPaginatorModule,
    MatSortModule,
    MatInputModule,
    MatSnackBarModule,
    MatCardModule,
    MatIconModule,
    MatFormFieldModule
  ],
  templateUrl: './officer-emi.html',
  styleUrls: ['./officer-emi.css']
})
export class OfficerEmiComponent implements OnInit, AfterViewInit {

  loanId!: number;
  loading = true;

  displayedColumns = [
    'installment',
    'dueDate',
    'amount',
    'status',
    'paidDate'
  ];

  dataSource = new MatTableDataSource<Emi>([]);

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  constructor(
    private readonly route: ActivatedRoute,
    private readonly service: CustomerLoanService,
    private readonly snack: MatSnackBar
  ) { }

  ngOnInit() {
    this.loanId = Number(this.route.snapshot.paramMap.get('loanId'));
    this.loadEmis();
  }

  ngAfterViewInit() {
    this.dataSource.paginator = this.paginator;
    this.dataSource.sort = this.sort;

    this.dataSource.sortingDataAccessor = (item, property) => {
      switch (property) {
        case 'installment': return item.installmentNumber;
        case 'amount': return item.emiAmount;
        case 'status': return item.isPaid ? 'Paid' : 'Pending';
        case 'dueDate': return new Date(item.dueDate).getTime();
        case 'paidDate': return item.paidDate ? new Date(item.paidDate).getTime() : 0;
        default: return '';
      }
    };

    this.dataSource.filterPredicate = (data, filter) => {
      const value = filter.trim().toLowerCase();
      const status = data.isPaid ? 'paid' : 'pending';
      const date = new Date(data.dueDate).toLocaleDateString().toLowerCase();
      const amount = data.emiAmount.toString();

      return data.installmentNumber.toString().includes(value) ||
        status.includes(value) ||
        date.includes(value) ||
        amount.includes(value);
    };
  }

  loadEmis() {
    this.service.getEmisByLoan(this.loanId).subscribe({
      next: res => {
        this.dataSource.data = res;
        this.loading = false;
      },
      error: () => {
        this.snack.open('Failed to load EMI schedule', 'Close', { duration: 3000 });
        this.loading = false;
      }
    });
  }

  applyFilter(event: Event) {
    const value = (event.target as HTMLInputElement).value;
    this.dataSource.filter = value.trim().toLowerCase();
  }
}