import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';

import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';

import { AuthService } from '../../../services/auth-service';
import { LoginRequest } from '../../../models/auth';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatSnackBarModule
  ],
  templateUrl: './login.html',
  styleUrls: ['./login.css']
})

export class LoginComponent {

  isLoading = false;
  loginForm!: FormGroup;



  constructor(
    private readonly fb: FormBuilder,
    private readonly authService: AuthService,
    private readonly router: Router,
    private readonly snackBar: MatSnackBar
  ) {
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]]
    });
  }

  ngOnInit() {
    // Clear session when visiting login page
    this.authService.logout();

    // Also trigger navbar update if needed, but logout() clears token
    // Using a shared service with a subject would be better for reactivity, 
    // but for now, this ensures the state is clear.
    // If navbar doesn't react, we might need a notify method in AuthService.
  }

  onSubmit(): void {
    if (this.loginForm.invalid) {
      this.snackBar.open('Please fill all required fields', 'Close', {
        duration: 3000
      });
      return;
    }

    const payload = this.loginForm.value as LoginRequest;
    this.isLoading = true;

    this.authService.login(payload).subscribe({
      next: (res) => {
        this.authService.saveToken(res.token);

        const role = this.authService.getUserRole()?.toLowerCase();

        if (role === 'admin') {
          this.router.navigate(['/admin/dashboard']);
        } else if (role === 'loanofficer') {
          this.router.navigate(['/loan-officer/dashboard']);
        } else {
          this.router.navigate(['/customer/dashboard']);
        }
      },
      error: () => {
        this.snackBar.open('Invalid email or password', 'Close', {
          duration: 3000
        });
        this.isLoading = false;
      }
    });
  }
}
