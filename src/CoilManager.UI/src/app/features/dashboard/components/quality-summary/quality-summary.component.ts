import { Component, Input } from '@angular/core';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatIconModule } from '@angular/material/icon';
import { QualitySummary } from '../../models/operations-dashboard.model';

@Component({
  selector: 'app-quality-summary',
  imports: [MatCardModule, MatChipsModule, MatIconModule],
  templateUrl: './quality-summary.component.html',
  styleUrl: '../summary-card.scss',
})
export class QualitySummaryComponent {
  @Input({ required: true }) quality!: QualitySummary;
}
