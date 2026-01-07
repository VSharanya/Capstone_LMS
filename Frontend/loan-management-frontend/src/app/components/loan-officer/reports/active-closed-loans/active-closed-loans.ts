import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { ReportsService } from '../../../../services/reports-service';

@Component({
    selector: 'app-officer-active-closed-loans',
    standalone: true,
    imports: [CommonModule, MatCardModule, MatIconModule, MatProgressBarModule],
    templateUrl: './active-closed-loans.html',
    styleUrls: ['./active-closed-loans.css']
})
export class OfficerActiveClosedLoansComponent implements OnInit {
    activeCount = signal(0);
    closedCount = signal(0);
    loading = signal(true);

    // Computed ratio for progress bar (0 to 100)
    activePercentage = computed(() => {
        const total = this.activeCount() + this.closedCount();
        return total > 0 ? (this.activeCount() / total) * 100 : 0;
    });

    constructor(private service: ReportsService) { }

    ngOnInit() {
        this.service.getLoansByStatus().subscribe({
            next: (res: any[]) => {
                const active = res.find(i => i.status === 'Active')?.count || 0;
                const closed = res.find(i => i.status === 'Closed')?.count || 0;

                this.activeCount.set(active);
                this.closedCount.set(closed);
                this.loading.set(false);
            },
            error: () => this.loading.set(false)
        });
    }
}
