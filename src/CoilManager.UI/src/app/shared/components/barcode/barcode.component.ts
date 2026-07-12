import { AfterViewInit, Component, ElementRef, effect, input, viewChild } from '@angular/core';
import JsBarcode from 'jsbarcode';

@Component({selector:'app-barcode',template:`<svg #barcode role="img" [attr.aria-label]="'Code 128 Barcode for '+value()"></svg>`,styles:[`:host{display:block;max-width:100%;overflow:hidden}svg{display:block;max-width:100%;height:auto}`]})
export class BarcodeComponent implements AfterViewInit {
  readonly value=input.required<string>(); readonly width=input(2); readonly height=input(48); readonly displayValue=input(true);
  private readonly element=viewChild.required<ElementRef<SVGSVGElement>>('barcode'); private ready=false;
  constructor(){effect(()=>{this.value();this.width();this.height();this.displayValue();if(this.ready)this.render();});}
  ngAfterViewInit(){this.ready=true;this.render();}
  private render(){JsBarcode(this.element().nativeElement,this.value(),{format:'CODE128',width:this.width(),height:this.height(),displayValue:this.displayValue(),margin:8,fontSize:14});}
}
