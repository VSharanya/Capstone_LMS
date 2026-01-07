export interface CustomerLoan {
  loanId: number;
  customerName: string;
  loanType: string;
  loanAmount: number;
  tenureMonths: number;
  status: string;
  appliedDate: string;
  remarks?: string;
  totalPaid: number;
  outstandingAmount: number;
  hasDocuments: boolean;
  verifiedDate?: string;
  approvedDate?: string;
}