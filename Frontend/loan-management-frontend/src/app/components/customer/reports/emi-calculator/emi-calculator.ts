
import { Component, computed, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatSliderModule } from '@angular/material/slider';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';

@Component({
    selector: 'app-emi-calculator',
    standalone: true,
    imports: [
        CommonModule,
        FormsModule,
        MatSliderModule,
        MatInputModule,
        MatFormFieldModule,
        MatCardModule,
        MatButtonModule
    ],
    templateUrl: './emi-calculator.html',
    styleUrls: ['./emi-calculator.css']
})
export class EmiCalculatorComponent {

    // Signals for Inputs
    principal = signal(500000);
    rate = signal(10.5);
    tenure = signal(24); // months

    // Computed EMI
    emi = computed(() => {
        const p = this.principal();
        const r = this.rate() / 12 / 100;
        const n = this.tenure();

        if (p === 0 || r === 0) return 0;

        const emi = (p * r * Math.pow(1 + r, n)) / (Math.pow(1 + r, n) - 1);
        return Math.round(emi);
    });

    // Computed Totals
    totalPayment = computed(() => this.emi() * this.tenure());
    totalInterest = computed(() => this.totalPayment() - this.principal());

    // Pie Chart Data (CSS Conic Gradient)
    pieStyle = computed(() => {
        const total = this.totalPayment();
        const pPercent = (this.principal() / total) * 100;
        return `conic-gradient(#3b82f6 0% ${pPercent}%, #f59e0b ${pPercent}% 100%)`;
    });

    // Helper to format values
    formatLabel(value: number): string {
        if (value >= 1000) return Math.round(value / 1000) + 'k';
        return `${value}`;
    }
}
