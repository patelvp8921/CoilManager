import { CoilStatus, PaginationResult } from '../../raw-coil/models/raw-coil.model';

export interface SlitCoil {
  id: string;
  coilNumber: string;
  motherCoilId: string;
  motherCoilNumber: string;
  slittingJobId: string;
  slittingJobNo: string;
  grade?: string | null;
  thickness: number;
  category: string;
  coreLossPerKg: number;
  width: number;
  weight: number;
  status: CoilStatus;
  warehouseLocation?: string | null;
  barcodeValue: string;
  qrCodeValue: string;
  labelVersion: string;
  createdOn: string;
}

export interface SlitCoilQuery {
  page: number;
  pageSize: number;
  search?: string;
  status?: CoilStatus | null;
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
