import { Component } from '@angular/core';
import { MatDialogRef, MatDialogModule } from '@angular/material/dialog';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButton, MatButtonModule } from '@angular/material/button';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-reject-dialog',
  imports: [
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    ReactiveFormsModule,
    CommonModule
  ],
  templateUrl: './reject-dialog.html',
  styleUrls: ['./reject-dialog.css']
})
export class RejectDialogComponent {
  form: FormGroup;

  

  constructor(
    private fb: FormBuilder,
    private dialogRef: MatDialogRef<RejectDialogComponent>
  ) {this.form = this.fb.group({
    remarks: ['', Validators.required]
  });}

  close() {
    this.dialogRef.close();
  }

  confirm() {
    this.dialogRef.close(this.form.value.remarks);
  }
}