import { DecimalPipe } from '@angular/common';
import { Component, Input, inject } from '@angular/core';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatIconModule } from '@angular/material/icon';
import { InventorySummary } from '../../models/operations-dashboard.model';
import { DashboardService } from '../../services/dashboard.service';

@Component({
  selector: 'app-inventory-summary',
  imports: [DecimalPipe, MatCardModule, MatChipsModule, MatIconModule],
  templateUrl: './inventory-summary.component.html',
  styleUrl: './inventory-summary.component.scss',
})
export class InventorySummaryComponent {
  protected readonly lowStockThreshold = 2;
  private readonly dashboardService = inject(DashboardService);

  @Input({ required: true }) inventory!: InventorySummary;

  protected openGradeDialog(dialog: HTMLDialogElement): void {
    this.dashboardService.getOperationsDashboard().subscribe({
      next: dashboard => {
        this.inventory = dashboard.inventory;
        dialog.showModal();
      },
      error: () => dialog.showModal(),
    });
  }
  protected get consumedMotherCoils(): number {
    return this.inventory.consumedMotherCoils ?? Math.max(
      0,
      this.inventory.totalMotherCoils
        - this.inventory.availableMotherCoils
        - this.inventory.reservedMotherCoils
        - this.inventory.holdMotherCoils
        - this.inventory.rejectedMotherCoils,
    );
  }

  protected get lowStockMotherCoils() {
    return this.inventory.gradeWiseStock.filter(item => item.count <= this.lowStockThreshold);
  }
}