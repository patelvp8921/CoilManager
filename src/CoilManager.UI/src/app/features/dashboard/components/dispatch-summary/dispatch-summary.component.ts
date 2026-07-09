import { DecimalPipe } from '@angular/common';
import { Component, Input } from '@angular/core';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatIconModule } from '@angular/material/icon';
import { DispatchSummary } from '../../models/operations-dashboard.model';

@Component({
  selector: 'app-dispatch-summary',
  imports: [DecimalPipe, MatCardModule, MatChipsModule, MatIconModule],
  templateUrl: './dispatch-summary.component.html',
  styleUrl: '../summary-card.scss',
})
export class DispatchSummaryComponent {
  @Input({ required: true }) dispatch!: DispatchSummary;
}
