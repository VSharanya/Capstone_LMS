import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';

// ----------------------------------------------------------------------------
// SERVICE: Loan Officer
// ----------------------------------------------------------------------------

@Injectable({
  providedIn: 'root'
})
export class LoanOfficerLoanService {

  private readonly baseUrl = environment.apiBaseUrl;

  constructor(private readonly http: HttpClient) { }

  getAllLoans() {
    return this.http.get<any[]>(`${this.baseUrl}/loans`);
  }
  getOfficerDashboardLoans() {
    return this.http.get<any[]>(
      `${this.baseUrl}/loans/officer/my-dashboard`
    );
  }
  updateLoanStatus(loanId: number, payload: any) {
    return this.http.put(
      `${this.baseUrl}/loans/${loanId}/decision`,
      payload
    );
  }

  // 🟡 MARK UNDER REVIEW
  markUnderReview(loanId: number) {
    return this.http.put(
      `${this.baseUrl}/loans/${loanId}/decision`,
      {
        status: 'Under Review'
      }
    );
  }

  // ✅ APPROVE LOAN
  approveLoan(loanId: number) {
    return this.http.put(
      `${this.baseUrl}/loans/${loanId}/decision`,
      {
        status: 'Approved'
      }
    );
  }

  // ❌ REJECT LOAN
  rejectLoan(loanId: number, remarks: string) {
    return this.http.put(
      `${this.baseUrl}/loans/${loanId}/decision`,
      {
        status: 'Rejected',
        remarks
      }
    );
  }

  getDocuments(loanId: number) {
    return this.http.get<any[]>(`${this.baseUrl}/documents/${loanId}`);
  }

  downloadDocument(layoutId: number) {
    return this.http.get(`${this.baseUrl}/documents/download/${layoutId}`, {
      responseType: 'blob'
    });
  }
}
