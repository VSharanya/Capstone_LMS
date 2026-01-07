export interface OutstandingReport {
    loanId: number;
    outstandingAmount: number;
}

export interface MonthlyEmiReport {
    loanId: number;
    customerName: string;
    loanType: string;
    dueDate: string;
    emiAmount: number;
    isPaid: boolean;
}

export interface CustomerLoanSummary {
    customerId: number;
    customerName: string;
    email: string;
    totalLoans: number;
    activeLoans: number;
    totalLoanAmount: number;
    totalOutstanding: number;
}
