export interface LoanType {
  loanTypeId: number;
  loanTypeName: string;
  interestRate: number;
  minAmount: number;
  maxAmount: number;
  maxTenureMonths: number;
  hasMoratorium: boolean;
  moratoriumMonths?: number;
  isActive: boolean;
  createdBy: string;
  createdOn: string;
  updatedBy?: string;
  updatedOn?: string;
}