import { Component, HostListener, computed, inject, signal } from '@angular/core';
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
import { AuthService } from '../../core/auth/auth.service';

interface NavigationItem { label: string; icon: string; route: string; exact?: boolean; }
interface NavigationGroup { label: string; icon: string; permissionPrefixes: readonly string[]; items: readonly NavigationItem[]; }

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
  protected readonly auth = inject(AuthService);
  protected readonly expandedGroups = signal(new Set<string>());
  protected readonly primaryItems: readonly NavigationItem[] = [
    { label: 'Dashboard', icon: 'dashboard', route: '/dashboard', exact: true },
  ];
  protected readonly groups: readonly NavigationGroup[] = [
    { label: 'Inventory', icon: 'inventory_2', permissionPrefixes: ['Inventory.'], items: [
      { label: 'Mother Coils', icon: 'inventory_2', route: '/mother-coils' },
      { label: 'Slit Coils', icon: 'view_list', route: '/slit-coils' },
    ]},
    { label: 'Production', icon: 'precision_manufacturing', permissionPrefixes: ['Production.'], items: [
      { label: 'Slitting Jobs', icon: 'precision_manufacturing', route: '/slitting-jobs' },
      { label: 'Lamination Jobs', icon: 'layers', route: '/lamination-jobs' },
    ]},
    { label: 'Administration', icon: 'admin_panel_settings', permissionPrefixes: ['Administration.'], items: [
      { label: 'Security & Access', icon: 'shield', route: '/admin/users' },
      { label: 'Manufacturers', icon: 'factory', route: '/admin/manufacturers' },
      { label: 'Suppliers', icon: 'storefront', route: '/admin/suppliers' },
      { label: 'Grades', icon: 'category', route: '/admin/grades' },
      { label: 'Analytics', icon: 'analytics', route: '/admin/analytics' },
      ...(environment.production ? [] : [{ label: 'Development Tools', icon: 'science', route: '/admin/development-tools' }]),
    ]},
    { label: 'Sales', icon: 'request_quote', permissionPrefixes: ['Sales.', 'Customers.', 'SalesOrders.'], items: [
      { label: 'Customers', icon: 'groups', route: '/customers' },
      { label: 'Sales Orders', icon: 'request_quote', route: '/sales-orders' },
    ]},
    { label: 'Planning', icon: 'event_note', permissionPrefixes: ['WorkOrders.'], items: [
      { label: 'Work Orders', icon: 'assignment', route: '/work-orders' },
    ]},
  ];
  protected readonly visibleGroups = computed(() => this.groups.filter(group =>
    group.permissionPrefixes.some(prefix => this.hasPermissionPrefix(prefix))));
  protected readonly reportsVisible = computed(() => this.hasPermissionPrefix('Reports.'));

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
  protected logout(): void { this.auth.logout(); }

  private hasPermissionPrefix(prefix: string): boolean {
    const user = this.auth.user();
    return !!user && (user.roles.includes('Administrator') || user.permissions.some(permission => permission.startsWith(prefix)));
  }

  protected userInitials(): string {
    const parts = (this.auth.user()?.displayName ?? 'User').trim().split(/\s+/).filter(Boolean);
    if (parts.length === 0) return 'U';
    if (parts.length === 1) return parts[0].slice(0, 2).toUpperCase();
    return `${parts[0][0]}${parts[parts.length - 1][0]}`.toUpperCase();
  }

  @HostListener('document:keydown.escape')
  protected escape(): void {
    if (this.layout.focusMode()) this.layout.exitFocusMode();
    else this.layout.closeMobile();
  }
}
