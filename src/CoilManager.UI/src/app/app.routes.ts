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
        path: 'mother-coils/create',
        loadComponent: () =>
          import('./features/raw-coil/pages/raw-coil-create/raw-coil-create-page.component').then((component) => component.RawCoilCreatePageComponent),
        title: 'Create Mother Coil | CoilManager',
      },
      {
        path: 'mother-coils/:id/edit',
        loadComponent: () =>
          import('./features/raw-coil/pages/raw-coil-edit/raw-coil-edit-page.component').then((component) => component.RawCoilEditPageComponent),
        title: 'Mother Coil Details | CoilManager',
      },
      {
        path: 'mother-coils/:id/details',
        loadComponent: () =>
          import('./features/raw-coil/pages/raw-coil-edit/raw-coil-edit-page.component').then((component) => component.RawCoilEditPageComponent),
        title: 'Mother Coil Details | CoilManager',
      },
      {
        path: 'mother-coils/:id/view',
        redirectTo: (route) => `/mother-coils/${route.params['id']}/details`,
        pathMatch: 'full',
      },
      {
        path: 'mother-coils/:id',
        redirectTo: '/mother-coils/:id/details',
        pathMatch: 'full',
      },
      {
        path: 'mother-coils',
        loadComponent: () =>
          import('./features/raw-coil/pages/raw-coil-list/raw-coil-list-page.component').then((component) => component.RawCoilListPageComponent),
        title: 'Mother Coils | CoilManager',
      },
      {
        path: 'raw-coils/create',
        redirectTo: '/mother-coils/create',
        pathMatch: 'full',
      },
      {
        path: 'raw-coils/:id/edit',
        redirectTo: (route) => `/mother-coils/${route.params['id']}/edit`,
        pathMatch: 'full',
      },
      {
        path: 'raw-coils/:id/details',
        redirectTo: (route) => `/mother-coils/${route.params['id']}/details`,
        pathMatch: 'full',
      },
      {
        path: 'raw-coils/:id/view',
        redirectTo: (route) => `/mother-coils/${route.params['id']}/details`,
        pathMatch: 'full',
      },
      {
        path: 'raw-coils/:id',
        redirectTo: (route) => `/mother-coils/${route.params['id']}/details`,
        pathMatch: 'full',
      },
      {
        path: 'raw-coils',
        redirectTo: '/mother-coils',
        pathMatch: 'full',
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
