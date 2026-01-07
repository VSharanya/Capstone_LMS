import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { LoanType } from '../models/loan-type';
import { environment } from '../../environments/environment';

// ----------------------------------------------------------------------------
// SERVICE: Admin Loan Type
// ----------------------------------------------------------------------------

@Injectable({
  providedIn: 'root'
})
export class AdminLoanTypeService {

  private readonly baseUrl = `${environment.apiBaseUrl}/loan-types`;

  constructor(private readonly http: HttpClient) { }

  getAll(): Observable<LoanType[]> {
    return this.http.get<LoanType[]>(`${this.baseUrl}/all`);
  }

  create(data: any) {
    return this.http.post(this.baseUrl, data);
  }

  update(id: number, data: any) {
    return this.http.put(`${this.baseUrl}/${id}`, data);
  }
}