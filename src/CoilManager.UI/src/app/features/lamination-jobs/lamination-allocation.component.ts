import { CommonModule } from '@angular/common';
import { Component, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatTooltipModule } from '@angular/material/tooltip';
import { forkJoin } from 'rxjs';
import { AvailableCoil, LaminationJob, Requirement } from './lamination-job.model';
import { LaminationJobService } from './lamination-job.service';

interface Allocation {
  id: string; slitCoilId: string; slitCoilNumber: string; requiredWidth: number;
  allocatedWeight: number; remainingWeightAfterAllocation: number; status: string | number;
  reservedBy: string; reservedOn: string; remarks?: string; coreLossPerKg: number;
}

@Component({
  selector: 'app-lamination-allocation',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, MatButtonModule, MatCardModule, MatChipsModule,
    MatFormFieldModule, MatIconModule, MatInputModule, MatProgressBarModule, MatSnackBarModule, MatTooltipModule],
  templateUrl: './lamination-allocation.component.html',
  styleUrl: './lamination-allocation.component.scss',
})
export class LaminationAllocationComponent {
  private readonly api = inject(LaminationJobService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly snack = inject(MatSnackBar);
  readonly id = this.route.snapshot.paramMap.get('id')!;

  readonly job = signal<LaminationJob | null>(null);
  readonly requirements = signal<Requirement[]>([]);
  readonly allocations = signal<Allocation[]>([]);
  readonly coils = signal<AvailableCoil[]>([]);
  readonly selectedWidth = signal<number | null>(null);
  readonly loadingPage = signal(true);
  readonly loadingInventory = signal(false);
  readonly actionBusy = signal(false);
  readonly expandedWidths = signal<number[]>([]);
  readonly allocationWeights: Record<string, number> = {};
  searchText = '';
  motherCoilNumber = '';
  warehouse = '';
  minimumAvailableWeight: number | null = null;
  onlyMatching = true;

  readonly statusValue = computed(() => this.toStatus(this.job()?.status));
  readonly canEdit = computed(() => this.statusValue() === 2);
  readonly readOnly = computed(() => this.statusValue() !== 2);
  readonly requiredWeight = computed(() => this.requirements().reduce((sum, r) => sum + (+r.requiredWeight || 0), 0));
  readonly allocatedWeight = computed(() => this.requirements().reduce((sum, r) => sum + (+r.allocatedWeight || 0), 0));
  readonly shortageWeight = computed(() => this.requirements().reduce((sum, r) => sum + (+r.shortageWeight || 0), 0));
  readonly allocatedNoLoadLoss = computed(() => this.activeAllocations().reduce((sum, a) => sum + (+a.allocatedWeight || 0) * (+a.coreLossPerKg || 0) * 1.15, 0));
  readonly allocationPercentage = computed(() => this.requiredWeight() ? Math.min(100, this.allocatedWeight() / this.requiredWeight() * 100) : 0);
  readonly activeAllocations = computed(() => this.allocations().filter(a => this.isActive(a.status)));
  readonly completeRequirements = computed(() => this.requirements().filter(r => r.shortageWeight <= 0).length);
  readonly exactMatches = computed(() => this.activeAllocations().filter(a => Math.abs((this.coilFor(a)?.width ?? a.requiredWidth) - a.requiredWidth) < 0.0001).length);
  readonly toleranceMatches = computed(() => Math.max(0, this.activeAllocations().length - this.exactMatches()));
  readonly selectedRequirement = computed(() => this.requirements().find(r => r.width === this.selectedWidth()) ?? null);
  readonly validationMessage = computed(() => {
    if (!this.requirements().length) return 'No material requirements could be generated from the released Step Schedule.';
    const incomplete = this.requirements().filter(r => r.shortageWeight > 0);
    if (!incomplete.length) return 'All material requirements are fully allocated.';
    return `${incomplete.length} requirement${incomplete.length === 1 ? '' : 's'} has a total shortage of ${this.shortageWeight().toFixed(2)} kg.`;
  });

  constructor() { this.loadPage(); }

  loadPage(): void {
    this.loadingPage.set(true);
    forkJoin({ job: this.api.get(this.id), requirements: this.api.requirements(this.id), allocations: this.api.allocations(this.id) }).subscribe({
      next: ({ job, requirements, allocations }) => {
        this.job.set(job.data); this.requirements.set(requirements.data); this.allocations.set(allocations.data as Allocation[]);
        const current = this.selectedWidth();
        const selected = requirements.data.find(r => r.width === current) ?? requirements.data.find(r => r.shortageWeight > 0) ?? requirements.data[0];
        this.selectedWidth.set(selected?.width ?? null); this.loadingPage.set(false);
        if (selected) this.refreshInventory();
      },
      error: error => { this.loadingPage.set(false); this.showError(error, 'Unable to load Material Allocation.'); }
    });
  }

  selectRequirement(requirement: Requirement): void {
    this.selectedWidth.set(requirement.width); this.resetToRequirement();
  }

  resetToRequirement(): void {
    this.searchText = ''; this.motherCoilNumber = ''; this.warehouse = ''; this.minimumAvailableWeight = 0; this.onlyMatching = true;
    this.refreshInventory();
  }

  clearFilters(): void {
    this.searchText = ''; this.motherCoilNumber = ''; this.warehouse = ''; this.minimumAvailableWeight = null; this.onlyMatching = false;
    this.refreshInventory();
  }

  refreshInventory(): void {
    const requirement = this.selectedRequirement();
    if (!requirement) { this.coils.set([]); return; }
    this.loadingInventory.set(true);
    this.api.available(this.id, {
      search: this.searchText || this.motherCoilNumber,
      width: this.onlyMatching ? requirement.width : undefined,
      thickness: this.onlyMatching ? this.job()?.thickness : undefined,
      availableWeight: this.minimumAvailableWeight ?? 0,
      warehouse: this.warehouse || undefined,
    }).subscribe({
      next: response => {
        this.coils.set(response.data);
        response.data.forEach(c => this.allocationWeights[c.id] = Math.min(c.availableWeight, requirement.shortageWeight));
        this.loadingInventory.set(false);
      },
      error: error => { this.loadingInventory.set(false); this.showError(error, 'Unable to refresh Slit Coil inventory.'); }
    });
  }

  allocate(coil: AvailableCoil): void {
    const requirement = this.selectedRequirement(); const weight = +this.allocationWeights[coil.id];
    if (!this.canEdit() || !requirement || weight <= 0) return;
    if (weight > coil.availableWeight) { this.snack.open(`Allocation cannot exceed available weight of ${coil.availableWeight.toFixed(2)} kg.`, 'Close', { duration: 5000 }); return; }
    if (weight > requirement.shortageWeight) { this.snack.open(`The remaining requirement is only ${requirement.shortageWeight.toFixed(2)} kg.`, 'Close', { duration: 5000 }); return; }
    const partialMatch = !this.isExactMatch(coil);
    const overrideReason = partialMatch ? prompt(`Core loss differs: job ${this.job()!.coreLossPerKg} W/kg, coil ${coil.coreLossPerKg} W/kg. Enter a reason to allocate this Partial Match:`) : null;
    if (partialMatch && !overrideReason?.trim()) return;
    this.actionBusy.set(true);
    this.api.allocate(this.id, { slitCoilId: coil.id, requiredWidth: requirement.width, allocatedWeight: weight, widthMismatchOverride: partialMatch, overrideReason: overrideReason?.trim() }).subscribe({
      next: () => { this.actionBusy.set(false); this.snack.open('Slit Coil weight reserved.', 'Close', { duration: 2500 }); this.reloadAllocationData(); },
      error: error => { this.actionBusy.set(false); this.showError(error, 'Unable to reserve Slit Coil weight.'); }
    });
  }

  edit(allocation: Allocation): void {
    if (!this.canEdit()) return;
    const entered = prompt(`Update allocated weight for ${allocation.slitCoilNumber} (kg)`, allocation.allocatedWeight.toString());
    if (entered === null) return; const weight = +entered;
    if (!Number.isFinite(weight) || weight <= 0) { this.snack.open('Allocated weight must be greater than zero.', 'Close', { duration: 4000 }); return; }
    this.actionBusy.set(true);
    this.api.updateAllocation(this.id, allocation.id, { allocatedWeight: weight }).subscribe({
      next: () => { this.actionBusy.set(false); this.snack.open('Allocation updated.', 'Close', { duration: 2500 }); this.reloadAllocationData(); },
      error: error => { this.actionBusy.set(false); this.showError(error, 'Unable to update allocation.'); }
    });
  }
  remove(allocation: Allocation): void {
    if (!this.canEdit() || !confirm(`Release ${allocation.allocatedWeight.toFixed(2)} kg from ${allocation.slitCoilNumber} for the ${allocation.requiredWidth} mm requirement?`)) return;
    this.actionBusy.set(true);
    this.api.releaseAllocation(this.id, allocation.id).subscribe({
      next: () => { this.actionBusy.set(false); this.snack.open('Allocation released.', 'Close', { duration: 2500 }); this.reloadAllocationData(); },
      error: error => { this.actionBusy.set(false); this.showError(error, 'Unable to release allocation.'); }
    });
  }

  skipAllocation(): void {
    if (!this.canEdit() || !confirm('Skip material allocation and print a Job Card with blank rows for the operator to complete? No inventory will be reserved.')) return;
    this.actionBusy.set(true);
    this.api.skipAllocation(this.id).subscribe({
      next: () => { this.actionBusy.set(false); this.snack.open('Material allocation skipped.', 'Close', { duration: 2500 }); this.router.navigate(['/lamination-jobs']); },
      error: error => { this.actionBusy.set(false); this.showError(error, 'Unable to skip material allocation.'); }
    });
  }
  saveAllocations(): void {
    this.actionBusy.set(true);
    forkJoin({ requirements: this.api.requirements(this.id), allocations: this.api.allocations(this.id) }).subscribe({
      next: () => {
        this.actionBusy.set(false);
        this.snack.open('Allocations saved successfully.', 'Close', { duration: 2500 });
        this.router.navigate(['/lamination-jobs']);
      },
      error: error => { this.actionBusy.set(false); this.showError(error, 'Unable to save allocations.'); }
    });
  }

  confirmAllocation(): void {
    if (!this.canEdit() || this.shortageWeight() > 0 || !this.activeAllocations().length) return;
    this.actionBusy.set(true);
    this.api.confirm(this.id).subscribe({
      next: () => { this.actionBusy.set(false); this.snack.open('Material allocation confirmed successfully.', 'Close', { duration: 3500 }); this.loadPage(); },
      error: error => { this.actionBusy.set(false); this.showError(error, 'Unable to confirm allocation.'); }
    });
  }

  toggleBreakdown(width: number): void {
    const open = this.expandedWidths(); this.expandedWidths.set(open.includes(width) ? open.filter(x => x !== width) : [...open, width]);
  }
  isExpanded(width: number): boolean { return this.expandedWidths().includes(width); }
  requirementStatus(r: Requirement): string { return r.allocatedWeight <= 0 ? 'Not Allocated' : r.shortageWeight > 0 ? 'Partial' : 'Complete'; }
  requirementClass(r: Requirement): string { return r.allocatedWeight <= 0 ? 'missing' : r.shortageWeight > 0 ? 'partial' : 'complete'; }
  allocationStatus(value: string | number): string { return typeof value === 'number' ? ['Reserved', 'Partially Consumed', 'Consumed', 'Released'][value] ?? 'Unknown' : `${value}`.replace('PartiallyConsumed', 'Partially Consumed'); }
  coilFor(a: Allocation): AvailableCoil | undefined { return this.coils().find(c => c.id === a.slitCoilId); }
  isExactMatch(coil: AvailableCoil): boolean {
    const job = this.job(); const requirement = this.selectedRequirement();
    return !!job && !!requirement && coil.thickness === job.thickness && Math.abs(coil.width - requirement.width) <= 0.1 && Math.abs(coil.coreLossPerKg - job.coreLossPerKg) < 0.0001 && coil.availableWeight > 0;
  }
  matchStatus(coil: AvailableCoil): string {
    const job = this.job(); const requirement = this.selectedRequirement();
    if (!job || !requirement || Math.abs(coil.width - requirement.width) > 0.1) return 'Width Mismatch';
    if (coil.thickness !== job.thickness) return 'Thickness Mismatch';
    if (coil.availableWeight <= 0) return 'No Available Weight';
    return this.isExactMatch(coil) ? 'Exact Match' : 'Partial Match';
  }
  isEligible(coil: AvailableCoil): boolean { const job = this.job(); const requirement = this.selectedRequirement(); return !!job && !!requirement && coil.thickness === job.thickness && Math.abs(coil.width - requirement.width) <= 0.1 && coil.availableWeight > 0; }  designName(value: unknown): string { return value === 1 || value === 'StepLap' ? 'Step Lap' : 'Simple'; }
  statusName(): string { return ['Draft', 'In Progress', 'Released', 'Legacy In Progress', 'Completed', 'Cancelled'][this.statusValue()] ?? 'Unknown'; }
  private toStatus(value: unknown): number { return typeof value === 'number' ? value : ['Draft', 'Allocated', 'Released', 'InProgress', 'Completed', 'Cancelled'].indexOf(`${value}`); }
  private isActive(value: string | number): boolean { return value === 0 || value === 'Reserved'; }
  private reloadAllocationData(): void {
    forkJoin({ requirements: this.api.requirements(this.id), allocations: this.api.allocations(this.id) }).subscribe({
      next: ({ requirements, allocations }) => { this.requirements.set(requirements.data); this.allocations.set(allocations.data as Allocation[]); this.refreshInventory(); },
      error: error => this.showError(error, 'Unable to refresh allocation totals.')
    });
  }
  private showError(error: any, fallback: string): void {
    const body = error?.error; const details = body?.errors ? Object.values(body.errors).flat().join(' | ') : null;
    this.snack.open(details || body?.message || body?.title || fallback, 'Close', { duration: 6000 });
  }
}