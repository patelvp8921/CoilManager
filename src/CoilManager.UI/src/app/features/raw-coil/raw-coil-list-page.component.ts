import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatTableModule } from '@angular/material/table';

interface RawCoilColumn {
  key: string;
  label: string;
}

@Component({
  selector: 'app-raw-coil-list-page',
  imports: [RouterLink, MatButtonModule, MatCardModule, MatIconModule, MatTableModule],
  templateUrl: './raw-coil-list-page.component.html',
  styleUrl: './raw-coil-list-page.component.scss',
})
export class RawCoilListPageComponent {
  protected readonly columns: RawCoilColumn[] = [
    { key: 'coilNumber', label: 'Coil No.' },
    { key: 'grade', label: 'Grade' },
    { key: 'weight', label: 'Weight' },
    { key: 'status', label: 'Status' },
  ];

  protected readonly displayedColumns = this.columns.map((column) => column.key);
  protected readonly dataSource: Record<string, string>[] = [];
}
