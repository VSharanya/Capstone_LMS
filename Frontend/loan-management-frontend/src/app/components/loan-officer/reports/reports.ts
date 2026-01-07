import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTabsModule } from '@angular/material/tabs';

import { EmiOverdueComponent } from './emi-overdue/emi-overdue';
import { MonthlyEmiComponent } from './monthly-emi/monthly-emi';
import { OutstandingComponent } from './outstanding/outstanding';
import { OfficerLoansByStatusComponent } from './loans-by-status/loans-by-status';
import { OfficerActiveClosedLoansComponent } from './active-closed-loans/active-closed-loans';
import { CustomerReportsComponent } from '../customer-reports/customer-reports';

@Component({
  selector: 'app-loan-officer-reports',
  standalone: true,
  imports: [
    CommonModule,
    MatTabsModule,
    EmiOverdueComponent,
    MonthlyEmiComponent,
    OutstandingComponent,
    OfficerLoansByStatusComponent,
    OfficerActiveClosedLoansComponent,
    CustomerReportsComponent
  ],
  templateUrl: './reports.html'
})
export class ReportsComponent { }