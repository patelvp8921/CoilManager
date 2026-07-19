import { CommonModule } from '@angular/common';
import { Component, OnDestroy, inject, signal } from '@angular/core';
import { AbstractControl, FormArray, FormBuilder, FormGroup, ReactiveFormsModule, ValidationErrors, ValidatorFn, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatNativeDateModule } from '@angular/material/core';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MAT_FORM_FIELD_DEFAULT_OPTIONS, MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatTabsModule } from '@angular/material/tabs';
import { MatTooltipModule } from '@angular/material/tooltip';
import { Subscription } from 'rxjs';
import { LookupService } from '../../shared/services/lookup.service';
import { LaminationJobService } from './lamination-job.service';
import { Dimension, LaminationJob, Plate, PlateType, Step } from './lamination-job.model';
import { PlateFieldDefinition, PlateReferencePanelComponent } from './components/plate-reference-panel/plate-reference-panel.component';

type PlateForm = FormGroup;
type StepForm = FormGroup;
const TYPES: PlateType[] = ['Side', 'Center', 'Top', 'Bottom'];

@Component({
  selector: 'app-lamination-form',
  imports: [CommonModule, ReactiveFormsModule, RouterLink, MatButtonModule, MatCardModule, MatChipsModule,
    MatDatepickerModule, MatNativeDateModule, MatFormFieldModule, MatIconModule, MatInputModule, MatSelectModule, MatSnackBarModule, MatTabsModule, MatTooltipModule,
    PlateReferencePanelComponent],
  providers: [{ provide: MAT_FORM_FIELD_DEFAULT_OPTIONS, useValue: { appearance: 'outline', subscriptSizing: 'dynamic' } }],
  templateUrl: './lamination-job-form.component.html', styleUrl: './lamination-job-form.component.scss',
})
export class LaminationJobFormComponent implements OnDestroy {
  private readonly fb = inject(FormBuilder);
  private readonly api = inject(LaminationJobService);
  private readonly lookups = inject(LookupService);
  private readonly route = inject(ActivatedRoute);
  private readonly snackBar = inject(MatSnackBar);
  protected readonly router = inject(Router);
  private readonly subscriptions = new Subscription();
  private readonly crgoDensityKgPerCubicMeter = 7650;
  private readonly cubicMillimetersPerCubicMeter = 1_000_000_000;
  private readonly id = this.route.snapshot.paramMap.get('id');

  protected readonly number = signal('Loading…');
  protected readonly grades = signal<any[]>([]);
  protected readonly saving = signal(false);
  protected readonly focus = signal(false);
  protected readonly activeTab = signal(0);
  protected readonly saveAttempted = signal(false);
  protected readonly selectedStep = signal(0);
  protected readonly expandedRow = signal<{ tab: number; step: number } | null>(null);
  protected readonly expandedSteps = signal<number[]>([0]);
  protected readonly allocatedWeight = signal(0);
  protected readonly loaded = signal(!this.id);
  protected drawing?: File;

  protected readonly references: { combined: { image: string; title: string; fields: PlateFieldDefinition[]; note: string }; topBottom: { image: string; title: string; fields: PlateFieldDefinition[]; note: string }; side: { image: string; title: string; fields: PlateFieldDefinition[]; note: string }; center: { image: string; title: string; fields: PlateFieldDefinition[]; note: string } } = {
    combined: { image: 'assets/images/lamination-profiles/complete-core-profile.svg', title: 'Complete Core Plate Assembly', fields: [
      { label: 'Top-Bottom', description: 'Shared Width, Height and planned Weight for the Top and Bottom yokes' }, { label: 'Side', description: 'Width, Height and planned Weight for both outer limbs' }, { label: 'Center', description: 'Width, Height and planned Weight for the center limb' }], note: 'The diagram shows how Top-Bottom, Side and Center plates combine to form the complete core.' },
    topBottom: { image: 'assets/images/lamination-profiles/top-bottom-plate-profile-l1-l2.png', title: 'Top-Bottom Plate Profile', fields: [
      { label: 'Width', description: 'Plate width' }, { label: 'Height', description: 'Plate height' }, { label: 'Weight', description: 'Planned plate weight' }], note: 'Top and Bottom dimensions are identical. The 45-degree notch is standard and quantity is doubled.' },
    side: { image: 'assets/images/lamination-profiles/side-plate-profile-generated.png', title: 'Side Plate Profile', fields: [
      { label: 'Width', description: 'Plate width' }, { label: 'Height', description: 'Plate height' }, { label: 'Weight', description: 'Planned plate weight' }], note: 'Side plate end profile and 45-degree cut are standard. Left and Right quantities each equal Stack Quantity, so the combined Side quantity is doubled.' },
    center: { image: 'assets/images/lamination-profiles/center-plate-profile-generated.png', title: 'Center Plate Profile', fields: [
      { label: 'Width', description: 'Center plate width' }, { label: 'Height', description: 'Plate height' }, { label: 'Weight', description: 'Planned plate weight' }], note: 'Center plate profile is standard. Enter only width, length, stack quantity, and planned weight.' },
  };

  private readonly stepArray = new FormArray<StepForm>([], this.uniqueStepsValidator());
  protected readonly form = this.fb.group({
    jobOrDrawingNumber: [''], customer: [''],
    rating: ['', Validators.required], gradeId: ['', Validators.required], designType: ['Simple'],
    stepLapOrientation: ['NotApplicable'], numberOfSteps: [1, [Validators.required, Validators.min(1)]],
    customerCategory: ['', Validators.required], totalWeight: [null, [Validators.required, Validators.min(0.01)]],
    customerCoreLossPerKg: [null, [Validators.required, Validators.min(0.0001)]],
    plannedDate: [new Date(), Validators.required], requiredDate: [null as Date | null],
    shift: [''], plannerName: [''], remarks: [''], rowVersion: [''], steps: this.stepArray,
  });
  protected get steps(): FormArray<StepForm> { return this.stepArray; }
  readonly isEdit = !!this.id;
  protected get isDirty(): boolean { return this.form.dirty; }
  protected filteredGrades(): any[] { const category=this.form.value.customerCategory; const loss=+(this.form.value.customerCoreLossPerKg??0); return this.grades().filter(g=>(!category||g.category===category)&&(!loss||+g.coreLossPerKg<=loss)); }
  protected noLoadLoss(): number { return +(this.form.value.totalWeight??0) * +(this.form.value.customerCoreLossPerKg??0) * 1.15; }
  protected materialCriteriaChanged(): void { const selected=this.grades().find(g=>g.id===this.form.value.gradeId); if(selected&&!this.filteredGrades().some(g=>g.id===selected.id))this.form.controls.gradeId.setValue(''); }

  constructor() {
    this.lookups.getGrades().subscribe((response: any) => { this.grades.set(response.data ?? response); this.recalculatePlateWeights(); });
    this.subscriptions.add(this.form.valueChanges.subscribe(() => this.recalculatePlateWeights()));
    if (this.id) this.api.get(this.id).subscribe(response => this.loadJob(response.data));
    else { this.api.next().subscribe(response => this.number.set(response.data)); this.addStep(false); }
  }

  ngOnDestroy(): void { this.subscriptions.unsubscribe(); }

  private positive(required = true): ValidatorFn[] { return required ? [Validators.required, Validators.min(0.01)] : [Validators.min(0)]; }
  private createDimension(value?: Partial<Dimension>, valueRequired = true): FormGroup {
    return this.fb.group({ dimensionCode: [value?.dimensionCode ?? '', Validators.required], displayName: [value?.displayName ?? ''],
      dimensionValue: [value?.dimensionValue ?? null, valueRequired ? [Validators.required, Validators.min(0)] : [Validators.min(0)]], unit: [value?.unit ?? 'mm'],
      sequence: [value?.sequence ?? 1], remarks: [value?.remarks ?? ''] });
  }
  private createPlate(type: PlateType, value?: Partial<Plate>): PlateForm {
    const rawDimensions = value?.dimensions ?? [];
    const dimensions = new FormArray<FormGroup>(rawDimensions.map(d => this.createDimension(d, false)));
    return this.fb.group({ plateType: [type], width: [value?.width ?? null, this.positive()],
      length: [value?.length ?? null, this.positive()], quantity: [value?.quantity ?? 1, [Validators.required, Validators.min(1), Validators.max(999)]],
      plannedWeight: [value?.plannedWeight ?? 0, [Validators.required, Validators.min(0)]], remarks: [value?.remarks ?? ''], dimensions });
  }
  private createStep(value?: Partial<Step>): StepForm {
    const plates = new FormArray<PlateForm>(TYPES.map(type => this.createPlate(type, value?.plates?.find(p => p.plateType === type))));
    return this.fb.group({ stepNumber: [value?.stepNumber ?? this.steps.length + 1, [Validators.required, Validators.min(1)]],
      stackQuantity: [value?.stackQuantity ?? 1, [Validators.required, Validators.min(1), Validators.max(999)]],
      sequence: [value?.sequence ?? this.steps.length + 1], width: [value?.width ?? 1, this.positive()],
      plannedWeight: [value?.plannedWeight ?? 0], remarks: [value?.remarks ?? ''], plates });
  }
  private uniqueStepsValidator(): ValidatorFn { return (control: AbstractControl): ValidationErrors | null => {
    const values = (control as FormArray).controls.map(c => c.get('stepNumber')?.value).filter(v => v != null);
    return new Set(values).size === values.length ? null : { duplicateStepNumbers: true };
  }; }

  protected plates(stepIndex: number): FormArray<PlateForm> { return this.steps.at(stepIndex).get('plates') as FormArray<PlateForm>; }
  protected plate(stepIndex: number, type: PlateType): PlateForm { return this.plates(stepIndex).at(TYPES.indexOf(type)); }
  protected dimensions(stepIndex: number, type: PlateType): FormArray<FormGroup> { return this.plate(stepIndex, type).get('dimensions') as FormArray<FormGroup>; }
  protected dimension(stepIndex: number, type: PlateType, code: string): AbstractControl | null {
    return this.dimensions(stepIndex, type).controls.find(d => `${d.value.dimensionCode}`.toUpperCase() === code)?.get('dimensionValue') ?? null;
  }
  protected ensureDimension(stepIndex: number, type: PlateType, code?: string): void {
    const dims = this.dimensions(stepIndex, type); dims.push(this.createDimension({ dimensionCode: code ?? '', displayName: code, sequence: dims.length + 1 }));
  }
  protected removeDimension(stepIndex: number, type: PlateType, index: number): void { this.dimensions(stepIndex, type).removeAt(index); }

  protected addStep(markDirty = true): void {
    if (this.form?.value.designType === 'Simple' && this.steps.length) return;
    this.steps.push(this.createStep()); this.renumber();
    if (markDirty) this.form.markAsDirty();
  }
  protected duplicateStep(index: number): void {
    const copy = structuredClone(this.steps.at(index).getRawValue()); copy.stepNumber = this.steps.length + 1; copy.sequence = this.steps.length + 1;
    this.steps.push(this.createStep(copy)); this.renumber(); this.form.markAsDirty();
  }
  protected removeStep(index: number): void {
    if (this.steps.length === 1 || !window.confirm(`Delete Step ${this.steps.at(index).value.stepNumber} from all plate tabs?`)) return;
    this.steps.removeAt(index); this.renumber(); this.form.markAsDirty();
  }
  protected moveStep(index: number, delta: number): void {
    const target = index + delta; if (target < 0 || target >= this.steps.length) return;
    const moving = this.steps.at(index); this.steps.removeAt(index); this.steps.insert(target, moving); this.renumber(); this.form.markAsDirty();
  }
  private renumber(): void {
    this.steps.controls.forEach((step, i) => step.patchValue({ stepNumber: i + 1, sequence: i + 1 }, { emitEvent: false }));
    this.form.controls.numberOfSteps.setValue(this.steps.length, { emitEvent: false }); this.steps.controls.forEach((_,index)=>this.syncStackQuantity(index)); this.steps.updateValueAndValidity();
  }
  protected copyTop(stepIndex: number): void {
    const top = structuredClone(this.plate(stepIndex, 'Top').getRawValue()); top.plateType = 'Bottom';
    this.plates(stepIndex).setControl(TYPES.indexOf('Bottom'), this.createPlate('Bottom', top)); this.form.markAsDirty();
  }
  protected copyTopAll(): void { this.steps.controls.forEach((_, i) => this.copyTop(i)); }
  protected syncTopBottomWidth(stepIndex: number): void { this.plate(stepIndex, 'Bottom').get('width')?.setValue(this.plate(stepIndex, 'Top').value.width); }

  protected designChanged(): void {
    if (this.form.value.designType === 'Simple') { while (this.steps.length > 1) this.steps.removeAt(this.steps.length - 1); this.form.patchValue({ numberOfSteps: 1, stepLapOrientation: 'NotApplicable' }); }
    else { if (this.steps.length === 1) this.steps.push(this.createStep()); this.form.patchValue({ numberOfSteps: this.steps.length, stepLapOrientation: 'HorizontalAndVertical' }); }
    this.renumber();
  }
  protected generateSteps(): void {
    if (this.form.value.designType === 'Simple') { this.form.controls.numberOfSteps.setValue(1); return; }
    const requested = Math.max(2, Math.floor(+(this.form.controls.numberOfSteps.value ?? 2)));
    while (this.steps.length < requested) this.steps.push(this.createStep());
    while (this.steps.length > requested) this.steps.removeAt(this.steps.length - 1);
    this.renumber(); this.form.markAsDirty();
  }
  protected stepValid(index: number): boolean { return this.steps.at(index).valid && this.plates(index).controls.every(p => p.valid); }
  protected stepStatus(index: number): 'Complete' | 'Incomplete' | 'Error' {
    const step = this.steps.at(index); return step.valid ? 'Complete' : (step.dirty || this.saveAttempted()) ? 'Error' : 'Incomplete';
  }
  protected completedSteps(): number { return this.steps.controls.filter((_, i) => this.stepValid(i)).length; }
  protected plateTotal(type: PlateType, field: 'quantity' | 'plannedWeight'): number { return this.steps.controls.reduce((sum, _, i) => sum + (+this.plate(i, type).value[field] || 0), 0); }
  protected totalPieces(): number { return TYPES.reduce((sum, type) => sum + this.plateTotal(type, 'quantity'), 0); }
  protected totalWeight(): number { return TYPES.reduce((sum, type) => sum + this.plateTotal(type, 'plannedWeight'), 0); }
  protected totalStackQuantity(): number { return this.steps.controls.reduce((sum, step) => sum + (+step.value.stackQuantity || 0), 0); }
  protected uniqueWidths(): number { return new Set(this.steps.controls.flatMap((_, i) => TYPES.map(t => +this.plate(i, t).value.width).filter(Boolean))).size; }
  protected warningCount(): number { return this.steps.controls.filter((_, i) => !this.stepValid(i)).length + (this.steps.hasError('duplicateStepNumbers') ? 1 : 0); }

  protected selectStep(index: number): void { this.selectedStep.set(index); }
  protected selectedValid(): boolean { return this.selectedStep() >= 0 && this.selectedStep() < this.steps.length; }
  protected canAddStep(): boolean { return this.form.value.designType !== 'Simple' && this.steps.length < 12; }
  protected duplicateSelected(): void { if (this.selectedValid()) { this.duplicateStep(this.selectedStep()); this.selectedStep.set(this.steps.length - 1); } }
  protected deleteSelected(): void { if (this.selectedValid()) { this.removeStep(this.selectedStep()); this.selectedStep.set(Math.max(0, Math.min(this.selectedStep(), this.steps.length - 1))); } }
  protected toggleDimensions(tab: number, step: number): void { const open=this.expandedRow(); this.expandedRow.set(open?.tab===tab&&open.step===step?null:{tab,step}); }
  protected dimensionsExpanded(tab: number, step: number): boolean { const open=this.expandedRow(); return open?.tab===tab&&open.step===step; }
  protected collapseDimensions(): void { this.expandedRow.set(null); }
  protected isStepExpanded(index: number): boolean { return this.expandedSteps().includes(index); }
  protected toggleStep(index: number): void { const rows=this.expandedSteps(); this.expandedSteps.set(rows.includes(index)?rows.filter(x=>x!==index):[...rows,index]); this.selectedStep.set(index); }
  protected expandAll(): void { this.expandedSteps.set(this.steps.controls.map((_,index)=>index)); }
  protected collapseAll(): void { this.expandedSteps.set([]); }
  protected dimensionValue(step: number,type: PlateType,code: string): number|null { const value=this.dimension(step,type,code)?.value; return value===null||value===''||value===undefined?null:+value; }
  protected centerQuantity(index: number): number { return +this.steps.at(index).value.stackQuantity||0; }
  protected sidePieces(): number { return this.steps.controls.reduce((sum,_,i)=>sum+this.sideTotalQuantity(i),0); }
  protected centerPieces(): number { return this.steps.controls.reduce((sum,_,i)=>sum+this.centerQuantity(i),0); }

  private relevantPlates(index: number, tab: number): PlateForm[] { const types: PlateType[][]=[['Top','Bottom'],['Side'],['Center']]; return types[tab].map(type=>this.plate(index,type)); }
  private hasEnteredInvalid(control: AbstractControl): boolean {
    if (control instanceof FormGroup || control instanceof FormArray) return Object.values(control.controls).some(child=>this.hasEnteredInvalid(child));
    return control.invalid && control.value !== null && control.value !== '';
  }
  protected plateRowStatus(index: number, tab: number): 'Complete'|'Incomplete'|'Error' {
    const step=this.steps.at(index); const controls=[step.get('stackQuantity')!,...this.relevantPlates(index,tab)];
    if (controls.every(control=>control.valid)) return 'Complete';
    return controls.some(control=>this.hasEnteredInvalid(control)) ? 'Error' : 'Incomplete';
  }
  protected tabStatus(tab: number): 'Complete'|'Incomplete'|'Error' {
    if (this.steps.length !== +(this.form.value.numberOfSteps ?? 0)) return 'Error';
    const states=this.steps.controls.map((_,index)=>this.plateRowStatus(index,tab));
    return states.every(state=>state==='Complete') ? 'Complete' : states.some(state=>state==='Error') ? 'Error' : 'Incomplete';
  }
  protected statusIcon(status: string): string { return status==='Complete'?'check_circle':status==='Error'?'error':'warning'; }

  protected syncTopBottom(index: number): void {
    const top=this.plate(index,'Top'); const bottom=this.plate(index,'Bottom'); const quantity=+(this.steps.at(index).value.stackQuantity??0);
    top.get('quantity')?.setValue(quantity,{emitEvent:false});
    bottom.patchValue({width:top.value.width,length:top.value.length,plannedWeight:top.value.plannedWeight,quantity},{emitEvent:false});
  }
  protected syncStackQuantity(index: number): void {
    this.syncTopBottom(index); const stack=+(this.steps.at(index).value.stackQuantity??0);
    this.plate(index,'Side').get('quantity')?.setValue(stack * 2,{emitEvent:false});
    this.plate(index,'Center').get('quantity')?.setValue(stack,{emitEvent:false});
  }
  private recalculatePlateWeights(): void {
    const grade = this.grades().find(item => item.id === this.form.controls.gradeId.value);
    const thickness = +(grade?.thicknessMm ?? grade?.thickness ?? 0);
    if (!thickness) return;

    this.steps.controls.forEach((step, index) => {
      const stackQuantity = +(step.get('stackQuantity')?.value ?? 0);
      const quantities: Record<PlateType, number> = {
        Top: stackQuantity,
        Bottom: stackQuantity,
        Side: stackQuantity * 2,
        Center: stackQuantity,
      };

      TYPES.forEach(type => {
        const plate = this.plate(index, type);
        const width = +(plate.get('width')?.value ?? 0);
        const length = +(plate.get('length')?.value ?? 0);
        const quantity = quantities[type];
        const weight = width > 0 && length > 0 && quantity > 0
          ? Math.round(width * length * thickness * quantity * this.crgoDensityKgPerCubicMeter / this.cubicMillimetersPerCubicMeter * 1000) / 1000
          : 0;
        plate.patchValue({ quantity, plannedWeight: weight }, { emitEvent: false });
      });

      const stepWeight = TYPES.reduce((sum, type) => sum + (+this.plate(index, type).get('plannedWeight')?.value || 0), 0);
      step.get('plannedWeight')?.setValue(Math.round(stepWeight * 1000) / 1000, { emitEvent: false });
    });
  }
  protected sideTotalQuantity(index: number): number { return (+this.steps.at(index).value.stackQuantity||0)*2; }
  protected topBottomQuantity(index: number): number { return (+this.steps.at(index).value.stackQuantity||0)*2; }
  protected topBottomWeight(index: number): number { return (+this.plate(index,'Top').value.plannedWeight||0)*2; }
  protected topBottomWeightPerType(): number { return this.steps.controls.reduce((sum,_,i)=>sum+(+this.plate(i,'Top').value.plannedWeight||0),0); }
  protected topBottomPieces(): number { return this.steps.controls.reduce((sum,_,i)=>sum+this.topBottomQuantity(i),0); }
  protected allocationPercentage(): number { const required=this.totalWeight(); return required?Math.min(100,(this.allocatedWeight()/required)*100):0; }
  protected allocationStatus(): 'Pending'|'Partial'|'Complete' { const p=this.allocationPercentage(); return p>=100?'Complete':p>0?'Partial':'Pending'; }
  protected overallValidation(): 'Ready'|'Incomplete'|'Errors Found' { const states=[0,1,2].map(tab=>this.tabStatus(tab)); return states.some(s=>s==='Error')?'Errors Found':states.every(s=>s==='Complete')?'Ready':'Incomplete'; }
  protected incompleteRowCount(): number { return this.steps.controls.filter((_,i)=>[0,1,2].some(tab=>this.plateRowStatus(i,tab)!=='Complete')).length; }
  private invalidJobFieldCount(): number {
    return Object.entries(this.form.controls).filter(([name, control]) => name !== 'steps' && control.invalid).length;
  }
  protected footerMessage(): string {
    const jobFields = this.invalidJobFieldCount();
    if (jobFields) return `${jobFields} required job field${jobFields === 1 ? '' : 's'} incomplete`;
    const rows = this.incompleteRowCount();
    return rows ? `${rows} incomplete step row${rows === 1 ? '' : 's'}` : 'Ready for Material Allocation';
  }

  protected saveAndRelease(): void { this.save(false, true); }
  protected save(allocateAfterSave = false, releaseAfterSave = false): void {
    this.saveAttempted.set(true); this.form.markAllAsTouched();
    if (this.form.invalid || this.steps.length !== +(this.form.value.numberOfSteps ?? 0)) { this.handleInvalidSave(); return; }
    this.saving.set(true);
    this.steps.controls.forEach((_,index)=>this.syncStackQuantity(index));
    const raw = this.form.getRawValue();
    const steps = raw.steps.map(step => ({
      ...step,
      stepNumber: Number(step['stepNumber']),
      stackQuantity: Number(step['stackQuantity']),
      sequence: Number(step['sequence']),
      width: Number(step['width']),
      plannedWeight: Number(step['plannedWeight']),
      plates: (step['plates'] as any[]).map((plate: any) => ({
        ...plate,
        plateType: this.plateTypeApiValue(plate.plateType),
        width: Number(plate.width),
        length: plate.length === null || plate.length === '' ? null : Number(plate.length),
        quantity: Number(plate.quantity),
        plannedWeight: Number(plate.plannedWeight),
        dimensions: (plate.dimensions as any[])
          .filter((dimension: any) => !!dimension.dimensionCode?.trim() && dimension.dimensionValue !== null && dimension.dimensionValue !== undefined)
          .map((dimension: any) => ({ ...dimension, dimensionValue: Number(dimension.dimensionValue), sequence: Number(dimension.sequence) })),
      })),
    }));
    const payload = { ...raw, designType: this.designTypeApiValue(raw.designType), stepLapOrientation: this.orientationApiValue(raw.stepLapOrientation), steps, plannedDate: this.toApiDate(raw.plannedDate), requiredDate: this.toApiDate(raw.requiredDate) };
    const request = this.id ? this.api.update(this.id, payload) : this.api.create(payload);
    request.subscribe({ next: response => {
      const complete = () => {
        this.form.controls.rowVersion.setValue(response.data.rowVersion ?? '', { emitEvent: false });
        this.form.markAsPristine();
        this.saving.set(false);
        if (releaseAfterSave) {
          this.api.release(response.data.id).subscribe({
            next: () => { this.saving.set(false); this.snackBar.open('Lamination Job released successfully.', 'Close', { duration: 3000 }); this.router.navigate(['/lamination-jobs']); },
            error: error => { this.saving.set(false); this.snackBar.open(this.apiErrorMessage(error, 'Release Job failed.'), 'Close', { duration: 6000 }); }
          });
          return;
        }
        if (!allocateAfterSave) { this.router.navigate(['/lamination-jobs']); return; }
        this.router.navigate(['/lamination-jobs', response.data.id, 'allocations']);
      };
      if (this.drawing) {
        this.api.upload(response.data.id, this.drawing).subscribe({ next: () => { this.drawing = undefined; complete(); }, error: error => { this.saving.set(false); this.snackBar.open(this.apiErrorMessage(error, 'Job saved, but the drawing could not be uploaded.'), 'Close', { duration: 6000 }); } });
      } else complete();
    }, error: error => { this.saving.set(false); this.snackBar.open(this.apiErrorMessage(error, 'Save Draft failed.'), 'Close', { duration: 6000 }); } });
  }

  private designTypeApiValue(value: unknown): number { return value === 'StepLap' || value === 1 ? 1 : 0; }
  private orientationApiValue(value: unknown): number {
    if (typeof value === 'number') return value;
    return ({ NotApplicable: 0, Horizontal: 1, Vertical: 2, HorizontalAndVertical: 3 } as Record<string, number>)[`${value}`] ?? 0;
  }
  private plateTypeApiValue(value: unknown): number {
    if (typeof value === 'number') return value;
    return ({ Side: 0, Center: 1, Top: 2, Bottom: 3 } as Record<string, number>)[`${value}`] ?? 0;
  }
  private designTypeFormValue(value: unknown): 'Simple' | 'StepLap' { return value === 1 || value === 'StepLap' ? 'StepLap' : 'Simple'; }
  private orientationFormValue(value: unknown): string {
    if (typeof value === 'string') return value;
    return ['NotApplicable', 'Horizontal', 'Vertical', 'HorizontalAndVertical'][Number(value)] ?? 'NotApplicable';
  }
  private plateTypeFormValue(value: unknown): PlateType {
    if (typeof value === 'string') return value as PlateType;
    return (['Side', 'Center', 'Top', 'Bottom'][Number(value)] ?? 'Side') as PlateType;
  }
  private apiErrorMessage(error: any, fallback: string): string {
    const validationErrors = error?.error?.errors;
    if (validationErrors && typeof validationErrors === 'object') {
      const details = Object.entries(validationErrors)
        .flatMap(([field, messages]) => (Array.isArray(messages) ? messages : [messages])
          .filter(message => typeof message === 'string')
          .map(message => `${field}: ${message}`));
      if (details.length) return details.slice(0, 2).join(' | ');
    }
    return error?.error?.message ?? error?.error?.detail ?? error?.error?.title ?? fallback;
  }
  private handleInvalidSave(): void {
    this.saveAttempted.set(true);
    if (this.invalidJobFieldCount()) {
      this.snackBar.open(this.footerMessage(), 'Close', { duration: 4000 });
      setTimeout(() => {
        const target = document.querySelector<HTMLElement>('.form-section:not(.schedule-section) input.ng-invalid, .form-section:not(.schedule-section) .ng-invalid input, .form-section:not(.schedule-section) .ng-invalid [role="combobox"]');
        target?.focus();
        target?.scrollIntoView({ behavior: 'smooth', block: 'center' });
      });
      return;
    }

    const tab=this.firstErrorTab();
    const row=this.steps.controls.findIndex((_,index)=>this.plateRowStatus(index,tab)!=='Complete');
    const invalidRow=Math.max(0,row); this.selectedStep.set(invalidRow); this.expandedSteps.set([...new Set([...this.expandedSteps(),invalidRow])]); this.snackBar.open(this.footerMessage(),'Close',{duration:4000});
    setTimeout(()=>{ const target=document.querySelector<HTMLElement>(`[data-step-index="${invalidRow}"] .ng-invalid input, [data-step-index="${invalidRow}"] input.ng-invalid`); target?.focus(); target?.scrollIntoView({behavior:'smooth',block:'center'}); });
  }
  private firstErrorTab(): number {
    for (let i = 0; i < this.steps.length; i++) { if (this.plate(i, 'Top').invalid || this.plate(i, 'Bottom').invalid) return 0; }
    for (let i = 0; i < this.steps.length; i++) if (this.plate(i, 'Side').invalid) return 1;
    for (let i = 0; i < this.steps.length; i++) if (this.plate(i, 'Center').invalid) return 2; return this.activeTab();
  }
  private loadJob(job: LaminationJob): void {
    this.number.set(job.laminationJobNumber); this.allocatedWeight.set(job.totalAllocatedWeight ?? 0); this.steps.clear();
    job.steps.forEach(step => this.steps.push(this.createStep({ ...step, plates: step.plates.map(plate => ({ ...plate, plateType: this.plateTypeFormValue(plate.plateType) })) })));
    this.steps.controls.forEach((_,index)=>this.syncStackQuantity(index));
    this.form.patchValue({ ...job, designType: this.designTypeFormValue(job.designType), stepLapOrientation: this.orientationFormValue(job.stepLapOrientation), customerCategory: job.category, plannedDate: this.fromApiDate(job.plannedDate), requiredDate: this.fromApiDate(job.requiredDate) } as any); this.form.markAsPristine(); this.loaded.set(true);
  }
  private fromApiDate(value?: string): Date | null { if (!value) return null; const [year,month,day]=value.slice(0,10).split('-').map(Number); return new Date(year,month-1,day); }
  private toApiDate(value: Date | string | null | undefined): string | null { if (!value) return null; if (typeof value==='string') return value.slice(0,10); const year=value.getFullYear(); const month=`${value.getMonth()+1}`.padStart(2,'0'); const day=`${value.getDate()}`.padStart(2,'0'); return `${year}-${month}-${day}`; }
}
