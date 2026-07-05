import { Routes } from '@angular/router';
import { ShellComponent } from './layout/shell/shell.component';

export const routes: Routes = [
  {
    path: '',
    component: ShellComponent,
    children: [
      {
        path: '',
        pathMatch: 'full',
        redirectTo: 'dashboard',
      },
      {
        path: 'dashboard',
        loadComponent: () =>
          import('./features/dashboard/dashboard-page.component').then((component) => component.DashboardPageComponent),
        title: 'Dashboard | CoilManager',
      },
      {
        path: 'raw-coils',
        loadComponent: () =>
          import('./features/raw-coil/raw-coil-list-page.component').then((component) => component.RawCoilListPageComponent),
        title: 'Raw Coils | CoilManager',
      },
      {
        path: 'raw-coils/create',
        loadComponent: () =>
          import('./features/raw-coil/raw-coil-create-page.component').then((component) => component.RawCoilCreatePageComponent),
        title: 'Create Raw Coil | CoilManager',
      },
    ],
  },
  {
    path: '**',
    redirectTo: 'dashboard',
  },
];
