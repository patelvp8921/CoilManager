import { DecimalPipe } from '@angular/common';
import { Component, Input } from '@angular/core';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatIconModule } from '@angular/material/icon';
import { SlittingSummary } from '../../models/operations-dashboard.model';

@Component({
  selector: 'app-slitting-summary',
  imports: [DecimalPipe, MatCardModule, MatChipsModule, MatIconModule],
  templateUrl: './slitting-summary.component.html',
  styleUrl: '../summary-card.scss',
})
export class SlittingSummaryComponent {
  @Input({ required: true }) slitting!: SlittingSummary;
}
