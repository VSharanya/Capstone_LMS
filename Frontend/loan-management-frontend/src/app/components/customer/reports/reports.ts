
import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTabsModule } from '@angular/material/tabs';
import { MatCardModule } from '@angular/material/card';

// Sub-components
import { CustomerLoanStatementComponent } from './loan-statement/loan-statement';
import { EmiCalculatorComponent } from './emi-calculator/emi-calculator';
import { ForeclosureCalculatorComponent } from './foreclosure-calculator/foreclosure-calculator';

@Component({
    selector: 'app-customer-reports',
    standalone: true,
    imports: [
        CommonModule,
        MatTabsModule,
        MatCardModule,
        CustomerLoanStatementComponent,
        EmiCalculatorComponent,
        ForeclosureCalculatorComponent
    ],
    templateUrl: './reports.html',
    styleUrls: ['./reports.css']
})
export class CustomerReportsComponent { }
