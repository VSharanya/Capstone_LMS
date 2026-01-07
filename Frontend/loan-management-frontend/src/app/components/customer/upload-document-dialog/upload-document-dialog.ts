import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { CustomerLoanService } from '../../../services/customer-loan-service';
import { finalize } from 'rxjs';

@Component({
  selector: 'app-upload-document-dialog',
  standalone: true,
  imports: [
    CommonModule,
    MatDialogModule,
    MatButtonModule,
    MatIconModule,
    MatSnackBarModule,
    MatProgressBarModule
  ],
  templateUrl: './upload-document-dialog.html',
  styleUrls: ['./upload-document-dialog.css']
})
export class UploadDocumentDialogComponent {
  selectedFile: File | null = null;
  uploading = false;

  constructor(
    @Inject(MAT_DIALOG_DATA) public data: { loanId: number },
    private service: CustomerLoanService,
    private snack: MatSnackBar,
    private dialogRef: MatDialogRef<UploadDocumentDialogComponent>
  ) { }

  onFileSelected(event: any) {
    this.selectedFile = event.target.files[0] ?? null;
  }

  upload() {
    if (!this.selectedFile) return;

    this.uploading = true;
    const formData = new FormData();
    formData.append('loanApplicationId', this.data.loanId.toString());
    formData.append('documentType', 'IdentityProof');
    formData.append('file', this.selectedFile);

    this.service.uploadDocument(formData)
      .pipe(finalize(() => this.uploading = false))
      .subscribe({
        next: () => {
          this.snack.open('Document uploaded successfully!', 'Close', { duration: 3000 });
          this.dialogRef.close(true);
        },
        error: () => {
          this.snack.open('Failed to upload document', 'Close', { duration: 3000 });
        }
      });
  }
}
