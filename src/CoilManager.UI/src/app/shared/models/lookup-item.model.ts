export interface LookupItem {
  id: string;
  code: string;
  name: string;
  thicknessMm?: number | null;
  category?: string | null;
  coreLossPerKg?: number | null;
}
