import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { OperationalAlert } from './operational-alerts.component';

@Component({
  selector: 'app-operational-alert-detail-dialog',
  imports: [MatDialogModule, MatButtonModule, MatIconModule],
  template: `
    <div class="dialog-heading">
      <span class="alert-icon" [class]="data.severity"><mat-icon>{{ icon }}</mat-icon></span>
      <div><span class="eyebrow">Operational alert</span><h2 mat-dialog-title>{{ data.title }}</h2></div>
    </div>
    <mat-dialog-content>
      <p class="description">{{ data.description }}</p>
      <dl>
        <div><dt>Status</dt><dd>{{ status }}</dd></div>
        <div><dt>Last updated</dt><dd>{{ data.relativeTime }}</dd></div>
      </dl>
      <section class="guidance">
        <mat-icon>lightbulb</mat-icon>
        <div><strong>Recommended action</strong><p>{{ recommendation }}</p></div>
      </section>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button mat-dialog-close>Close</button>
      @if (data.route) { <button mat-flat-button [mat-dialog-close]="true">Open details</button> }
    </mat-dialog-actions>
  `,
  styles: [`
    :host { display: block; color: #101828; }
    .dialog-heading { display: grid; grid-template-columns: 44px 1fr; align-items: center; gap: 12px; padding: 22px 24px 4px; }
    .alert-icon { display: grid; width: 44px; height: 44px; place-items: center; border-radius: 12px; background: #fff4e5; color: #b54708; }
    .alert-icon.critical { background: #fef3f2; color: #d92d20; }
    .alert-icon.information { background: #eff8ff; color: #175cd3; }
    .eyebrow { color: #667085; font-size: 11px; font-weight: 700; letter-spacing: .08em; text-transform: uppercase; }
    h2[mat-dialog-title] { margin: 2px 0 0; padding: 0; font-size: 20px; font-weight: 800; line-height: 26px; }
    mat-dialog-content { padding-top: 16px; }
    .description { margin: 0 0 18px; color: #344054; font-size: 14px; line-height: 21px; }
    dl { display: grid; margin: 0; border: 1px solid #e4e7ec; border-radius: 10px; overflow: hidden; }
    dl div { display: grid; grid-template-columns: 120px 1fr; padding: 11px 14px; }
    dl div + div { border-top: 1px solid #e4e7ec; }
    dt { color: #667085; font-size: 12px; font-weight: 600; }
    dd { margin: 0; color: #101828; font-size: 13px; font-weight: 700; }
    .guidance { display: grid; grid-template-columns: 24px 1fr; gap: 10px; margin-top: 16px; border-radius: 10px; padding: 13px; background: #f8fafc; }
    .guidance mat-icon { color: #0b63ce; }
    .guidance strong { font-size: 13px; }
    .guidance p { margin: 3px 0 0; color: #475467; font-size: 12px; line-height: 18px; }
    mat-dialog-actions { padding: 8px 24px 20px; }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class OperationalAlertDetailDialogComponent {
  protected readonly data = inject<OperationalAlert>(MAT_DIALOG_DATA);
  protected readonly icon = this.data.severity === 'critical' ? 'error' : 'warning_amber';
  protected readonly status = this.data.severity === 'critical' ? 'Immediate attention required' : 'Action required';
  protected readonly recommendation = this.getRecommendation();

  private getRecommendation(): string {
    switch (this.data.category) {
      case 'material-shortage': return 'Review the work order requirement and arrange or reallocate the missing material.';
      case 'awaiting-allocation': return 'Open the job and allocate the required material before production planning.';
      case 'dispatch-pending': return 'Review completed jobs and prepare the eligible items for dispatch.';
      default: return 'Review this alert and take the appropriate operational action.';
    }
  }
}
