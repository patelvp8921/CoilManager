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
}
