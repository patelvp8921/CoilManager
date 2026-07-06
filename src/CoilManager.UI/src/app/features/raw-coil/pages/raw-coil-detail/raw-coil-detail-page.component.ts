import { DatePipe, DecimalPipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { RawCoil, statusLabel } from '../../models/raw-coil.model';
import { RawCoilService } from '../../services/raw-coil.service';

@Component({
  selector: 'app-raw-coil-detail-page',
  imports: [DatePipe, DecimalPipe, RouterLink, MatButtonModule, MatCardModule, MatIconModule, MatProgressBarModule],
  templateUrl: './raw-coil-detail-page.component.html',
  styleUrl: './raw-coil-detail-page.component.scss',
})
export class RawCoilDetailPageComponent implements OnInit {
  protected rawCoil?: RawCoil;
  protected isLoading = false;
  protected error = '';

  private readonly route = inject(ActivatedRoute);
  private readonly rawCoilService = inject(RawCoilService);
  private readonly id = this.route.snapshot.paramMap.get('id') ?? '';

  ngOnInit(): void {
    this.isLoading = true;
    this.rawCoilService
      .getRawCoilById(this.id)
      .pipe(finalize(() => (this.isLoading = false)))
      .subscribe({
        next: (rawCoil) => (this.rawCoil = rawCoil),
        error: (error: HttpErrorResponse) => {
          const body = error.error as { message?: string; errors?: string[] } | null;
          this.error = body?.errors?.join('\n') || body?.message || error.message || 'Raw coil could not be loaded.';
        },
      });
  }

  protected statusLabel(rawCoil: RawCoil): string {
    return statusLabel(rawCoil.status);
  }
}
