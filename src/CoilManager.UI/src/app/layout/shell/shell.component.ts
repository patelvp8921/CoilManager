import { Component } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatDividerModule } from '@angular/material/divider';
import { MatIconModule } from '@angular/material/icon';
import { MatListModule } from '@angular/material/list';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatToolbarModule } from '@angular/material/toolbar';

interface NavigationItem {
  label: string;
  icon: string;
  route: string;
  exact: boolean;
}

interface AdminNavigationItem {
  label: string;
  icon: string;
  route: string;
}

@Component({
  selector: 'app-shell',
  imports: [
    RouterLink,
    RouterLinkActive,
    RouterOutlet,
    MatButtonModule,
    MatDividerModule,
    MatIconModule,
    MatListModule,
    MatSidenavModule,
    MatToolbarModule,
  ],
  templateUrl: './shell.component.html',
  styleUrl: './shell.component.scss',
})
export class ShellComponent {
  protected readonly navigationItems: NavigationItem[] = [
    {
      label: 'Mother Coils',
      icon: 'inventory_2',
      route: '/mother-coils',
      exact: false,
    },
    {
      label: 'Slitting Jobs',
      icon: 'precision_manufacturing',
      route: '/slitting-jobs',
      exact: false,
    },
  ];

  protected readonly adminItems: AdminNavigationItem[] = [
    { label: 'Manufacturers', icon: 'factory', route: '/admin/manufacturers' },
    { label: 'Suppliers', icon: 'storefront', route: '/admin/suppliers' },
    { label: 'Grades', icon: 'category', route: '/admin/grades' },
    { label: 'Analytics', icon: 'analytics', route: '/admin/analytics' },
  ];
}
