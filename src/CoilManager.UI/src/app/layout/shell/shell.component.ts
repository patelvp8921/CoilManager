import { Component, HostListener, inject, signal } from '@angular/core';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatDividerModule } from '@angular/material/divider';
import { MatIconModule } from '@angular/material/icon';
import { MatListModule } from '@angular/material/list';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatTooltipModule } from '@angular/material/tooltip';
import { LayoutStateService } from '../services/layout-state.service';
import { environment } from '../../../environments/environment';

interface NavigationItem { label: string; icon: string; route: string; exact?: boolean; }
interface NavigationGroup { label: string; icon: string; items: readonly NavigationItem[]; }

@Component({
  selector: 'app-shell',
  imports: [RouterLink, RouterLinkActive, RouterOutlet, MatButtonModule, MatDividerModule,
    MatIconModule, MatListModule, MatSidenavModule, MatToolbarModule, MatTooltipModule],
  templateUrl: './shell.component.html',
  styleUrl: './shell.component.scss',
})
export class ShellComponent {
  protected readonly layout = inject(LayoutStateService);
  private readonly router = inject(Router);
  protected readonly expandedGroups = signal(new Set(['Inventory', 'Planning', 'Production', 'Administration']));
  protected readonly primaryItems: readonly NavigationItem[] = [
    { label: 'Dashboard', icon: 'dashboard', route: '/dashboard', exact: true },
    { label: 'Coil Search', icon: 'qr_code_scanner', route: '/coil-search' },
  ];
  protected readonly groups: readonly NavigationGroup[] = [
    { label: 'Inventory', icon: 'inventory_2', items: [
      { label: 'Mother Coils', icon: 'inventory_2', route: '/mother-coils' },
      { label: 'Slit Coils', icon: 'view_list', route: '/slit-coils' },
    ]},
    { label: 'Production', icon: 'precision_manufacturing', items: [
      { label: 'Slitting Jobs', icon: 'precision_manufacturing', route: '/slitting-jobs' },
      { label: 'Lamination Jobs', icon: 'layers', route: '/lamination-jobs' },
    ]},
    { label: 'Planning', icon: 'event_note', items: [
      { label: 'Work Orders', icon: 'assignment', route: '/work-orders' },
    ]},
    { label: 'Administration', icon: 'admin_panel_settings', items: [
      { label: 'Manufacturers', icon: 'factory', route: '/admin/manufacturers' },
      { label: 'Suppliers', icon: 'storefront', route: '/admin/suppliers' },
      { label: 'Grades', icon: 'category', route: '/admin/grades' },
      { label: 'Analytics', icon: 'analytics', route: '/admin/analytics' },
      ...(environment.production ? [] : [{ label: 'Development Tools', icon: 'science', route: '/admin/development-tools' }]),
    ]},
  ];

  protected toggleGroup(label: string): void {
    if (this.layout.effectiveCollapsed()) { this.layout.expandSidebar(); return; }
    const groups = new Set(this.expandedGroups());
    groups.has(label) ? groups.delete(label) : groups.add(label);
    this.expandedGroups.set(groups);
  }

  protected groupExpanded(label: string): boolean { return this.expandedGroups().has(label); }
  protected groupActive(group: NavigationGroup): boolean {
    return group.items.some(item => this.router.url === item.route || this.router.url.startsWith(`${item.route}/`));
  }
  protected routeSelected(): void { this.layout.closeMobile(); }

  @HostListener('document:keydown.escape')
  protected escape(): void {
    if (this.layout.focusMode()) this.layout.exitFocusMode();
    else this.layout.closeMobile();
  }
}
