import { PaginationResult } from '../../raw-coil/models/raw-coil.model';

export type MasterType = 'manufacturers' | 'suppliers' | 'grades';

export interface MasterRouteData {
  type: MasterType;
  title: string;
  singular: string;
}

export interface MasterRecord {
  id: string;
  code: string;
  name: string;
  description?: string | null;
  country?: string | null;
  address?: string | null;
  gst?: string | null;
  email?: string | null;
  contactNo?: string | null;
  isActive: boolean;
  createdOn: string;
  createdBy?: string | null;
  modifiedOn?: string | null;
  modifiedBy?: string | null;
  rowVersion: string;
}

export interface MasterRequest {
  code?: string | null;
  name: string;
  description?: string | null;
  country?: string | null;
  address?: string | null;
  gst?: string | null;
  email?: string | null;
  contactNo?: string | null;
  isActive: boolean;
  rowVersion?: string | null;
}

export interface MasterQuery {
  page: number;
  pageSize: number;
  search?: string;
  sortBy?: string;
  sortDirection?: string;
  isActive?: boolean | null;
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
