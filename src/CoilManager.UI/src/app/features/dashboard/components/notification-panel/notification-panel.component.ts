import { Component, Input } from '@angular/core';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { RouterLink } from '@angular/router';
import { DashboardNotification } from '../../models/operations-dashboard.model';

@Component({
  selector: 'app-notification-panel',
  imports: [RouterLink, MatCardModule, MatIconModule],
  templateUrl: './notification-panel.component.html',
  styleUrl: './notification-panel.component.scss',
})
export class NotificationPanelComponent {
  @Input({ required: true }) notifications: readonly DashboardNotification[] = [];
}
