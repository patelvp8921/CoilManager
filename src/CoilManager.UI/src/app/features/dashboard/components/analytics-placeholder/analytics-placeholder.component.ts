import { Component, Input } from '@angular/core';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { AnalyticsSummary } from '../../models/operations-dashboard.model';

@Component({
  selector: 'app-analytics-placeholder',
  imports: [MatCardModule, MatIconModule],
  templateUrl: './analytics-placeholder.component.html',
  styleUrl: './analytics-placeholder.component.scss',
})
export class AnalyticsPlaceholderComponent {
  @Input({ required: true }) analytics!: AnalyticsSummary;
}
