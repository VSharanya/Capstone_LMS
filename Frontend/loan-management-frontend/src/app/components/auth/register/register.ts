
import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';

import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatStepperModule } from '@angular/material/stepper';

import { AuthService } from '../../../services/auth-service';
import { RegisterRequest } from '../../../models/auth';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatSnackBarModule,
    MatStepperModule
  ],
  templateUrl: './register.html',
  styleUrls: ['./register.css']
})

export class RegisterComponent {

  isLoading = false;
  personalDetailsForm!: FormGroup;
  financialDetailsForm!: FormGroup;

  constructor(
    private readonly fb: FormBuilder,
    private readonly authService: AuthService,
    private readonly router: Router,
    private readonly snackBar: MatSnackBar
  ) {
    this.personalDetailsForm = this.fb.group({
      fullName: ['', [Validators.required, Validators.maxLength(100)]],
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]],
      confirmPassword: ['', Validators.required]
    }, { validators: this.passwordMatchValidator });

    this.financialDetailsForm = this.fb.group({
      annualIncome: ['', [Validators.required, Validators.min(1)]]
    });
  }

  // Validator to check match
  passwordMatchValidator(g: FormGroup) {
    return g.get('password')?.value === g.get('confirmPassword')?.value
      ? null : { mismatch: true };
  }

  onSubmit(): void {
    if (this.personalDetailsForm.invalid || this.financialDetailsForm.invalid) {
      this.snackBar.open('Please fill all required fields correctly', 'Close', {
        duration: 3000
      });
      return;
    }

    const payload: RegisterRequest = {
      ...this.personalDetailsForm.value,
      ...this.financialDetailsForm.value,
      role: 'Customer'
    } as RegisterRequest;

    this.isLoading = true;

    this.authService.register(payload).subscribe({
      next: () => {
        this.snackBar.open(
          'Registration successful. Please login.',
          'Close',
          { duration: 3000 }
        );

        this.router.navigate(['/auth/login']);
      },
      error: (err) => {
        this.isLoading = false;

        // 1. Handle Field-Specific Validation Errors (Standard ASP.NET Core)
        if (err.error?.errors) {
          const validationErrors = err.error.errors;
          let hasFieldErrors = false;

          for (const key in validationErrors) {
            if (validationErrors.hasOwnProperty(key)) {
              const fieldName = key.charAt(0).toLowerCase() + key.slice(1);
              const errorMessage = validationErrors[key][0];

              // Check Personal Form
              const personalControl = this.personalDetailsForm.get(fieldName);
              if (personalControl) {
                personalControl.setErrors({ serverError: errorMessage });
                personalControl.markAsTouched();
                hasFieldErrors = true;
              }

              // Check Financial Form
              const financialControl = this.financialDetailsForm.get(fieldName);
              if (financialControl) {
                financialControl.setErrors({ serverError: errorMessage });
                financialControl.markAsTouched();
                hasFieldErrors = true;
              }
            }
          }

          if (hasFieldErrors) {
            this.snackBar.open('Please correct the highlighted errors.', 'Close', { duration: 4000 });
            return;
          }
        }

        // 2. Handle Custom Middleware Errors (e.g. "User with this email already exists")
        const customError = err.error?.error;
        if (customError) {
          // Heuristic: If message contains "email", map to email field
          if (customError.toLowerCase().includes('email')) {
            const emailControl = this.personalDetailsForm.get('email');
            if (emailControl) {
              emailControl.setErrors({ serverError: customError });
              emailControl.markAsTouched();
              // Jump to step 1 if needed, but we used stepper linear so it's fine
            }
          }

          this.snackBar.open(customError, 'Close', { duration: 4000 });
          return;
        }

        // 3. Fallback
        const message = err.error?.message || 'Registration failed. Please try again.';
        this.snackBar.open(message, 'Close', { duration: 4000 });
      }
    });
  }
}
