import { DatePipe, DecimalPipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, DestroyRef, OnInit, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { EMPTY, catchError, distinctUntilChanged, finalize, map, switchMap, timeout } from 'rxjs';
import { RawCoil, statusLabel } from '../../models/raw-coil.model';
import { RawCoilService } from '../../services/raw-coil.service';

@Component({
  selector: 'app-raw-coil-detail-page',
  imports: [DatePipe, DecimalPipe, RouterLink, MatButtonModule, MatCardModule, MatIconModule, MatProgressBarModule],
  templateUrl: './raw-coil-detail-page.component.html',
  styleUrl: './raw-coil-detail-page.component.scss',
})
export class RawCoilDetailPageComponent implements OnInit {
  protected readonly rawCoil = signal<RawCoil | null>(null);
  protected readonly isLoading = signal(false);
  protected readonly error = signal('');

  private readonly route = inject(ActivatedRoute);
  private readonly destroyRef = inject(DestroyRef);
  private readonly rawCoilService = inject(RawCoilService);

  ngOnInit(): void {
    this.route.paramMap
      .pipe(
        map((params) => params.get('id') ?? ''),
        distinctUntilChanged(),
        switchMap((id) => {
          this.rawCoil.set(null);
          this.error.set('');

          if (!id) {
            this.error.set('Mother coil id is missing from the route.');
            return EMPTY;
          }

          this.isLoading.set(true);
          return this.rawCoilService.getRawCoilById(id).pipe(
            timeout(15000),
            catchError((error: unknown) => {
              this.error.set(this.extractError(error));
              return EMPTY;
            }),
            finalize(() => this.isLoading.set(false)),
          );
        }),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe((rawCoil) => this.rawCoil.set(rawCoil));
  }

  protected statusLabel(rawCoil: RawCoil): string {
    return statusLabel(rawCoil.status);
  }

  private extractError(error: unknown): string {
    if (error instanceof HttpErrorResponse) {
      if (error.status === 0) {
        return 'The API is not reachable at http://localhost:5170. Start CoilManager.API and try again.';
      }

      const body = error.error as { message?: string; errors?: string[] } | null;
      return body?.errors?.join('\n') || body?.message || error.message || 'Mother coil could not be loaded.';
    }

    if (error instanceof Error && error.name === 'TimeoutError') {
      return 'Mother coil detail request timed out. Please check that the API is running and try again.';
    }

    return error instanceof Error ? error.message : 'Mother coil could not be loaded.';
  }
}
