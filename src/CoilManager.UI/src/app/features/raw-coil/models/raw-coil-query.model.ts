import { CoilStatus } from './raw-coil.model';

export interface RawCoilQuery {
  page: number;
  pageSize: number;
  search?: string;
  grade?: string;
  manufacturer?: string;
  status?: CoilStatus | null;
  sortBy?: string;
  sortDirection?: 'asc' | 'desc' | '';
}
