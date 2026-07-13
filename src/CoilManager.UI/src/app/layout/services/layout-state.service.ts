import { BreakpointObserver } from '@angular/cdk/layout';
import { Injectable, computed, inject, signal } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class LayoutStateService {
  private readonly storageKey = 'coilManager.sidebarCollapsed';
  private readonly breakpointObserver = inject(BreakpointObserver);

  readonly isMobile = signal(false);
  readonly isTablet = signal(false);
  readonly sidebarCollapsed = signal(this.readCollapsedPreference());
  readonly sidebarOpenMobile = signal(false);
  readonly focusMode = signal(false);
  readonly sidebarVisible = computed(() => !this.focusMode() && (!this.isMobile() || this.sidebarOpenMobile()));
  readonly sidenavMode = computed<'over' | 'side'>(() => this.isMobile() ? 'over' : 'side');
  readonly effectiveCollapsed = computed(() => !this.isMobile() && this.sidebarCollapsed());

  constructor() {
    this.breakpointObserver.observe(['(max-width: 767px)', '(min-width: 768px) and (max-width: 1199px)'])
      .subscribe(result => {
        const mobile = result.breakpoints['(max-width: 767px)'];
        const tablet = result.breakpoints['(min-width: 768px) and (max-width: 1199px)'];
        this.isMobile.set(mobile);
        this.isTablet.set(tablet);
        if (mobile) this.sidebarOpenMobile.set(false);
        if (tablet && !this.hasSavedPreference()) this.sidebarCollapsed.set(true);
      });
  }

  toggleSidebar(): void {
    if (this.focusMode()) this.focusMode.set(false);
    if (this.isMobile()) this.sidebarOpenMobile.update(open => !open);
    else this.setCollapsed(!this.sidebarCollapsed());
  }

  expandSidebar(): void { if (!this.isMobile()) this.setCollapsed(false); }
  closeMobile(): void { if (this.isMobile()) this.sidebarOpenMobile.set(false); }
  toggleFocusMode(): void { this.focusMode.update(value => !value); this.sidebarOpenMobile.set(false); }
  exitFocusMode(): void { this.focusMode.set(false); }

  private setCollapsed(value: boolean): void {
    this.sidebarCollapsed.set(value);
    try { localStorage.setItem(this.storageKey, String(value)); } catch { /* storage is optional */ }
  }

  private readCollapsedPreference(): boolean {
    try { return localStorage.getItem(this.storageKey) === 'true'; } catch { return false; }
  }

  private hasSavedPreference(): boolean {
    try { return ['true', 'false'].includes(localStorage.getItem(this.storageKey) ?? ''); } catch { return false; }
  }
}
