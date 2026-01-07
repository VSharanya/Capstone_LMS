export interface Emi {
  emiId: number;
  installmentNumber: number;
  dueDate: string;
  emiAmount: number;
  isPaid: boolean;
  paidDate?: string | null;
}