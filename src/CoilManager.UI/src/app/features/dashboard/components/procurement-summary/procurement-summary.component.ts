import { DecimalPipe } from '@angular/common';
import { Component, Input } from '@angular/core';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatIconModule } from '@angular/material/icon';
import { ProcurementSummary } from '../../models/operations-dashboard.model';

@Component({
  selector: 'app-procurement-summary',
  imports: [DecimalPipe, MatCardModule, MatChipsModule, MatIconModule],
  templateUrl: './procurement-summary.component.html',
  styleUrl: '../summary-card.scss',
})
export class ProcurementSummaryComponent {
  @Input({ required: true }) procurement!: ProcurementSummary;
}
