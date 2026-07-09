import { DecimalPipe } from '@angular/common';
import { Component, Input } from '@angular/core';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatIconModule } from '@angular/material/icon';
import { ProductionSummary } from '../../models/operations-dashboard.model';

@Component({
  selector: 'app-production-summary',
  imports: [DecimalPipe, MatCardModule, MatChipsModule, MatIconModule],
  templateUrl: './production-summary.component.html',
  styleUrl: '../summary-card.scss',
})
export class ProductionSummaryComponent {
  @Input({ required: true }) production!: ProductionSummary;
}
