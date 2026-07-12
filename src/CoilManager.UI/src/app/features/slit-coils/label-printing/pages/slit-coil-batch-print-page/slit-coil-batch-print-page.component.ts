import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormControl, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button'; import { MatCardModule } from '@angular/material/card';
import { MatCheckboxModule } from '@angular/material/checkbox'; import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon'; import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner'; import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { ActivatedRoute, RouterLink } from '@angular/router'; import { forkJoin } from 'rxjs';
import { SlitCoil } from '../../../models/slit-coil.model'; import { SlitCoilService } from '../../../services/slit-coil.service';
import { SlitCoilLabel } from '../../models/slit-coil-label.models'; import { SlitCoilLabelService } from '../../services/slit-coil-label.service';
import { SlitCoilLabelPreviewComponent } from '../../components/slit-coil-label-preview/slit-coil-label-preview.component';

@Component({selector:'app-slit-coil-batch-print-page',imports:[ReactiveFormsModule,RouterLink,MatButtonModule,MatCardModule,MatCheckboxModule,MatFormFieldModule,MatIconModule,MatInputModule,MatProgressSpinnerModule,MatSnackBarModule,SlitCoilLabelPreviewComponent],templateUrl:'./slit-coil-batch-print-page.component.html',styleUrl:'./slit-coil-batch-print-page.component.scss'})
export class SlitCoilBatchPrintPageComponent implements OnInit {
  protected readonly coils=signal<readonly SlitCoil[]>([]); protected readonly selected=signal(new Set<string>());
  protected readonly labels=signal<readonly SlitCoilLabel[]>([]); protected readonly loading=signal(false);
  protected readonly search=new FormControl('',{nonNullable:true}); protected readonly copies=new FormControl(1,{nonNullable:true,validators:[Validators.min(1),Validators.max(100)]});
  protected readonly printLabels=computed(()=>this.labels().flatMap(label=>Array.from({length:this.copies.value},()=>label)));
  private readonly route=inject(ActivatedRoute); private readonly coilsService=inject(SlitCoilService);
  private readonly labelService=inject(SlitCoilLabelService); private readonly snack=inject(MatSnackBar);
  ngOnInit(){const jobId=this.route.snapshot.paramMap.get('id'); if(jobId){this.loading.set(true);this.labelService.jobLabels(jobId).subscribe(labels=>{this.labels.set(labels);this.selected.set(new Set(labels.map(x=>x.slitCoilId)));this.coils.set(labels.map(this.toCoil));this.loading.set(false);});}else this.load();}
  protected load(){this.loading.set(true);this.coilsService.getSlitCoils({page:1,pageSize:100,search:this.search.value}).subscribe(r=>{this.coils.set(r.data);this.loading.set(false);});}
  protected toggle(id:string,checked:boolean){const next=new Set(this.selected());checked?next.add(id):next.delete(id);this.selected.set(next);this.loadPreviews();}
  protected all(checked:boolean){this.selected.set(new Set(checked?this.coils().map(c=>c.id):[]));this.loadPreviews();}
  protected has(id:string){return this.selected().has(id);}
  protected print(){const ids=[...this.selected()];if(!ids.length)return;this.labelService.batch({slitCoilIds:ids,copiesPerLabel:this.copies.value}).subscribe(result=>{this.snack.open(`Batch Print: ${result.totalPrinted} printed, ${result.failed.length} failed.`,'Close',{duration:5000});setTimeout(()=>window.print());});}
  private loadPreviews(){const ids=[...this.selected()];if(!ids.length){this.labels.set([]);return;}forkJoin(ids.map(id=>this.labelService.getLabel(id))).subscribe(labels=>this.labels.set(labels));}
  private toCoil=(label:SlitCoilLabel):SlitCoil=>({id:label.slitCoilId,coilNumber:label.coilNumber,motherCoilId:'',motherCoilNumber:label.motherCoilNumber,slittingJobId:'',slittingJobNo:label.slittingJobNo,grade:label.grade,thickness:label.thickness,category:label.category,width:label.width,weight:label.weight,supplier:label.supplier,manufacturer:label.manufacturer,status:1,warehouseLocation:null,createdOn:'',labelVersion:label.labelVersion,labelPrinted:label.labelPrinted,labelPrintCount:label.labelPrintCount,labelLastPrintedOn:label.labelLastPrintedOn,hasBarcode:true,hasQrCode:true});
}
