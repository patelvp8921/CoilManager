import { Component } from '@angular/core';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-dashboard-page',
  imports: [MatCardModule, MatIconModule],
  templateUrl: './dashboard-page.component.html',
  styleUrl: './dashboard-page.component.scss',
})
export class DashboardPageComponent {
  protected readonly metrics = [
    {
      label: 'Raw coil inventory',
      value: '0',
      icon: 'inventory_2',
    },
    {
      label: 'Pending receipts',
      value: '0',
      icon: 'pending_actions',
    },
    {
      label: 'Quality holds',
      value: '0',
      icon: 'rule',
    },
  ];
}
