import { Component, OnInit, ViewChild, signal, computed, effect, AfterViewInit } from '@angular/core';
import { CommonModule } from '@angular/common';

import { MatTableModule, MatTableDataSource } from '@angular/material/table';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatButtonModule } from '@angular/material/button';
import { MatInputModule } from '@angular/material/input';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatCardModule } from '@angular/material/card';

import { LoanType } from '../../../models/loan-type';
import { AdminLoanTypeService } from '../../../services/admin-loan-type-service';
import { LoanTypeDialogComponent } from './loan-type-dialog/loan-type-dialog';

@Component({
  selector: 'app-admin-loan-types',
  standalone: true,
  imports: [
    CommonModule,
    MatTableModule,
    MatPaginatorModule,
    MatSortModule,
    MatDialogModule,
    MatSnackBarModule,
    MatButtonModule,
    MatInputModule,
    MatSlideToggleModule,
    MatIconModule,
    MatFormFieldModule,
    MatCardModule
  ],
  templateUrl: './loan-types.html',
  styleUrls: ['./loan-types.css']
})
export class AdminLoanTypesComponent implements OnInit, AfterViewInit {

  // Signals for State
  loanTypes = signal<LoanType[]>([]);
  searchQuery = signal<string>('');

  dataSource = new MatTableDataSource<LoanType>([]);

  displayedColumns = [
    'name',
    'interest',
    'amount',
    'tenure',
    'moratorium',
    'status',
    'createdOn',
    'updatedOn',
    'actions'
  ];

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  // Computed Filtered Data
  filteredLoanTypes = computed(() => {
    const query = this.searchQuery().toLowerCase();
    const allTypes = this.loanTypes();

    if (!query) return allTypes;

    return allTypes.filter(type => {
      // Helper to safely stringify values
      const matches = (val: any) => String(val).toLowerCase().includes(query);
      const dateMatch = (date: any) => date ? new Date(date).toDateString().toLowerCase().includes(query) : false;

      return (
        matches(type.loanTypeName) ||
        matches(type.interestRate) ||
        matches(type.minAmount) ||
        matches(type.maxAmount) ||
        matches(type.maxTenureMonths) ||
        (query === 'yes' && type.hasMoratorium) ||
        (query === 'no' && !type.hasMoratorium) ||
        dateMatch(type.createdOn) ||
        dateMatch(type.updatedOn)
      );
    });
  });

  constructor(
    private readonly service: AdminLoanTypeService,
    private readonly dialog: MatDialog,
    private readonly snack: MatSnackBar
  ) {
    // Sync computed data -> table
    effect(() => {
      this.dataSource.data = this.filteredLoanTypes();
    });
  }

  ngOnInit() {
    this.loadLoanTypes();

    // Custom sorting
    this.dataSource.sortingDataAccessor = (item, property) => {
      switch (property) {
        case 'name': return item.loanTypeName?.toLowerCase() ?? '';
        case 'interest': return item.interestRate;
        case 'amount': return item.maxAmount; // Sort by Max Amount
        case 'tenure': return Number(item.maxTenureMonths);
        case 'moratorium': return item.hasMoratorium ? 1 : 0;
        case 'createdOn': return item.createdOn ? new Date(item.createdOn).getTime() : 0;
        case 'updatedOn': return item.updatedOn ? new Date(item.updatedOn).getTime() : 0;
        default: return '';
      }
    };
  }

  ngAfterViewInit() {
    this.dataSource.paginator = this.paginator;
    this.dataSource.sort = this.sort;
  }

  loadLoanTypes() {
    this.service.getAll().subscribe(res => this.loanTypes.set(res));
  }

  onSearch(event: Event) {
    const value = (event.target as HTMLInputElement).value;
    this.searchQuery.set(value);
  }

  openDialog(loanType?: LoanType) {
    const dialogRef = this.dialog.open(LoanTypeDialogComponent, {
      width: '480px',
      panelClass: 'premium-dialog',
      data: loanType ?? null
    });

    dialogRef.afterClosed().subscribe(done => {
      if (done) {
        this.loadLoanTypes();
        this.snack.open('Loan type saved successfully', 'Close', { duration: 3000 });
      }
    });
  }

  toggleStatus(loanType: LoanType) {
    const newStatus = !loanType.isActive;
    const updatePayload = {
      interestRate: loanType.interestRate,
      minAmount: loanType.minAmount,
      maxAmount: loanType.maxAmount,
      maxTenureMonths: loanType.maxTenureMonths,
      hasMoratorium: loanType.hasMoratorium,
      isActive: newStatus
    };

    this.service.update(loanType.loanTypeId, updatePayload).subscribe({
      next: () => {
        this.snack.open('Loan type status updated', 'Close', { duration: 3000 });
        this.loadLoanTypes();
      },
      error: (err) => {
        this.snack.open('Failed to update status', 'Close', { duration: 3000 });
        // Revert toggle if failed
        // Note: For full robustness, we should fetch list again or undo local change
        loanType.isActive = !newStatus;
      }
    });
  }
}