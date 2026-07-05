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

interface CoilMetric {
  label: string;
  value: string;
  detail: string;
  icon: string;
  tone: 'blue' | 'green' | 'amber' | 'red' | 'purple';
}

@Component({
  selector: 'app-raw-coil-list-page',
  imports: [RouterLink, MatButtonModule, MatCardModule, MatIconModule, MatTableModule],
  templateUrl: './raw-coil-list-page.component.html',
  styleUrl: './raw-coil-list-page.component.scss',
})
export class RawCoilListPageComponent {
  protected readonly metrics: CoilMetric[] = [
    { label: 'Total Coils', value: '128', detail: 'Total Weight: 1,258.560 MT', icon: 'track_changes', tone: 'blue' },
    { label: 'Available Weight', value: '1,085.320', detail: '86.24% of Total', icon: 'account_tree', tone: 'green' },
    { label: 'On Hold', value: '12', detail: 'Weight: 85.750 MT', icon: 'content_paste_search', tone: 'amber' },
    { label: 'Scrapped', value: '8', detail: 'Weight: 87.490 MT', icon: 'block', tone: 'red' },
    { label: 'Warehouses', value: '5', detail: 'Active Locations', icon: 'warehouse', tone: 'purple' },
  ];

  protected readonly columns: RawCoilColumn[] = [
    { key: 'select', label: '' },
    { key: 'coilId', label: 'Coil ID' },
    { key: 'heatNo', label: 'Heat No.' },
    { key: 'supplier', label: 'Supplier' },
    { key: 'grade', label: 'Grade' },
    { key: 'thickness', label: 'Thickness (mm)' },
    { key: 'width', label: 'Width (mm)' },
    { key: 'weight', label: 'Weight' },
    { key: 'status', label: 'Status' },
    { key: 'warehouse', label: 'Warehouse' },
    { key: 'receivedDate', label: 'Received Date' },
    { key: 'actions', label: 'Actions' },
  ];

  protected readonly displayedColumns = this.columns.map((column) => column.key);
  protected readonly dataSource: Record<string, string>[] = [
    { coilId: 'RC-2024-000128', heatNo: 'H-98123', supplier: 'Tata Steel Ltd.', grade: 'M6', thickness: '0.27', width: '1250', weight: '12.560', status: 'Available', warehouse: 'Main Warehouse', receivedDate: '21/05/2024' },
    { coilId: 'RC-2024-000127', heatNo: 'H-98122', supplier: 'Jindal Steel', grade: 'M5', thickness: '0.30', width: '1250', weight: '10.240', status: 'Available', warehouse: 'Main Warehouse', receivedDate: '20/05/2024' },
    { coilId: 'RC-2024-000126', heatNo: 'H-98121', supplier: 'POSCO', grade: 'M4', thickness: '0.23', width: '1000', weight: '8.750', status: 'On Hold', warehouse: 'QA Hold Area', receivedDate: '20/05/2024' },
    { coilId: 'RC-2024-000125', heatNo: 'H-98120', supplier: 'Tata Steel Ltd.', grade: 'M6', thickness: '0.27', width: '1250', weight: '15.300', status: 'Available', warehouse: 'Main Warehouse', receivedDate: '19/05/2024' },
    { coilId: 'RC-2024-000124', heatNo: 'H-98119', supplier: 'Jindal Steel', grade: 'M5', thickness: '0.30', width: '1250', weight: '11.450', status: 'Scrapped', warehouse: 'Scrap Yard', receivedDate: '18/05/2024' },
    { coilId: 'RC-2024-000123', heatNo: 'H-98118', supplier: 'POSCO', grade: 'M4', thickness: '0.23', width: '1000', weight: '7.680', status: 'Available', warehouse: 'Second Warehouse', receivedDate: '18/05/2024' },
    { coilId: 'RC-2024-000122', heatNo: 'H-98117', supplier: 'Tata Steel Ltd.', grade: 'M6', thickness: '0.27', width: '1250', weight: '12.800', status: 'Available', warehouse: 'Main Warehouse', receivedDate: '17/05/2024' },
    { coilId: 'RC-2024-000121', heatNo: 'H-98116', supplier: 'JFE Steel', grade: 'M5', thickness: '0.30', width: '1250', weight: '9.750', status: 'On Hold', warehouse: 'QA Hold Area', receivedDate: '17/05/2024' },
  ];
}
