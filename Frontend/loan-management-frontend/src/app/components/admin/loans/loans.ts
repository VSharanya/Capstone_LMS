import { Component, OnInit, ViewChild, signal, computed, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { MatButtonModule } from '@angular/material/button';
import { MatSelectModule } from '@angular/material/select';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatInputModule } from '@angular/material/input';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatChipsModule } from '@angular/material/chips';
import { Router, RouterModule } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatIconModule } from '@angular/material/icon';
import { FormsModule } from '@angular/forms';

import { LoanOfficerLoanService } from '../../../services/loan-officer-loan-service';
import { LoanDetailsDialogComponent } from '../../shared/loan-details-dialog/loan-details-dialog';
import { LoanApplication } from '../../../models/loan';

@Component({
    selector: 'app-admin-loans',
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
        MatChipsModule,
        RouterModule,
        MatCardModule,
        MatTooltipModule,
        MatIconModule,
        FormsModule // Required for ngModel binding used in Search
    ],
    templateUrl: './loans.html',
    styleUrls: ['./loans.css']
})
export class AdminLoansComponent implements OnInit {

    displayedColumns = [
        'loanId',
        'customer',
        'loanType',
        'amount',
        'status',
        'appliedOn',
        'actions'
    ];

    // Signals for State
    allLoans = signal<LoanApplication[]>([]);
    selectedStatus = signal<string>('All');
    searchFilter = signal<string>('');

    // MatTableDataSource source of truth
    dataSource = new MatTableDataSource<LoanApplication>([]);

    @ViewChild(MatPaginator) paginator!: MatPaginator;
    @ViewChild(MatSort) sort!: MatSort;

    // Computed Filter (Reactive Logic)
    filteredLoans = computed(() => {
        const status = this.selectedStatus();
        const search = this.searchFilter().toLowerCase();
        const loans = this.allLoans();

        return loans.filter(loan => {
            const matchesStatus = status === 'All' || loan.status === status;
            const matchesSearch = !search ||
                loan.customerName.toLowerCase().includes(search) ||
                loan.loanType.toLowerCase().includes(search) ||
                String(loan.loanId).includes(search);

            return matchesStatus && matchesSearch;
        });
    });

    constructor(
        private service: LoanOfficerLoanService,
        private snack: MatSnackBar,
        private dialog: MatDialog
    ) {
        // Effect to update DataSource whenever Filtered Loans change
        effect(() => {
            this.dataSource.data = this.filteredLoans();

            // Re-assign paginator/sort after data update to ensure they bind correctly
            if (this.paginator) this.dataSource.paginator = this.paginator;
            if (this.sort) this.dataSource.sort = this.sort;
        });

        // Custom Sorting Logic for Mapped Columns
        this.dataSource.sortingDataAccessor = (item, property) => {
            switch (property) {
                case 'customer': return item.customerName.toLowerCase();
                case 'amount': return item.loanAmount;
                case 'appliedOn': return new Date(item.appliedDate).getTime();
                default: return (item as any)[property];
            }
        };
    }

    ngOnInit() {
        this.loadLoans();
    }

    loadLoans() {
        this.service.getAllLoans().subscribe({
            next: (loans: LoanApplication[]) => {
                this.allLoans.set(loans);
            },
            error: (err: any) => {
                console.error('Error fetching loans', err);
                this.snack.open('Failed to load loans', 'Close', { duration: 3000 });
            }
        });
    }

    // Event Handlers update signals
    onStatusChange(status: string) {
        this.selectedStatus.set(status);
    }

    onSearch(event: Event) {
        const input = (event.target as HTMLInputElement).value;
        this.searchFilter.set(input);
    }

    viewDetails(loan: LoanApplication) {
        this.dialog.open(LoanDetailsDialogComponent, {
            width: '600px',
            data: { loan, readOnly: true }
        });
    }

    getStatusClass(status: string): string {
        return status.toLowerCase().replace(' ', '-');
    }
}

