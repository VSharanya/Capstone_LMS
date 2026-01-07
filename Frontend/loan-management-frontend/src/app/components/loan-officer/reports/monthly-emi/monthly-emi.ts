import { Component, signal, ViewChild, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableModule, MatTableDataSource } from '@angular/material/table';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatInputModule } from '@angular/material/input';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { FormsModule } from '@angular/forms';
import { ReportsService } from '../../../../services/reports-service';
import { MonthlyEmiReport } from '../../../../models/reports';

@Component({
  selector: 'app-monthly-emi',
  standalone: true,
  imports: [
    CommonModule,
    MatTableModule,
    MatSelectModule,
    MatButtonModule,
    FormsModule,
    MatInputModule,
    MatPaginatorModule,
    MatSortModule,
    MatIconModule,
    MatCardModule,
    MatFormFieldModule
  ],
  templateUrl: './monthly-emi.html',
  styleUrls: ['./monthly-emi.css']
})
export class MonthlyEmiComponent {

  data = signal<MonthlyEmiReport[]>([]);
  stats = signal({ paid: 0, pending: 0, total: 0, paidPct: 0 });

  dataSource = new MatTableDataSource<MonthlyEmiReport>([]);
  displayedColumns = ['loanId', 'customer', 'dueDate', 'amount', 'status'];

  @ViewChild(MatPaginator) set paginator(paginator: MatPaginator) {
    if (paginator) this.dataSource.paginator = paginator;
  }

  @ViewChild(MatSort) set sort(sort: MatSort) {
    if (sort) this.dataSource.sort = sort;
  }

  months = [
    { name: 'January', value: 1 }, { name: 'February', value: 2 },
    { name: 'March', value: 3 }, { name: 'April', value: 4 },
    { name: 'May', value: 5 }, { name: 'June', value: 6 },
    { name: 'July', value: 7 }, { name: 'August', value: 8 },
    { name: 'September', value: 9 }, { name: 'October', value: 10 },
    { name: 'November', value: 11 }, { name: 'December', value: 12 }
  ];

  years = [2024, 2025, 2026];

  selectedMonth = new Date().getMonth() + 1;
  selectedYear = new Date().getFullYear();
  searched = false;

  constructor(private service: ReportsService) {
    effect(() => {
      this.dataSource.data = this.data();
      this.calculateStats();
    });
  }

  calculateStats() {
    const items = this.data();
    if (!items.length) {
      this.stats.set({ paid: 0, pending: 0, total: 0, paidPct: 0 });
      return;
    }

    const paid = items.filter(i => i.isPaid).reduce((acc, curr) => acc + curr.emiAmount, 0);
    const pending = items.filter(i => !i.isPaid).reduce((acc, curr) => acc + curr.emiAmount, 0);
    const total = paid + pending;
    const paidPct = total > 0 ? (paid / total) * 100 : 0;

    this.stats.set({ paid, pending, total, paidPct });
  }

  ngOnInit() {
    // Custom Filter Predicate
    this.dataSource.filterPredicate = (data: MonthlyEmiReport, filter: string) => {
      const query = filter.toLowerCase();
      const matches = (val: any) => String(val).toLowerCase().includes(query);
      const dateMatch = (date: string) => new Date(date).toDateString().toLowerCase().includes(query);
      const statusMatch = (isPaid: boolean) => (isPaid ? 'paid' : 'pending').includes(query);

      return (
        matches(data.loanId) ||
        matches(data.customerName) ||
        matches(data.emiAmount) ||
        dateMatch(data.dueDate) ||
        statusMatch(data.isPaid)
      );
    };

    this.dataSource.sortingDataAccessor = (item, property) => {
      switch (property) {
        case 'customer': return item.customerName;
        case 'amount': return item.emiAmount;
        case 'status': return item.isPaid ? 'Paid' : 'Pending';
        case 'dueDate': return new Date(item.dueDate).getTime();
        default: return (item as any)[property];
      }
    };
  }

  fetchReport() {
    this.searched = true;
    this.service.getMonthlyEmi(this.selectedMonth, this.selectedYear)
      .subscribe({
        next: (res) => this.data.set(res),
        error: (err) => {
          console.error(err);
          this.data.set([]);
        }
      });
  }

  applyFilter(event: Event) {
    const filterValue = (event.target as HTMLInputElement).value;
    this.dataSource.filter = filterValue.trim().toLowerCase();

    if (this.dataSource.paginator) {
      this.dataSource.paginator.firstPage();
    }
  }

  getActionMonthName() {
    return this.months.find(m => m.value == this.selectedMonth)?.name || 'Selected Month';
  }
}
