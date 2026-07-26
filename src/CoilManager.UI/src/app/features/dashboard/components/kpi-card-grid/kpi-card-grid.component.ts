import { Component, Input } from '@angular/core';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { DashboardKpi } from '../../models/operations-dashboard.model';

@Component({
  selector: 'app-kpi-card-grid',
  imports: [MatCardModule, MatIconModule],
  templateUrl: './kpi-card-grid.component.html',
  styleUrl: './kpi-card-grid.component.scss',
})
export class KpiCardGridComponent {
  @Input({ required: true }) kpis: readonly DashboardKpi[] = [];

  protected displayLabel(label: string): string {
    return label === 'Lamination Jobs' ? 'CTL Jobs' : label;
  }

  protected footerText(kpi: DashboardKpi): string | null {
    if (kpi.hint) {
      return kpi.hint;
    }

    const detailLabel = kpi.label === 'Dispatches' ? 'Pending' : 'In Progress';
    const detail = kpi.details?.find(item => item.label === detailLabel);
    if (detail) {
      return `${detailLabel}: ${detail.count}`;
    }

    const weights = kpi.details
      ?.map(item => Number(item.weight?.replace(/[^\d.-]/g, '')))
      .filter(weight => Number.isFinite(weight));
    if (weights?.length) {
      const totalWeight = weights.reduce((total, weight) => total + weight, 0);
      return `Total Weight: ${totalWeight.toLocaleString('en-US')} kg`;
    }

    return null;
  }
}
