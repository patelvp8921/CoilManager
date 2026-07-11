import { DatePipe } from '@angular/common';
import { Component, Input } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { SlittingJob, SlittingJobStatus } from '../../models/slitting-job.model';

interface TimelineStep {
  label: string;
  icon: string;
  date?: string | null;
  user?: string | null;
  done: boolean;
}

@Component({
  selector: 'app-slitting-job-timeline',
  imports: [DatePipe, MatIconModule],
  template: `
    @if (job) {
      <section class="timeline" aria-label="Slitting job workflow timeline">
        @for (step of steps(); track step.label) {
          <div class="step" [class.done]="step.done">
            <span class="marker"><mat-icon>{{ step.icon }}</mat-icon></span>
            <div>
              <strong>{{ step.label }}</strong>
              @if (step.done && step.date) {
                <span>{{ step.date | date:'dd MMM yyyy HH:mm' }}</span>
                <small>{{ step.user || 'System' }}</small>
              } @else {
                <span>Pending</span>
              }
            </div>
          </div>
        }
      </section>
    }
  `,
  styles: [`
    .timeline {
      display: grid;
      grid-template-columns: repeat(5, minmax(0, 1fr));
      gap: 10px;
      margin: 12px 0;
    }

    .step {
      display: grid;
      grid-template-columns: 34px minmax(0, 1fr);
      gap: 8px;
      align-items: start;
      border: 1px solid #e4e7ec;
      border-radius: 8px;
      padding: 10px;
      background: #ffffff;
    }

    .marker {
      display: grid;
      width: 32px;
      height: 32px;
      place-items: center;
      border-radius: 999px;
      background: #f2f4f7;
      color: #667085;
    }

    .done .marker {
      background: #e8f5e9;
      color: #15803d;
    }

    strong,
    span,
    small {
      display: block;
      min-width: 0;
    }

    strong {
      color: #101828;
      font-size: 13px;
      line-height: 18px;
    }

    span,
    small {
      color: #667085;
      font-size: 12px;
      line-height: 18px;
    }

    @media (max-width: 980px) {
      .timeline {
        grid-template-columns: 1fr;
      }
    }
  `],
})
export class SlittingJobTimelineComponent {
  @Input({ required: true }) job!: SlittingJob;

  protected steps(): readonly TimelineStep[] {
    return [
      { label: 'Created', icon: 'add_circle', date: this.job.createdOn, user: this.job.createdBy, done: true },
      { label: 'Released', icon: 'lock', date: this.job.releasedOn, user: this.job.releasedBy, done: !!this.job.releasedOn || this.job.status !== SlittingJobStatus.Draft },
      { label: 'Started', icon: 'play_circle', date: this.job.startedOn, user: this.job.startedBy, done: !!this.job.startedOn || this.job.status === SlittingJobStatus.InProgress || this.job.status === SlittingJobStatus.Completed },
      { label: 'Completed', icon: 'task_alt', date: this.job.completedOn, user: this.job.completedBy, done: !!this.job.completedOn || this.job.status === SlittingJobStatus.Completed },
      { label: 'Cancelled', icon: 'cancel', date: this.job.cancelledOn, user: this.job.cancelledBy, done: !!this.job.cancelledOn || this.job.status === SlittingJobStatus.Cancelled },
    ];
  }
}
