export enum CoilStatus {
  Draft = 0,
  Available = 1,
  Reserved = 2,
  OnHold = 2,
  Rejected = 3,
  Scrapped = 3,
  Consumed = 4,
  Dispatched = 5,
  UnderInspection = 6,
}

export const COIL_STATUS_OPTIONS: readonly { value: CoilStatus; label: string }[] = [
  { value: CoilStatus.Draft, label: 'Draft' },
  { value: CoilStatus.Available, label: 'Available' },
  { value: CoilStatus.Reserved, label: 'Reserved' },
  { value: CoilStatus.Rejected, label: 'Rejected' },
  { value: CoilStatus.UnderInspection, label: 'Under Inspection' },
  { value: CoilStatus.Consumed, label: 'Consumed' },
  { value: CoilStatus.Dispatched, label: 'Dispatched' },
];

export interface RawCoil {
  id: string;
  rawCoilNumber: string;
  coilID: string;
  coilNumber: string;
  heatNumber: string;
  poNumber?: string | null;
  invoiceNo?: string | null;
  millTCNo?: string | null;
  bisLicNumber?: string | null;
  supplierId: string;
  supplierName: string;
  manufacturerId: string;
  manufacturerName: string;
  gradeId: string;
  grade: string;
  thickness: number;
  thicknessMm?: number;
  category: string;
  width: number;
  weight: number;
  length: number;
  coreLossPerKg: number;
  warehouseLocation?: string | null;
  status: CoilStatus;
  receivedDate: string;
  createdOn: string;
  createdBy?: string | null;
  modifiedOn?: string | null;
  modifiedBy?: string | null;
  isDeleted: boolean;
  deletedOn?: string | null;
  rowVersion: string;
  documentPlaceholders: readonly string[];
}

export interface CreateRawCoilRequest {
  coilNumber: string;
  heatNumber: string;
  poNumber?: string | null;
  invoiceNo?: string | null;
  millTCNo?: string | null;
  bisLicNumber?: string | null;
  supplierId: string;
  manufacturerId: string;
  gradeId: string;
  width?: number | null;
  weight: number;
  length: number;
  warehouseLocation?: string | null;
  receivedDate: string;
  status?: CoilStatus;
}

export interface UpdateRawCoilRequest extends CreateRawCoilRequest {
  status: CoilStatus;
  rowVersion: string;
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

export interface PaginationResult {
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export function statusLabel(status: CoilStatus): string {
  return COIL_STATUS_OPTIONS.find((option) => option.value === status)?.label ?? 'Unknown';
}
