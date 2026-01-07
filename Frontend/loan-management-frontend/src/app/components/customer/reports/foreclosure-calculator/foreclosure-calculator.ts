
import { Component, computed, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatSliderModule } from '@angular/material/slider';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';

@Component({
    selector: 'app-foreclosure-calculator',
    standalone: true,
    imports: [
        CommonModule,
        FormsModule,
        MatSliderModule,
        MatInputModule,
        MatIconModule,
        MatCardModule
    ],
    templateUrl: './foreclosure-calculator.html',
    styleUrls: ['./foreclosure-calculator.css']
})
export class ForeclosureCalculatorComponent {

    // Inputs matching the backend logic requirements
    loanAmount = signal(500000);
    tenure = signal(24);
    interestRate = signal(10.5);
    emisPaid = signal(12);

    // 1. Calculate the Fixed EMI Amount (Standard Formula)
    monthlyEmi = computed(() => {
        const p = this.loanAmount();
        const r = this.interestRate() / 12 / 100;
        const n = this.tenure();

        if (p === 0 || r === 0) return 0;

        // EMI = [P x R x (1+R)^N] / [(1+R)^N - 1]
        const emi = (p * r * Math.pow(1 + r, n)) / (Math.pow(1 + r, n) - 1);
        return emi;
    });

    // 2. Exact Logic from Backend (Re-run amortization)
    amortizationSnapshot = computed(() => {
        let balance = this.loanAmount();
        const annualRate = this.interestRate();
        const monthlyRate = annualRate / 12 / 100;
        const emiAmount = this.monthlyEmi();
        const paidCount = this.emisPaid();

        let totalPrincipalPaid = 0;
        let totalInterestPaid = 0;

        // "foreach (var emi in paidEmis)" simulation
        for (let i = 0; i < paidCount; i++) {
            const interestComponent = balance * monthlyRate;
            const principalComponent = emiAmount - interestComponent;

            balance -= principalComponent;

            totalPrincipalPaid += principalComponent;
            totalInterestPaid += interestComponent;
        }

        // Ensure we don't go below 0 if they slid the slider too far
        balance = Math.max(0, balance);

        return {
            outstandingBalance: balance, // This is the Foreclosure Amount
            principalPaid: totalPrincipalPaid,
            interestPaid: totalInterestPaid
        };
    });

    // Expose values for UI
    outstandingBalance = computed(() => this.amortizationSnapshot().outstandingBalance);
    principalPaidSoFar = computed(() => this.amortizationSnapshot().principalPaid);

    // 3. Foreclosure Amount = Remaining Principal (as per User requirement)
    foreclosureAmount = computed(() => this.outstandingBalance());

    // 4. Future Stats
    remainingEmisCount = computed(() => Math.max(0, this.tenure() - this.emisPaid()));

    totalCostContinued = computed(() => {
        return this.remainingEmisCount() * this.monthlyEmi();
    });

    savings = computed(() => {
        return this.totalCostContinued() - this.foreclosureAmount();
    });

    isProfitable = computed(() => this.savings() > 0);

    formatLabel(value: number): string {
        if (value >= 1000) return Math.round(value / 1000) + 'k';
        return `${value}`;
    }
}
