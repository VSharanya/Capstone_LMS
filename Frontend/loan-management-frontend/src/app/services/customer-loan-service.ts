import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

import { environment } from '../../environments/environment';
import { LoanType } from '../models/loan-type';
import { Emi } from '../models/emi';

// ----------------------------------------------------------------------------
// SERVICE: Customer Loan
// ----------------------------------------------------------------------------

@Injectable({
  providedIn: 'root'
})
export class CustomerLoanService {

  private readonly baseUrl = environment.apiBaseUrl;

  constructor(private readonly http: HttpClient) { }

  // Get active loan types
  getActiveLoanTypes(): Observable<LoanType[]> {
    return this.http.get<LoanType[]>(
      `${this.baseUrl}/loan-types`
    );
  }

  // Apply loan
  applyLoan(payload: any): Observable<any> {
    return this.http.post(
      `${this.baseUrl}/loans/apply`,
      payload
    );
  }

  // Get customer loans
  getMyLoans(): Observable<any[]> {
    return this.http.get<any[]>(
      `${this.baseUrl}/loans/my`
    );
  }

  // Get EMI schedule
  getEmiSchedule(): Observable<any[]> {
    return this.http.get<any[]>(
      `${this.baseUrl}/emis/my`
    );
  }

  getEmisByLoan(loanId: number) {
    return this.http.get<Emi[]>(`${this.baseUrl}/emis/loan/${loanId}`);
  }

  payEmi(emiId: number, paymentMode: string) {
    return this.http.put(`${this.baseUrl}/emis/${emiId}/pay`, { paymentMode });
  }

  payFullOutstanding(loanId: number, paymentMode: string) {
    return this.http.put(`${this.baseUrl}/emis/loan/${loanId}/pay-full`, { paymentMode });
  }

  getForeclosureAmount(loanId: number): Observable<any> {
    return this.http.get<any>(`${this.baseUrl}/emis/loan/${loanId}/foreclosure`);
  }

  getPaymentHistory(loanId: number): Observable<any[]> {
    return this.http.get<any[]>(`${this.baseUrl}/emis/loan/${loanId}/payments`);
  }

  uploadDocument(formData: FormData): Observable<any> {
    return this.http.post(`${this.baseUrl}/documents/upload`, formData);
  }
}