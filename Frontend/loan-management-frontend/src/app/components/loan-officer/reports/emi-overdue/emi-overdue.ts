import { Component, OnInit, ViewChild, AfterViewInit } from '@angular/core'; // Removed signal
import { CommonModule } from '@angular/common';
import { MatTableModule, MatTableDataSource } from '@angular/material/table';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { ReportsService } from '../../../../services/reports-service';

@Component({
  selector: 'app-emi-overdue',
  standalone: true,
  imports: [
    CommonModule,
    MatTableModule,
    MatCardModule,
    MatIconModule,
    MatPaginatorModule,
    MatSortModule,
    MatFormFieldModule,
    MatInputModule
  ],
  templateUrl: './emi-overdue.html',
  styleUrls: ['./emi-overdue.css']
})

export class EmiOverdueComponent implements OnInit, AfterViewInit {
  displayedColumns = ['loanId', 'overdue', 'count'];
  dataSource = new MatTableDataSource<any>([]);

  @ViewChild(MatPaginator) set matPaginator(mp: MatPaginator) {
    this.paginator = mp;
    this.dataSource.paginator = this.paginator;
  }

  @ViewChild(MatSort) set matSort(ms: MatSort) {
    this.sort = ms;
    this.dataSource.sort = this.sort;
  }

  paginator!: MatPaginator;
  sort!: MatSort;

  constructor(private service: ReportsService) { }

  ngOnInit() {
    this.service.getEmiOverdue().subscribe((res: any[]) => {
      // Aggregate data by LoanId
      const grouped = res.reduce((acc, curr) => {
        const id = curr.loanId;
        if (!acc[id]) {
          acc[id] = { loanId: id, amount: 0, count: 0 };
        }
        acc[id].amount += curr.emiAmount;
        acc[id].count++;
        return acc;
      }, {} as Record<number, any>);

      this.dataSource.data = Object.values(grouped);

      // Custom Sorting Accessor
      this.dataSource.sortingDataAccessor = (item, property) => {
        switch (property) {
          case 'overdue': return item.amount;
          case 'loanId': return item.loanId;
          case 'count': return item.count;
          default: return item[property];
        }
      };

      // Custom Filter
      this.dataSource.filterPredicate = (data, filter) => {
        const dataStr = (data.loanId + ' ' + data.amount + ' ' + data.count).toLowerCase();
        return dataStr.indexOf(filter) !== -1;
      };
    });
  }

  ngAfterViewInit() { }

  applyFilter(event: Event) {
    const filterValue = (event.target as HTMLInputElement).value;
    this.dataSource.filter = filterValue.trim().toLowerCase();

    if (this.dataSource.paginator) {
      this.dataSource.paginator.firstPage();
    }
  }
}
