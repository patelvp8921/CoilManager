import { CoilStatus, PaginationResult } from '../../raw-coil/models/raw-coil.model';

export enum SlittingJobStatus {
  Draft = 0,
  Released = 1,
  InProgress = 2,
  Completed = 3,
  Cancelled = 4,
}

export interface SlittingMotherCoilLookup {
  id: string;
  motherCoilId: string;
  coilNumber: string;
  heatNumber: string;
  supplierName?: string | null;
  manufacturerName?: string | null;
  grade?: string | null;
  thickness: number;
  category: string;
  coreLossPerKg: number;
  width: number;
  weight: number;
  length: number;
  warehouseLocation?: string | null;
  status: CoilStatus;
}

export interface SlittingJobItemRequest {
  sequenceNo: number;
  width: number;
  remarks?: string | null;
}

export interface CreateSlittingJobRequest {
  planningDate: string;
  plannerId?: string | null;
  motherCoilId: string;
  machineId?: string | null;
  shift?: string | null;
  knifeThickness: number;
  leftEdgeTrim: number;
  rightEdgeTrim: number;
  remarks?: string | null;
  items: readonly SlittingJobItemRequest[];
}

export interface UpdateSlittingJobRequest extends CreateSlittingJobRequest {
  rowVersion: string;
}

export interface SlittingJobItem {
  id: string;
  sequenceNo: number;
  slitCoilId: string;
  width: number;
  estimatedWeight: number;
  actualWidth?: number | null;
  actualWeight?: number | null;
  status: SlittingJobStatus;
  remarks?: string | null;
}

export interface SlittingJob {
  id: string;
  slittingJobNo: string;
  planningDate: string;
  plannerId?: string | null;
  motherCoilId: string;
  motherCoilNo: string;
  supplierCoilNumber?: string | null;
  heatNumber?: string | null;
  supplierName?: string | null;
  manufacturerName?: string | null;
  grade?: string | null;
  thickness: number;
  category: string;
  coreLossPerKg: number;
  motherCoilWidth: number;
  motherCoilWeight: number;
  motherCoilLength: number;
  warehouseLocation?: string | null;
  motherCoilStatus: CoilStatus;
  machineId?: string | null;
  shift?: string | null;
  knifeThickness: number;
  leftEdgeTrim: number;
  rightEdgeTrim: number;
  remarks?: string | null;
  status: SlittingJobStatus;
  totalPlannedWidth: number;
  knifeLoss: number;
  edgeTrim: number;
  remainingWidth: number;
  utilizationPercent: number;
  createdOn: string;
  createdBy?: string | null;
  modifiedOn?: string | null;
  modifiedBy?: string | null;
  releasedBy?: string | null;
  releasedOn?: string | null;
  startedBy?: string | null;
  startedOn?: string | null;
  completedBy?: string | null;
  completedOn?: string | null;
  cancelledBy?: string | null;
  cancelledOn?: string | null;
  rowVersion: string;
  items: readonly SlittingJobItem[];
}

export interface SlittingJobQuery {
  page: number;
  pageSize: number;
  search?: string;
  status?: SlittingJobStatus | null;
  sortBy?: string;
  sortDirection?: 'asc' | 'desc' | '';
}

export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data: T | null;
  errors: readonly string[];
}

export interface ApiPagedResponse<T> {
  success: boolean;
  message: string;
  data: readonly T[];
  pagination: PaginationResult;
  errors: readonly string[];
}

export interface CompleteSlittingItemRequest {
  slittingJobItemId: string;
  actualWeight: number;
  actualWidth?: number | null;
  remarks?: string | null;
}

export interface CompleteSlittingRequest {
  rowVersion: string;
  slits: readonly CompleteSlittingItemRequest[];
}

export interface StartSlittingRequest {
  rowVersion: string;
  machineId?: string | null;
  shift?: string | null;
  remarks?: string | null;
}

export interface StartSlittingResponse {
  slittingJobId: string;
  slittingJobNo: string;
  status: SlittingJobStatus;
  motherCoilNumber: string;
  motherCoilStatus: CoilStatus;
  startedBy?: string | null;
  startedOn: string;
}

export interface GeneratedSlitCoil {
  id: string;
  coilNumber: string;
  parentCoilNumber: string;
  motherCoilNumber: string;
  slittingJobNo: string;
  width: number;
  weight: number;
  grade?: string | null;
  thickness: number;
  category: string;
  coreLossPerKg: number;
  status: CoilStatus;
  barcodeValue: string;
  qrCodeValue: string;
  labelVersion: string;
}

export interface CompleteSlittingResponse {
  slittingJobId: string;
  slittingJobNo: string;
  motherCoilNumber: string;
  generatedSlitCoils: readonly GeneratedSlitCoil[];
  totalGeneratedWeight: number;
  completedOn: string;
  remainingWidth: number;
  unusedEstimatedWeight: number;
  warnings: readonly string[];
}

export const SLITTING_JOB_STATUS_OPTIONS: readonly { value: SlittingJobStatus; label: string }[] = [
  { value: SlittingJobStatus.Draft, label: 'Draft' },
  { value: SlittingJobStatus.Released, label: 'Released' },
  { value: SlittingJobStatus.InProgress, label: 'In Progress' },
  { value: SlittingJobStatus.Completed, label: 'Completed' },
  { value: SlittingJobStatus.Cancelled, label: 'Cancelled' },
];

export function slittingJobStatusLabel(status: SlittingJobStatus): string {
  return SLITTING_JOB_STATUS_OPTIONS.find((option) => option.value === status)?.label ?? 'Unknown';
}
