import { DecimalPipe } from '@angular/common';
import { Component, Input } from '@angular/core';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatIconModule } from '@angular/material/icon';
import { InventorySummary } from '../../models/operations-dashboard.model';

@Component({
  selector: 'app-inventory-summary',
  imports: [DecimalPipe, MatCardModule, MatChipsModule, MatIconModule],
  templateUrl: './inventory-summary.component.html',
  styleUrl: './inventory-summary.component.scss',
})
export class InventorySummaryComponent {
  @Input({ required: true }) inventory!: InventorySummary;
}
