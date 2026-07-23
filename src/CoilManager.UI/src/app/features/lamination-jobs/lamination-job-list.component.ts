import { DatePipe, DecimalPipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, ViewChild, inject, signal } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatPaginator, MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSelectModule } from '@angular/material/select';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatTableModule } from '@angular/material/table';
import { MatTooltipModule } from '@angular/material/tooltip';
import { finalize } from 'rxjs';
import { LaminationJobService } from './lamination-job.service';
import { LaminationJob } from './lamination-job.model';

@Component({
  selector: 'app-lamination-list',
  imports: [DatePipe, DecimalPipe, ReactiveFormsModule, RouterLink, MatButtonModule, MatCardModule,
    MatFormFieldModule, MatIconModule, MatInputModule, MatPaginatorModule, MatProgressBarModule,
    MatSelectModule, MatSnackBarModule, MatTableModule, MatTooltipModule],
  templateUrl: './lamination-job-list.component.html',
  styleUrl: './lamination-job-list.component.scss',
})
export class LaminationJobListComponent implements OnInit {
  @ViewChild(MatPaginator) private paginator?: MatPaginator;

  protected readonly statusOptions = [
    { value: 0, label: 'Draft' }, { value: 2, label: 'Released' }, { value: 1, label: 'In Progress' },
    { value: 4, label: 'Completed' }, { value: 5, label: 'Cancelled' },
  ];
  protected readonly displayedColumns = ['number','date','reference','rating','design','grade','weight','status','actions'];
  protected readonly searchControl = new FormControl('', { nonNullable: true });
  protected readonly statusControl = new FormControl<number | null>(null);
  protected readonly jobs = signal<readonly LaminationJob[]>([]);
  protected readonly totalCount = signal(0);
  protected readonly pageSize = signal(25);
  protected readonly pageIndex = signal(0);
  protected readonly isLoading = signal(false);

  private readonly api = inject(LaminationJobService);
  private readonly snackBar = inject(MatSnackBar);

  ngOnInit(): void { this.loadJobs(); }
  protected applyFilters(): void { this.pageIndex.set(0); this.paginator?.firstPage(); this.loadJobs(); }
  protected resetFilters(): void { this.searchControl.reset(''); this.statusControl.reset(null); this.pageIndex.set(0); this.paginator?.firstPage(); this.loadJobs(); }
  protected onPageChange(event: PageEvent): void { this.pageIndex.set(event.pageIndex); this.pageSize.set(event.pageSize); this.loadJobs(); }
  protected statusName(value: unknown): string { return typeof value==='number' ? ['Draft','In Progress','Released','Legacy In Progress','Completed','Cancelled'][value] ?? 'Unknown' : `${value}`.replace('InProgress','In Progress'); }
  protected statusClass(value: unknown): string { return this.statusName(value).toLowerCase().replace(' ','-'); }
  protected designName(value: unknown): string { return value===1||value==='StepLap' ? 'Step Lap' : 'Simple'; }
  protected isStatus(job:LaminationJob,status:number):boolean { return (typeof job.status==='number' ? job.status : ['Draft','Allocated','Released','InProgress','Completed','Cancelled'].indexOf(job.status))===status; }
  protected hasActions(job:LaminationJob):boolean { return [0,1,2,4].some(status=>this.isStatus(job,status)); }
  protected remove(job:LaminationJob):void { if(!window.confirm(`Delete ${job.laminationJobNumber}?`))return; this.api.delete(job.id).subscribe({next:()=>this.loadJobs(),error:e=>this.showError(e)}); }
  protected cancel(job:LaminationJob):void { if(!window.confirm(`Cancel ${job.laminationJobNumber}?`))return; this.api.cancel(job.id).subscribe({next:()=>this.loadJobs(),error:e=>this.showError(e)}); }
  protected loadJobs():void {
    const params:Record<string,string|number>={pageNumber:this.pageIndex()+1,pageSize:this.pageSize()};
    const search=this.searchControl.value.trim(); if(search)params['search']=search;
    if(this.statusControl.value!==null)params['status']=this.statusControl.value;
    this.isLoading.set(true);
    this.api.list(params).pipe(finalize(()=>this.isLoading.set(false))).subscribe({
      next:response=>{this.jobs.set(response.data??[]);this.totalCount.set(response.pagination?.totalCount??0);},
      error:error=>this.showError(error),
    });
  }
  private showError(error:HttpErrorResponse):void { this.snackBar.open(error.error?.errors?.join('\n')||error.error?.message||error.message||'Request failed.','Close',{duration:6000}); }
}