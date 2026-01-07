import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatSelectModule } from '@angular/material/select';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { FormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';

@Component({
    selector: 'app-payment-dialog',
    standalone: true,
    imports: [
        CommonModule,
        MatDialogModule,
        MatButtonModule,
        MatSelectModule,
        MatInputModule,
        MatFormFieldModule,
        FormsModule,
        MatIconModule
    ],
    templateUrl: './payment-dialog.html',
    styleUrls: ['./payment-dialog.css']
})
export class PaymentDialogComponent {

    selectedMode = 'UPI';
    enteredAmount: number;

    constructor(
        public dialogRef: MatDialogRef<PaymentDialogComponent>,
        @Inject(MAT_DIALOG_DATA) public data: {
            amount: number,
            type?: 'EMI' | 'FORECLOSURE',
            title?: string,
            subtitle?: string
        }
    ) {
        this.enteredAmount = data.amount;
    }

    isValid() {
        return this.selectedMode && this.enteredAmount > 0;
    }

    confirmPay() {
        this.dialogRef.close({ paid: true, mode: this.selectedMode, amount: this.enteredAmount });
    }
}
