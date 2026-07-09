import { DatePipe } from '@angular/common';
import { Component, Input } from '@angular/core';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { RouterLink } from '@angular/router';
import { RecentActivity } from '../../models/operations-dashboard.model';

@Component({
  selector: 'app-recent-activities',
  imports: [DatePipe, RouterLink, MatCardModule, MatIconModule],
  templateUrl: './recent-activities.component.html',
  styleUrl: './recent-activities.component.scss',
})
export class RecentActivitiesComponent {
  @Input({ required: true }) activities: readonly RecentActivity[] = [];
}
