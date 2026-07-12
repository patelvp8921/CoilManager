export enum LabelPrintType{Initial=1,Reprint=2,BatchPrint=3}
export interface SlitCoilLabel{slitCoilId:string;coilNumber:string;motherCoilNumber:string;slittingJobNo:string;grade?:string|null;thickness:number;category:string;coreLossPerKg:number;width:number;weight:number;supplier?:string|null;manufacturer?:string|null;heatNumber:string;barcodeValue:string;qrCodeValue:string;labelVersion:string;labelPrinted:boolean;labelPrintCount:number;labelLastPrintedOn?:string|null;labelLastPrintedBy?:string|null;companyName:string;companyAddress?:string|null;companyLogoUrl?:string|null;labelWidthMm:number;labelHeightMm:number;}
export interface PrintLabelRequest{copies:number;printerName?:string|null;remarks?:string|null;}
export interface PrintLabelResult{slitCoilId:string;coilNumber:string;labelVersion:string;printCount:number;printedOn:string;printedBy?:string|null;copies:number;printType:LabelPrintType;}
export interface LabelPrintHistory{printedOn:string;printedBy?:string|null;copies:number;labelVersion:string;printerName?:string|null;printType:LabelPrintType;remarks?:string|null;}
export interface BatchPrintRequest{slitCoilIds:readonly string[];copiesPerLabel:number;printerName?:string|null;remarks?:string|null;}
export interface BatchPrintResult{totalRequested:number;totalPrinted:number;failed:readonly {slitCoilId:string;reason:string}[];labels:readonly PrintLabelResult[];}
