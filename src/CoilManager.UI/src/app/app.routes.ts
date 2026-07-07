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
        path: 'raw-coils/create',
        loadComponent: () =>
          import('./features/raw-coil/pages/raw-coil-create/raw-coil-create-page.component').then((component) => component.RawCoilCreatePageComponent),
        title: 'Create Raw Coil | CoilManager',
      },
      {
        path: 'raw-coils/:id/edit',
        loadComponent: () =>
          import('./features/raw-coil/pages/raw-coil-edit/raw-coil-edit-page.component').then((component) => component.RawCoilEditPageComponent),
        title: 'Edit Raw Coil | CoilManager',
      },
      {
        path: 'raw-coils/:id/view',
        loadComponent: () =>
          import('./features/raw-coil/pages/raw-coil-detail/raw-coil-detail-page.component').then((component) => component.RawCoilDetailPageComponent),
        title: 'Raw Coil Detail | CoilManager',
      },
      {
        path: 'raw-coils/:id',
        loadComponent: () =>
          import('./features/raw-coil/pages/raw-coil-detail/raw-coil-detail-page.component').then((component) => component.RawCoilDetailPageComponent),
        title: 'Raw Coil Detail | CoilManager',
      },
      {
        path: 'raw-coils',
        loadComponent: () =>
          import('./features/raw-coil/pages/raw-coil-list/raw-coil-list-page.component').then((component) => component.RawCoilListPageComponent),
        title: 'Raw Coils | CoilManager',
      },
      {
        path: 'admin/manufacturers/create',
        loadComponent: () =>
          import('./features/admin/master-data/master-form-page.component').then((component) => component.MasterFormPageComponent),
        title: 'Create Manufacturer | CoilManager',
        data: { type: 'manufacturers', title: 'Create Manufacturer', singular: 'Manufacturer' },
      },
      {
        path: 'admin/manufacturers/:id/edit',
        loadComponent: () =>
          import('./features/admin/master-data/master-form-page.component').then((component) => component.MasterFormPageComponent),
        title: 'Edit Manufacturer | CoilManager',
        data: { type: 'manufacturers', title: 'Edit Manufacturer', singular: 'Manufacturer' },
      },
      {
        path: 'admin/manufacturers',
        loadComponent: () =>
          import('./features/admin/master-data/master-list-page.component').then((component) => component.MasterListPageComponent),
        title: 'Manufacturers | CoilManager',
        data: { type: 'manufacturers', title: 'Manufacturers', singular: 'Manufacturer' },
      },
      {
        path: 'admin/suppliers/create',
        loadComponent: () =>
          import('./features/admin/master-data/master-form-page.component').then((component) => component.MasterFormPageComponent),
        title: 'Create Supplier | CoilManager',
        data: { type: 'suppliers', title: 'Create Supplier', singular: 'Supplier' },
      },
      {
        path: 'admin/suppliers/:id/edit',
        loadComponent: () =>
          import('./features/admin/master-data/master-form-page.component').then((component) => component.MasterFormPageComponent),
        title: 'Edit Supplier | CoilManager',
        data: { type: 'suppliers', title: 'Edit Supplier', singular: 'Supplier' },
      },
      {
        path: 'admin/suppliers',
        loadComponent: () =>
          import('./features/admin/master-data/master-list-page.component').then((component) => component.MasterListPageComponent),
        title: 'Suppliers | CoilManager',
        data: { type: 'suppliers', title: 'Suppliers', singular: 'Supplier' },
      },
      {
        path: 'admin/grades/create',
        loadComponent: () =>
          import('./features/admin/master-data/master-form-page.component').then((component) => component.MasterFormPageComponent),
        title: 'Create Grade | CoilManager',
        data: { type: 'grades', title: 'Create Grade', singular: 'Grade' },
      },
      {
        path: 'admin/grades/:id/edit',
        loadComponent: () =>
          import('./features/admin/master-data/master-form-page.component').then((component) => component.MasterFormPageComponent),
        title: 'Edit Grade | CoilManager',
        data: { type: 'grades', title: 'Edit Grade', singular: 'Grade' },
      },
      {
        path: 'admin/grades',
        loadComponent: () =>
          import('./features/admin/master-data/master-list-page.component').then((component) => component.MasterListPageComponent),
        title: 'Grades | CoilManager',
        data: { type: 'grades', title: 'Grades', singular: 'Grade' },
      },
    ],
  },
  {
    path: '**',
    redirectTo: 'dashboard',
  },
];
