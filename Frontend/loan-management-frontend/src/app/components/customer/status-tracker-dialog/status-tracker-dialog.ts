import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { CustomerLoan } from '../../../models/customer-loan';

@Component({
    selector: 'app-status-tracker-dialog',
    standalone: true,
    imports: [CommonModule, MatDialogModule, MatIconModule, MatButtonModule],
    templateUrl: './status-tracker-dialog.html',
    styleUrls: ['./status-tracker-dialog.css']
})
export class StatusTrackerDialogComponent {

    steps = [
        { label: 'Applied', status: 'Applied', icon: 'description', date: '' },
        { label: 'Under Review', status: 'Under Review', icon: 'rate_review', date: '' },
        { label: 'Decision', status: 'Decision', icon: 'gavel', date: '' }, // Dynamic: Active or Rejected
        { label: 'Closed', status: 'Closed', icon: 'check_circle', date: '' }
    ];

    currentStepIndex = 0;
    isRejected = false;

    constructor(@Inject(MAT_DIALOG_DATA) public data: { loan: CustomerLoan }) {
        this.initializeSteps();
    }

    initializeSteps() {
        const loan = this.data.loan;

        // Step 1: Applied (Always filled)
        this.steps[0].date = loan.appliedDate;

        // Step 2: Under Review
        if (['Under Review', 'Verified', 'Approved', 'Active', 'Rejected', 'Closed'].includes(loan.status)) {
            this.steps[1].date = loan.verifiedDate || 'In Progress';
        }

        // Step 3: Decision (Active or Rejected)
        if (loan.status === 'Rejected') {
            this.isRejected = true;
            this.steps[2].label = 'Rejected';
            this.steps[2].icon = 'cancel';
            this.steps[2].status = 'Rejected';
            // If approvedDate is null but rejected, we might need a rejectedDate or just use updated logic
            // For now, if we don't have a rejectedDate, we assume it's done. 
            this.steps[2].date = 'Rejected';
        } else if (['Active', 'Closed', 'Approved'].includes(loan.status)) {
            this.steps[2].label = 'Active';
            this.steps[2].icon = 'account_balance_wallet';
            this.steps[2].status = 'Active';
            this.steps[2].date = loan.approvedDate || '';
        }

        // Step 4: Closed
        if (loan.status === 'Closed') {
            this.steps[3].date = 'Completed';
        }

        // Determine Active Step Index
        switch (loan.status) {
            case 'Applied': this.currentStepIndex = 0; break;
            case 'Under Review': this.currentStepIndex = 1; break;
            case 'Approved':
            case 'Active': this.currentStepIndex = 2; break;
            case 'Rejected': this.currentStepIndex = 2; break;
            case 'Closed': this.currentStepIndex = 3; break;
            default: this.currentStepIndex = 0;
        }
    }

    getStepClass(index: number): string {
        if (index < this.currentStepIndex) return 'completed';
        if (index === this.currentStepIndex) {
            return this.isRejected && index === 2 ? 'rejected' : 'active';
        }
        return 'pending';
    }
}
