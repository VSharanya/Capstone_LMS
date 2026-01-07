import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { finalize } from 'rxjs/operators';

import { CustomerLoanService } from '../../../services/customer-loan-service';
import { LoanType } from '../../../models/loan-type';


@Component({
  selector: 'app-apply-loan',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatSelectModule,
    MatInputModule,
    MatButtonModule,
    MatSnackBarModule,
    MatCardModule,
    MatIconModule
  ],
  templateUrl: './apply-loan.html',
  styleUrls: ['./apply-loan.css']
})
export class ApplyLoanComponent implements OnInit {

  form!: FormGroup;
  loanTypes: LoanType[] = [];
  selectedLoanType: LoanType | null = null;
  submitting = false;

  constructor(
    private readonly fb: FormBuilder,
    private readonly service: CustomerLoanService,
    private readonly snack: MatSnackBar,
    private readonly router: Router
  ) { }

  ngOnInit() {
    this.form = this.fb.group({
      loanTypeId: [null, Validators.required],
      loanAmount: [null, Validators.required],
      tenureMonths: [null, Validators.required],
      moratoriumMonths: [{ value: null, disabled: true }]
    });

    this.loadLoanTypes();

    // Listen for changes to calculate EMI
    this.form.get('loanAmount')?.valueChanges.subscribe(() => this.calculateEmi());
    this.form.get('tenureMonths')?.valueChanges.subscribe(() => this.calculateEmi());
  }

  loadLoanTypes() {
    this.service.getActiveLoanTypes().subscribe(res => {
      this.loanTypes = res;
    });
  }

  // ✅ EMI Calculation State
  monthlyEmi = 0;
  totalRepayment = 0;

  onLoanTypeChange(loanTypeId: number) {
    this.selectedLoanType =
      this.loanTypes.find(l => l.loanTypeId === loanTypeId) ?? null;

    const amountCtrl = this.form.get('loanAmount');
    const tenureCtrl = this.form.get('tenureMonths');
    const moratoriumCtrl = this.form.get('moratoriumMonths');

    if (this.selectedLoanType) {
      // ✅ Amount validators (LIVE blocking)
      amountCtrl?.setValidators([
        Validators.required,
        Validators.min(this.selectedLoanType.minAmount),
        Validators.max(this.selectedLoanType.maxAmount)
      ]);

      // ✅ Tenure validators
      tenureCtrl?.setValidators([
        Validators.required,
        Validators.min(1),
        Validators.max(this.selectedLoanType.maxTenureMonths)
      ]);

      // ✅ Moratorium logic
      if (this.selectedLoanType.hasMoratorium) {
        moratoriumCtrl?.enable();
        moratoriumCtrl?.setValidators([Validators.required, Validators.min(1)]);
      } else {
        moratoriumCtrl?.reset();
        moratoriumCtrl?.disable();
        moratoriumCtrl?.clearValidators();
      }
    }

    amountCtrl?.updateValueAndValidity();
    tenureCtrl?.updateValueAndValidity();
    moratoriumCtrl?.updateValueAndValidity();

    this.calculateEmi();
  }

  // Calculate EMI whenever values change
  calculateEmi() {
    if (!this.selectedLoanType) {
      this.monthlyEmi = 0;
      this.totalRepayment = 0;
      return;
    }

    const P = this.form.get('loanAmount')?.value;
    const n = this.form.get('tenureMonths')?.value;
    const r = this.selectedLoanType.interestRate / 12 / 100; // Monthly Interest Rate

    if (P && n && r) {
      // EMI = P * r * (1 + r)^n / ((1 + r)^n - 1)
      const emi = (P * r * Math.pow(1 + r, n)) / (Math.pow(1 + r, n) - 1);
      this.monthlyEmi = emi;
      this.totalRepayment = emi * n;
    } else {
      this.monthlyEmi = 0;
      this.totalRepayment = 0;
    }
  }



  // ✅ Document Upload State
  createdLoanId: number | null = null;
  selectedFile: File | null = null;
  uploadingDoc = false;

  submit() {
    this.form.markAllAsTouched();

    if (this.form.invalid || this.submitting) return;

    this.submitting = true;

    this.service.applyLoan(this.form.getRawValue())
      .pipe(
        finalize(() => {
          this.submitting = false;
        })
      )
      .subscribe({
        next: (res: any) => {
          this.snack.open('Loan applied successfully! Please upload documents.', 'Close', {
            duration: 3000
          });

          // ✅ Set Created Loan ID and Show Upload UI
          this.createdLoanId = res.loanId;

          // Disable form to prevent changes
          this.form.disable();
        },
        error: err => {
          let message = 'Failed to apply loan';

          if (typeof err?.error === 'string') {
            message = err.error;
          } else if (err?.error?.message) {
            message = err.error.message;
          }

          this.snack.open(message, 'Close', {
            duration: 4000
          });
        }
      });
  }

  // ✅ File Selection
  onFileSelected(event: any) {
    const file = event.target.files[0];
    if (file) {
      this.selectedFile = file;
    }
  }

  // ✅ Upload Document
  uploadDocument() {
    if (!this.selectedFile || !this.createdLoanId) return;

    this.uploadingDoc = true;
    const formData = new FormData();
    formData.append('loanApplicationId', this.createdLoanId.toString());
    formData.append('documentType', 'IdentityProof'); // Default for now, or add dropdown
    formData.append('file', this.selectedFile);

    this.service.uploadDocument(formData)
      .pipe(finalize(() => this.uploadingDoc = false))
      .subscribe({
        next: () => {
          this.snack.open('Document uploaded successfully!', 'Close', { duration: 3000 });
          this.router.navigate(['/customer/my-loans']);
        },
        error: err => {
          console.error(err);
          this.snack.open('Failed to upload document.', 'Close', { duration: 3000 });
        }
      });
  }

  resetForm() {
    // ✅ FULL RESET
    this.form.enable();
    this.form.reset();

    // Clear dynamic validators
    this.form.get('loanAmount')?.clearValidators();
    this.form.get('tenureMonths')?.clearValidators();
    this.form.get('moratoriumMonths')?.clearValidators();

    // Disable moratorium again
    this.form.get('moratoriumMonths')?.disable();

    // Reset visual state
    this.createdLoanId = null;
    this.selectedFile = null;
    this.selectedLoanType = null;

    this.form.markAsPristine();
    this.form.markAsUntouched();
  }

  onUploadLater() {
    this.snack.open('You can upload documents anytime from "My Loans".', 'OK', { duration: 4000 });
    this.router.navigate(['/customer/my-loans']);
  }
}