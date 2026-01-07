import { Component, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';

import { CustomerLoanService } from '../../../../services/customer-loan-service';

@Component({
    selector: 'app-customer-loan-statement',
    standalone: true,
    imports: [
        CommonModule,
        MatTableModule,
        MatPaginatorModule,
        MatSortModule,
        MatButtonModule,
        MatIconModule,
        MatChipsModule,
        MatFormFieldModule,
        MatInputModule
    ],
    templateUrl: './loan-statement.html',
    styleUrls: ['./loan-statement.css']
})
export class CustomerLoanStatementComponent implements OnInit {
    displayedColumns: string[] = [
        'loanId',
        'type',
        'amount',
        'tenure',
        'paid',
        'outstanding',
        'status'
    ];
    dataSource = new MatTableDataSource<any>([]);

    @ViewChild(MatPaginator) paginator!: MatPaginator;
    @ViewChild(MatSort) sort!: MatSort;

    constructor(private service: CustomerLoanService) { }

    ngOnInit() {
        this.service.getMyLoans().subscribe(loans => {
            // Newest first
            this.dataSource.data = loans.sort((a, b) => b.loanId - a.loanId);
            this.dataSource.paginator = this.paginator;
            this.dataSource.sort = this.sort;

            // Custom Sorting to handle column name mismatches
            this.dataSource.sortingDataAccessor = (item, property) => {
                switch (property) {
                    case 'type': return item.loanType;
                    case 'amount': return item.loanAmount;
                    case 'tenure': return item.tenureMonths;
                    case 'paid': return item.totalPaid || 0;
                    case 'outstanding': return item.outstandingAmount || 0;
                    default: return item[property];
                }
            };

            // Custom filter predicate to search all fields
            this.dataSource.filterPredicate = (data, filter) => {
                const dataStr = Object.keys(data).reduce((currentTerm, key) => {
                    return currentTerm + (data[key] + '◬');
                }, '').toLowerCase();
                return dataStr.indexOf(filter) !== -1;
            };
        });
    }

    applyFilter(event: Event) {
        const filterValue = (event.target as HTMLInputElement).value;
        this.dataSource.filter = filterValue.trim().toLowerCase();

        if (this.dataSource.paginator) {
            this.dataSource.paginator.firstPage();
        }
    }

    getStatusColor(status: string): string {
        switch (status?.toLowerCase()) {
            case 'active': return 'primary';
            case 'approved': return 'accent';
            case 'closed': return 'warn';
            default: return 'primary';
        }
    }

    printReport() {
        window.print();
    }
}
