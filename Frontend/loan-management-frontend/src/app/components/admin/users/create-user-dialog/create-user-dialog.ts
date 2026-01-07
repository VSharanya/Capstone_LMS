import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatDialogRef, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatSnackBar } from '@angular/material/snack-bar';

import { AdminUserService } from '../../../../services/admin-user-service';

@Component({
  selector: 'app-create-user-dialog',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule
  ],
  templateUrl: './create-user-dialog.html',
  styleUrls: ['./create-user-dialog.css']
})
export class CreateUserDialogComponent {

  loading = false;



  roles = ['Admin', 'LoanOfficer'];
  form!: FormGroup;

  constructor(
    private readonly fb: FormBuilder,
    private readonly dialogRef: MatDialogRef<CreateUserDialogComponent>,
    private readonly userService: AdminUserService,
    private readonly snack: MatSnackBar
  ) {
    this.form = this.fb.group({
      fullName: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      role: ['', Validators.required]
    });
    console.log('CreateUserDialogComponent initialized');
  }

  submit() {
    if (this.form.invalid) return;

    this.loading = true;

    this.userService.createUser(this.form.value).subscribe({
      next: () => {
        this.snack.open(
          'User created successfully. Default password has been emailed.',
          'Close',
          { duration: 4000 }
        );
        this.dialogRef.close(true);
      },
      error: err => {
        const message =
          err?.error?.message ||
          err?.error ||
          'Failed to create user';

        this.snack.open(message, 'Close', { duration: 3000 });
        this.loading = false;
      }
    });
  }

  cancel() {
    this.dialogRef.close(false);
  }
}
