import { Component, EventEmitter, Input, Output } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatDividerModule } from '@angular/material/divider';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { CoilStatus, statusLabel } from '../../models/raw-coil.model';

export interface CoilPreviewModel {
  coilId: string;
  supplier: string;
  manufacturer: string;
  grade: string;
  thickness: number | null;
  width: number | null;
  weight: number | null;
  status: CoilStatus;
}

@Component({
  selector: 'app-coil-preview',
  imports: [
    MatButtonModule,
    MatCardModule,
    MatChipsModule,
    MatDividerModule,
    MatIconModule,
    MatProgressSpinnerModule,
  ],
  templateUrl: './coil-preview.component.html',
  styleUrl: './coil-preview.component.scss',
})
export class CoilPreviewComponent {
  @Input({ required: true }) preview!: CoilPreviewModel;
  @Input() qrCodeDataUrl = '';
  @Input() isQrGenerating = false;

  @Output() readonly generateQr = new EventEmitter<void>();

  protected readonly placeholder = 'Not selected';

  protected statusLabel(status: CoilStatus): string {
    return statusLabel(status);
  }

  protected statusClass(status: CoilStatus): string {
    switch (status) {
      case CoilStatus.Available:
        return 'status-available';
      case CoilStatus.Reserved:
      case CoilStatus.OnHold:
        return 'status-reserved';
      case CoilStatus.Rejected:
      case CoilStatus.Scrapped:
        return 'status-rejected';
      case CoilStatus.UnderInspection:
        return 'status-inspection';
      case CoilStatus.Draft:
        return 'status-draft';
      default:
        return 'status-default';
    }
  }

  protected formatNumber(value: number | null): string {
    return value === null || value === undefined ? this.placeholder : `${value}`;
  }
}
