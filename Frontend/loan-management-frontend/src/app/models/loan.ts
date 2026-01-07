export interface LoanApplication {
    loanId: number;
    customerName: string;
    customerEmail: string;
    customerPhone: string;
    loanType: string;
    loanAmount: number;
    status: string;
    appliedDate: string; // ISO Date String
    updatedOn?: string;
    remarks?: string;
    interestRate?: number;
    tenureMonths?: number;
}
