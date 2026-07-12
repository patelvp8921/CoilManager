import { DatePipe, DecimalPipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { ActivatedRoute, RouterLink } from '@angular/router';
import QRCode from 'qrcode';
import { statusLabel } from '../../../raw-coil/models/raw-coil.model';
import { InventoryTransaction, SlitCoilDetails } from '../../models/slit-coil.model';
import { SlitCoilService } from '../../services/slit-coil.service';

@Component({selector:'app-slit-coil-detail', imports:[DatePipe,DecimalPipe,RouterLink,MatButtonModule,MatCardModule,MatChipsModule,MatIconModule,MatProgressSpinnerModule,MatSnackBarModule], templateUrl:'./slit-coil-detail.component.html', styleUrl:'./slit-coil-detail.component.scss'})
export class SlitCoilDetailComponent implements OnInit {
  protected readonly coil=signal<SlitCoilDetails|null>(null); protected readonly transactions=signal<readonly InventoryTransaction[]>([]); protected readonly qr=signal(''); protected readonly loading=signal(true);
  private readonly route=inject(ActivatedRoute); private readonly service=inject(SlitCoilService); private readonly snack=inject(MatSnackBar);
  ngOnInit(){const id=this.route.snapshot.paramMap.get('id')!; this.service.getById(id).subscribe({next:async coil=>{this.coil.set(coil);this.service.getTransactions(coil.coilNumber).subscribe(rows=>this.transactions.set(rows));this.qr.set(await QRCode.toDataURL(coil.coilNumber,{width:180,margin:1}));this.loading.set(false);},error:()=>{this.loading.set(false);this.snack.open('Unable to load Coil Details.','Close',{duration:4000});}});}
  protected status(value:number){return statusLabel(value);} protected transaction(value:number){return ['','Slitting Job Release','Slitting Job Cancel','Mother Coil Consumed','Slit Coil Generated','Slitting Started'][value]??'Inventory Movement';}
  protected copy(){const number=this.coil()?.coilNumber;if(number)navigator.clipboard.writeText(number).then(()=>this.snack.open('Coil Number copied.','Close',{duration:2000}));}
  protected print(){this.snack.open('Thermal label printing is planned for B3.4.','Close',{duration:3500});}
}
