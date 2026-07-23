import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { environment } from '../../../environments/environment';

interface Summary { motherCoilsCreated:number;slittingJobsCreated:number;slitCoilsCreated:number;laminationJobsCreated:number;materialAllocationsCreated:number;inventoryTransactionsCreated:number;elapsedMilliseconds:number;message:string; }
interface Api<T>{data:T;message:string}
@Component({selector:'app-development-tools',standalone:true,imports:[CommonModule,FormsModule,RouterLink,MatButtonModule,MatCardModule,MatIconModule,MatProgressBarModule,MatSnackBarModule],templateUrl:'./development-tools.component.html',styleUrl:'./development-tools.component.scss'})
export class DevelopmentToolsComponent {
 private http=inject(HttpClient);private snack=inject(MatSnackBar);readonly running=signal(false);readonly stage=signal('');readonly summary=signal<Summary|null>(null);clearExistingData=true;
 readonly actions=[['All','Generate Demo Data','dataset'],['MotherCoils','Generate Mother Coils','inventory_2'],['SlittingJobs','Generate Slitting Jobs','precision_manufacturing'],['SlitCoils','Generate Slit Coils','view_list'],['LaminationJobs','Generate Lamination Jobs','layers'],['MaterialAllocation','Generate Material Allocation','inventory'],['Dashboard','Generate Dashboard Data','dashboard']];
 generate(stage:string):void{if(this.running())return;if(this.clearExistingData&&!confirm('This will clear existing production workflow data while preserving master data and users. Continue?'))return;this.running.set(true);this.stage.set(stage);this.http.post<Api<Summary>>(`${environment.apiBaseUrl}/development/demo-data/generate`,{clearExistingData:this.clearExistingData,stage}).subscribe({next:r=>{this.running.set(false);this.summary.set(r.data);this.snack.open(r.message||'Demo data generated.','Close',{duration:4000})},error:e=>{this.running.set(false);this.snack.open(e?.error?.message||'Demo data generation failed.','Close',{duration:6000})}})}
 clear():void{this.clearExistingData=true;this.generate('Clear')}
}