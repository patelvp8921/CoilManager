import { Routes } from '@angular/router';
import { ShellComponent } from './layout/shell/shell.component';
import { environment } from '../environments/environment';

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
          import('./features/dashboard/pages/operations-dashboard/operations-dashboard.component').then(
            (component) => component.OperationsDashboardComponent,
          ),
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
        path: 'work-orders/create',
        loadComponent: () => import('./features/work-orders/work-order-form.component').then(c => c.WorkOrderFormComponent),
        title: 'Create Work Order | CoilManager',
      },
      {
        path: 'work-orders/:id/edit',
        loadComponent: () => import('./features/work-orders/work-order-form.component').then(c => c.WorkOrderFormComponent),
        title: 'Edit Work Order | CoilManager',
      },
      {
        path: 'work-orders/:id/allocations',
        loadComponent: () => import('./features/work-orders/work-order-detail.component').then(c => c.WorkOrderDetailComponent),
        title: 'Work Order Allocations | CoilManager',
      },
      {
        path: 'work-orders/:id',
        loadComponent: () => import('./features/work-orders/work-order-detail.component').then(c => c.WorkOrderDetailComponent),
        title: 'Work Order Details | CoilManager',
      },
      {
        path: 'work-orders',
        loadComponent: () => import('./features/work-orders/work-order-list.component').then(c => c.WorkOrderListComponent),
        title: 'Work Orders | CoilManager',
      },
      {
        path: 'lamination-jobs/create',
        loadComponent: () => import('./features/lamination-jobs/lamination-job-form.component').then(c => c.LaminationJobFormComponent),
        title: 'Lamination / Cut-to-Length Job | CoilManager',
      },
      {
        path: 'lamination-jobs/:id/edit',
        loadComponent: () => import('./features/lamination-jobs/lamination-job-form.component').then(c => c.LaminationJobFormComponent),
        title: 'Edit Lamination Job | CoilManager',
      },      {
        path: 'lamination-jobs/:id/view',
        loadComponent: () => import('./features/lamination-jobs/lamination-job-form.component').then(c => c.LaminationJobFormComponent),
        title: 'View Lamination Job | CoilManager',
        data: { readOnly: true },
      },
      {
        path: 'lamination-jobs/:id/complete',
        loadComponent: () => import('./features/lamination-jobs/complete-lamination-job.component').then(c => c.CompleteLaminationJobComponent),
        title: 'Complete Lamination Job | CoilManager',
      },      {
        path: 'lamination-jobs/:id/material-allocation',
        loadComponent: () => import('./features/lamination-jobs/lamination-allocation.component').then(c => c.LaminationAllocationComponent),
        title: 'Lamination Material Allocation | CoilManager',
      },
      {
        path: 'lamination-jobs/:id/job-card',
        loadComponent: () => import('./features/lamination-jobs/lamination-job-card.component').then(c => c.LaminationJobCardComponent),
        title: 'Lamination Job Card | CoilManager',
      },
      {
        path: 'lamination-jobs/:id',
        loadComponent: () => import('./features/lamination-jobs/lamination-detail.component').then(c => c.LaminationDetailComponent),
        title: 'Lamination Job | CoilManager',
      },
      {
        path: 'lamination-jobs',
        loadComponent: () => import('./features/lamination-jobs/lamination-job-list.component').then(c => c.LaminationJobListComponent),
        title: 'Lamination Jobs | CoilManager',
      },
      {
        path: 'slitting-jobs/create',
        loadComponent: () =>
          import('./features/slitting-jobs/pages/slitting-job-planning/slitting-job-planning.component').then((component) => component.SlittingJobPlanningComponent),
        title: 'Create Slitting Job | CoilManager',
      },
      {
        path: 'slitting-jobs/:id/labels',
        loadComponent: () => import('./features/slit-coils/label-printing/pages/slit-coil-batch-print-page/slit-coil-batch-print-page.component').then(c => c.SlitCoilBatchPrintPageComponent),
        title: 'Slitting Job Labels | CoilManager',
      },
      {
        path: 'slitting-jobs/:id/edit',
        loadComponent: () =>
          import('./features/slitting-jobs/pages/slitting-job-planning/slitting-job-planning.component').then((component) => component.SlittingJobPlanningComponent),
        title: 'Edit Slitting Job | CoilManager',
      },
      {
        path: 'slitting-jobs/:id/complete',
        loadComponent: () =>
          import('./features/slitting-jobs/pages/complete-slitting/complete-slitting.component').then((component) => component.CompleteSlittingComponent),
        title: 'Complete Slitting | CoilManager',
      },
      {
        path: 'slitting-jobs/:id/job-card',
        loadComponent: () =>
          import('./features/slitting-jobs/job-card/job-card-page/job-card-page.component').then((component) => component.JobCardPageComponent),
        title: 'Job Card | CoilManager',
      },
      {
        path: 'slitting-jobs',
        loadComponent: () =>
          import('./features/slitting-jobs/pages/slitting-job-list/slitting-job-list-page.component').then((component) => component.SlittingJobListPageComponent),
        title: 'Slitting Jobs | CoilManager',
      },
      {
        path: 'slit-coils/labels/batch',
        loadComponent: () => import('./features/slit-coils/label-printing/pages/slit-coil-batch-print-page/slit-coil-batch-print-page.component').then(c => c.SlitCoilBatchPrintPageComponent),
        title: 'Batch Print Slit Coil Labels | CoilManager',
      },
      {
        path: 'slit-coils/:id/label',
        loadComponent: () => import('./features/slit-coils/label-printing/pages/slit-coil-label-page/slit-coil-label-page.component').then(c => c.SlitCoilLabelPageComponent),
        title: 'Slit Coil Label | CoilManager',
      },
      {
        path: 'slit-coils/:id',
        loadComponent: () => import('./features/slit-coils/pages/slit-coil-detail/slit-coil-detail.component').then(c => c.SlitCoilDetailComponent),
        title: 'Coil Details | CoilManager',
      },
      {
        path: 'slit-coils',
        loadComponent: () =>
          import('./features/slit-coils/pages/slit-coil-list/slit-coil-list.component').then((component) => component.SlitCoilListComponent),
        title: 'Slit Coil Inventory | CoilManager',
      },
      {
        path: 'coil-search',
        loadComponent: () => import('./features/coil-search/pages/coil-search/coil-search.component').then(c => c.CoilSearchComponent),
        title: 'Coil Search | CoilManager',
      },
      {
        path: 'coils/:coilNumber/traceability',
        loadComponent: () => import('./features/traceability/pages/coil-traceability/coil-traceability.component').then(c => c.CoilTraceabilityComponent),
        title: 'Coil Traceability | CoilManager',
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
      ...(environment.production ? [] : [{
        path: 'admin/development-tools',
        loadComponent: () => import('./features/development-tools/development-tools.component').then(c => c.DevelopmentToolsComponent),
        title: 'Development Tools | CoilManager',
      }]),
      {
        path: 'admin/analytics',
        loadComponent: () =>
          import('./features/dashboard/pages/admin-analytics/admin-analytics.component').then((component) => component.AdminAnalyticsComponent),
        title: 'Analytics | CoilManager',
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
