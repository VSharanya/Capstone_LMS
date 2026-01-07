import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';

// ----------------------------------------------------------------------------
// SERVICE: Reports
// ----------------------------------------------------------------------------

@Injectable({ providedIn: 'root' })
export class ReportsService {

  private baseUrl = `${environment.apiBaseUrl}/reports`;

  constructor(private http: HttpClient) { }

  getLoansByStatus() {
    return this.http.get<any[]>(`${this.baseUrl}/loans-by-status`);
  }

  getEmiOverdue() {
    return this.http.get<any[]>(`${this.baseUrl}/emi-overdue`);
  }

  getMonthlyEmi(month: number, year: number) {
    return this.http.get<any[]>(`${this.baseUrl}/monthly-emi`, {
      params: { Month: month, Year: year }
    });
  }

  getOutstanding(loanId: number) {
    return this.http.get<any>(`${this.baseUrl}/outstanding/${loanId}`);
  }

  getCustomerLoanSummaries() {
    return this.http.get<import('../models/reports').CustomerLoanSummary[]>(`${this.baseUrl}/customer-summary`);
  }
}