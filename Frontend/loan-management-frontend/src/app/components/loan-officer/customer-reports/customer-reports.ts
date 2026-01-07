import { Component, OnInit, ViewChild, AfterViewInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableModule, MatTableDataSource } from '@angular/material/table';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { ReportsService } from '../../../services/reports-service';
import { CustomerLoanSummary } from '../../../models/reports';

@Component({
    selector: 'app-customer-reports',
    standalone: true,
    imports: [
        CommonModule,
        MatTableModule,
        MatPaginatorModule,
        MatSortModule,
        MatInputModule,
        MatIconModule,
        MatButtonModule,
        MatCardModule
    ],
    templateUrl: './customer-reports.html',
    styleUrls: ['./customer-reports.css']
})
export class CustomerReportsComponent implements OnInit, AfterViewInit {

    displayedColumns: string[] = ['customerId', 'customerName', 'email', 'totalLoans', 'activeLoans', 'totalLoanAmount', 'totalOutstanding'];
    dataSource = new MatTableDataSource<CustomerLoanSummary>([]);

    @ViewChild(MatPaginator) paginator!: MatPaginator;
    @ViewChild(MatSort) sort!: MatSort;

    constructor(private reportService: ReportsService) { }

    ngOnInit() {
        this.loadData();
    }

    ngAfterViewInit() {
        this.dataSource.paginator = this.paginator;
        this.dataSource.sort = this.sort;

        // Custom Sorting
        this.dataSource.sortingDataAccessor = (item, property) => {
            switch (property) {
                case 'customerId': return item.customerId;
                case 'customerName': return item.customerName.toLowerCase();
                case 'email': return item.email.toLowerCase();
                case 'totalLoans': return item.totalLoans;
                case 'activeLoans': return item.activeLoans;
                case 'totalLoanAmount': return item.totalLoanAmount;
                case 'totalOutstanding': return item.totalOutstanding;
                default: return (item as any)[property];
            }
        };

        // Custom Filtering
        this.dataSource.filterPredicate = (data: CustomerLoanSummary, filter: string) => {
            const value = filter.trim().toLowerCase();
            return data.customerId.toString().includes(value) ||
                data.customerName.toLowerCase().includes(value) ||
                data.email.toLowerCase().includes(value) ||
                data.totalLoans.toString().includes(value) ||
                data.activeLoans.toString().includes(value) ||
                data.totalLoanAmount.toString().includes(value) ||
                data.totalOutstanding.toString().includes(value);
        };
    }

    loadData() {
        this.reportService.getCustomerLoanSummaries().subscribe({
            next: (data: CustomerLoanSummary[]) => {
                this.dataSource.data = data;
                // Paginator and Sort are already linked in AfterViewInit
            },
            error: (err: any) => console.error('Error loading report', err)
        });
    }

    applyFilter(event: Event) {
        const filterValue = (event.target as HTMLInputElement).value;
        this.dataSource.filter = filterValue.trim().toLowerCase();

        if (this.dataSource.paginator) {
            this.dataSource.paginator.firstPage();
        }
    }
}
