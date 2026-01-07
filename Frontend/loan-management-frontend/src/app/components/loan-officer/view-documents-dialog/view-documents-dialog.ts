import { Component, Inject, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatTooltipModule } from '@angular/material/tooltip';
import { finalize } from 'rxjs/operators';
import { LoanOfficerLoanService } from '../../../services/loan-officer-loan-service';

@Component({
    selector: 'app-view-documents-dialog',
    standalone: true,
    imports: [
        CommonModule,
        MatDialogModule,
        MatButtonModule,
        MatIconModule,
        MatProgressBarModule,
        MatTooltipModule
    ],
    templateUrl: './view-documents-dialog.html',
    styleUrls: ['./view-documents-dialog.css']
})
export class ViewDocumentsDialogComponent implements OnInit {
    documents: any[] = [];
    loading = true;

    constructor(
        @Inject(MAT_DIALOG_DATA) public data: { loanId: number },
        private readonly service: LoanOfficerLoanService,
        private readonly cdr: ChangeDetectorRef
    ) { }

    ngOnInit() {
        this.service.getDocuments(this.data.loanId)
            .pipe(finalize(() => {
                this.loading = false;
                this.cdr.detectChanges();
            }))
            .subscribe({
                next: docs => {
                    this.documents = docs;
                },
                error: err => {
                    console.error('Error fetching documents:', err);
                }
            });
    }

    isImage(filename: string): boolean {
        return /\.(jpg|jpeg|png|gif)$/i.test(filename);
    }

    download(doc: any) {
        this.service.downloadDocument(doc.documentId).subscribe(blob => {
            const url = window.URL.createObjectURL(blob);
            const a = document.createElement('a');
            a.href = url;
            a.download = doc.originalFileName;
            a.click();
            window.URL.revokeObjectURL(url);
        });
    }
}
