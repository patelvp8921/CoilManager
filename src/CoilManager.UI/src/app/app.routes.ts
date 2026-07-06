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
          import('./features/raw-coil/pages/raw-coil-list/raw-coil-list-page.component').then((component) => component.RawCoilListPageComponent),
        title: 'Raw Coils | CoilManager',
      },
      {
        path: 'raw-coils/create',
        loadComponent: () =>
          import('./features/raw-coil/pages/raw-coil-create/raw-coil-create-page.component').then((component) => component.RawCoilCreatePageComponent),
        title: 'Create Raw Coil | CoilManager',
      },
      {
        path: 'raw-coils/:id',
        loadComponent: () =>
          import('./features/raw-coil/pages/raw-coil-detail/raw-coil-detail-page.component').then((component) => component.RawCoilDetailPageComponent),
        title: 'Raw Coil Detail | CoilManager',
      },
      {
        path: 'raw-coils/:id/edit',
        loadComponent: () =>
          import('./features/raw-coil/pages/raw-coil-edit/raw-coil-edit-page.component').then((component) => component.RawCoilEditPageComponent),
        title: 'Edit Raw Coil | CoilManager',
      },
      {
        path: 'admin/suppliers',
        loadComponent: () =>
          import('./features/admin/master-placeholder/master-placeholder-page.component').then((component) => component.MasterPlaceholderPageComponent),
        title: 'Suppliers | CoilManager',
        data: { title: 'Supplier List', section: 'Supplier master', createRoute: '/admin/suppliers/create' },
      },
      {
        path: 'admin/suppliers/create',
        loadComponent: () =>
          import('./features/admin/master-placeholder/master-placeholder-page.component').then((component) => component.MasterPlaceholderPageComponent),
        title: 'Create Supplier | CoilManager',
        data: { title: 'Supplier Create', section: 'Supplier master' },
      },
      {
        path: 'admin/manufacturers',
        loadComponent: () =>
          import('./features/admin/master-placeholder/master-placeholder-page.component').then((component) => component.MasterPlaceholderPageComponent),
        title: 'Manufacturers | CoilManager',
        data: { title: 'Manufacturer List', section: 'Manufacturer master', createRoute: '/admin/manufacturers/create' },
      },
      {
        path: 'admin/manufacturers/create',
        loadComponent: () =>
          import('./features/admin/master-placeholder/master-placeholder-page.component').then((component) => component.MasterPlaceholderPageComponent),
        title: 'Create Manufacturer | CoilManager',
        data: { title: 'Manufacturer Create', section: 'Manufacturer master' },
      },
      {
        path: 'admin/grades',
        loadComponent: () =>
          import('./features/admin/master-placeholder/master-placeholder-page.component').then((component) => component.MasterPlaceholderPageComponent),
        title: 'Grades | CoilManager',
        data: { title: 'Grade List', section: 'Grade master', createRoute: '/admin/grades/create' },
      },
      {
        path: 'admin/grades/create',
        loadComponent: () =>
          import('./features/admin/master-placeholder/master-placeholder-page.component').then((component) => component.MasterPlaceholderPageComponent),
        title: 'Create Grade | CoilManager',
        data: { title: 'Grade Create', section: 'Grade master' },
      },
    ],
  },
  {
    path: '**',
    redirectTo: 'dashboard',
  },
];
