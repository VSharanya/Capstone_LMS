
import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { MatCardModule } from '@angular/material/card';
import { ReportsService } from '../../../../services/reports-service';

@Component({
  selector: 'app-officer-loans-by-status',
  standalone: true,
  imports: [CommonModule, MatTableModule, MatCardModule],
  templateUrl: './loans-by-status.html',
  styleUrls: ['./loans-by-status.css']
})
export class OfficerLoansByStatusComponent implements OnInit {
  data = signal<any[]>([]);
  displayedColumns = ['status', 'count'];

  // Computed Gradient for the Chart
  chartGradient = signal('');

  constructor(private service: ReportsService) { }

  ngOnInit() {
    this.service.getLoansByStatus().subscribe(res => {
      this.data.set(res);
      this.calculateGradient(res);
    });
  }

  calculateGradient(data: any[]) {
    if (!data.length) return;

    const total = data.reduce((acc, curr) => acc + curr.count, 0);
    let currentAngle = 0;
    let gradients: string[] = [];

    // Colors
    const colors: any = {
      'Confirmed': '#22c55e', // Active/Approved
      'Approved': '#22c55e',
      'Active': '#22c55e',
      'Pending': '#3b82f6', // Applied
      'Applied': '#3b82f6',
      'Under Review': '#eab308', // Yellow
      'Rejected': '#ef4444',
      'Closed': '#64748b'
    };

    // Sort to keep consistent order
    const sorted = [...data].sort((a, b) => a.status.localeCompare(b.status));

    sorted.forEach(item => {
      const percentage = (item.count / total) * 100;
      const color = colors[item.status] || '#cbd5e1'; // default slate-300

      gradients.push(`${color} ${currentAngle}% ${currentAngle + percentage}%`);
      currentAngle += percentage;
    });

    this.chartGradient.set(`conic-gradient(${gradients.join(', ')})`);
  }

  getTotalLoans(): number {
    return this.data().reduce((acc, curr) => acc + curr.count, 0);
  }

  getCount(status: string): number {
    const item = this.data().find(d => d.status === status);
    return item ? item.count : 0;
  }
}
