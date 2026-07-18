import { Component, Input, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MAT_DIALOG_DATA, MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';

export interface PlateFieldDefinition { label: string; description: string; }

@Component({
  selector: 'app-plate-reference-panel',
  imports: [MatButtonModule, MatCardModule, MatDialogModule, MatIconModule],
  templateUrl: './plate-reference-panel.component.html',
  styleUrl: './plate-reference-panel.component.scss',
})
export class PlateReferencePanelComponent {
  @Input({ required: true }) plateType = '';
  @Input() icon = 'image';
  @Input({ required: true }) imageUrl = '';
  @Input({ required: true }) title = '';
  @Input() fieldDefinitions: PlateFieldDefinition[] = [];
  @Input() notes = '';
  private readonly dialog = inject(MatDialog);

  protected expand(): void {
    this.dialog.open(PlateImageDialogComponent, {
      data: {
        imageUrl: this.imageUrl,
        title: this.title,
        alt: `${this.plateType} plate profile reference`,
        fieldDefinitions: this.fieldDefinitions,
        notes: this.notes,
      },
      width: '1100px',
      maxWidth: '96vw',
    });
  }
}

@Component({
  selector: 'app-plate-image-dialog',
  imports: [MatButtonModule, MatDialogModule],
  template: `
    <h2 mat-dialog-title>{{ data.title }}</h2>
    <mat-dialog-content>
      <div class="dialog-layout">
        <div class="dialog-image"><img [src]="data.imageUrl" [alt]="data.alt"></div>
        <aside>
          <h3>Field Guide</h3>
          <dl>
            @for (field of data.fieldDefinitions; track field.label) {
              <div><dt>{{ field.label }}</dt><dd>{{ field.description }}</dd></div>
            }
          </dl>
          @if (data.notes) {
            <section class="production-note">
              <h3>Production Note</h3>
              <p>{{ data.notes }}</p>
            </section>
          }
        </aside>
      </div>
    </mat-dialog-content>
    <mat-dialog-actions align="end"><button mat-button mat-dialog-close>Close</button></mat-dialog-actions>
  `,
  styles: [`
    mat-dialog-content{max-height:78vh}.dialog-layout{display:grid;grid-template-columns:minmax(0,1.7fr) minmax(260px,.8fr);gap:20px}.dialog-image{display:grid;place-items:center;min-height:500px;overflow:hidden;border:1px solid #e0e6ef;border-radius:8px;background:#fff}.dialog-image img{display:block;width:100%;max-height:68vh;object-fit:contain}aside{padding:4px 2px}h3{margin:0 0 10px;color:#17365f;font-size:14px}dl{margin:0}dl div{padding:8px 0;border-bottom:1px solid #e7ebf1}dt{color:#26384e;font-size:12px;font-weight:650}dd{margin:2px 0 0;color:#5e6d7e;font-size:12px;line-height:1.4}.production-note{margin-top:18px;padding:12px;border:1px solid #d9e5f4;border-radius:7px;background:#f5f9fe}.production-note h3{margin-bottom:5px}.production-note p{margin:0;color:#46576a;font-size:12px;line-height:1.45}@media(max-width:800px){.dialog-layout{grid-template-columns:1fr}.dialog-image{min-height:320px}}
  `],
})
export class PlateImageDialogComponent {
  readonly data = inject<{
    imageUrl: string;
    title: string;
    alt: string;
    fieldDefinitions: PlateFieldDefinition[];
    notes: string;
  }>(MAT_DIALOG_DATA);
}