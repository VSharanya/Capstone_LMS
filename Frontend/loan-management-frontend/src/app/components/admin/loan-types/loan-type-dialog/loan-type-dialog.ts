import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef, MatDialogModule } from '@angular/material/dialog';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';

import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatButtonModule } from '@angular/material/button';
import { MatDividerModule } from '@angular/material/divider';


import { AdminLoanTypeService } from '../../../../services/admin-loan-type-service';
import { LoanType } from '../../../../models/loan-type';

@Component({
  selector: 'app-loan-type-dialog',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatSlideToggleModule,
    MatButtonModule,
    MatDividerModule,
    MatSnackBarModule
  ],
  templateUrl: './loan-type-dialog.html',
  styleUrls: ['./loan-type-dialog.css']
})
export class LoanTypeDialogComponent {

  isEdit = false;
  form!: FormGroup;
  saving = false;

  constructor(
    private readonly fb: FormBuilder,
    private readonly service: AdminLoanTypeService,
    private readonly dialogRef: MatDialogRef<LoanTypeDialogComponent>,
    private readonly snackBar: MatSnackBar,
    @Inject(MAT_DIALOG_DATA) public data: LoanType | null
  ) {
    this.form = this.fb.group({
      loanTypeName: ['', Validators.required],
      interestRate: [0, [Validators.required, Validators.min(0.1)]],
      minAmount: [0, [Validators.required, Validators.min(1)]],
      maxAmount: [0, [Validators.required, Validators.min(1)]],
      maxTenureMonths: [0, [Validators.required, Validators.min(1)]],
      hasMoratorium: [false],
      isActive: [true]
    });

    if (data) {
      this.isEdit = true;
      this.form.patchValue(data);
    }
  }

  save() {
    if (this.form.invalid || this.saving) return;

    this.saving = true;
    const payload = this.form.getRawValue();

    const request = (this.isEdit && this.data)
      ? this.service.update(this.data.loanTypeId, payload)
      : this.service.create(payload);

    request.subscribe({
      next: () => {
        this.snackBar.open(this.isEdit ? 'Loan Type Updated' : 'Loan Type Created', 'Close', { duration: 3000 });
        this.dialogRef.close(true);
      },
      error: (err) => {
        console.error(err);
        this.snackBar.open('Failed to save. Please check inputs.', 'Close', { duration: 3000 });
        this.saving = false;
      }
    });
  }

  cancel() {
    this.dialogRef.close(false);
  }
}