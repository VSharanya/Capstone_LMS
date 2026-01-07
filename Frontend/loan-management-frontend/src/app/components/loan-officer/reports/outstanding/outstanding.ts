import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { FormsModule } from '@angular/forms';
import { ReportsService } from '../../../../services/reports-service';

@Component({
  selector: 'app-outstanding',
  standalone: true,
  imports: [
    CommonModule,
    MatTableModule,
    MatInputModule,
    MatButtonModule,
    FormsModule,
    MatIconModule,
    MatCardModule,
    MatFormFieldModule
  ],
  templateUrl: './outstanding.html',
  styleUrls: ['./outstanding.css']
})
export class OutstandingComponent {
  loanId: number | null = null;
  result = signal<any>(null);
  error = signal<string | null>(null);

  constructor(private service: ReportsService) { }

  fetchOutstanding() {
    if (!this.loanId) return;
    this.error.set(null);
    this.result.set(null);

    this.service.getOutstanding(this.loanId).subscribe({
      next: res => this.result.set(res),
      error: err => this.error.set('Loan ID not found or no outstanding details.')
    });
  }
}
