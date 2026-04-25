export type FieldValue = string | number | boolean | null;

export type ColumnType =
  | "string"
  | "number"
  | "boolean"
  | "datetime"
  | "uuid"
  | "json";

export interface ColumnDef {
  name: string;
  type: ColumnType;
  isPrimaryKey: boolean;
  isForeignKey: boolean;
  isNullable: boolean;
  foreignKeyTable?: string;
}

export type RecordRow = Record<string, FieldValue>;

export interface DbContextDef {
  name: string;
  displayName: string;
  isSelected: boolean;
  isDefault: boolean;
  isAvailable: boolean;
  activationError?: string;
}

export interface TableDef {
  key: string;
  name: string;
  schema?: string;
  displayName: string;
  modelDisplayName: string;
  columns: ColumnDef[];
  rows: RecordRow[];
}

export interface SortState {
  column: string | null;
  direction: "asc" | "desc";
}

export interface PaginationState {
  page: number;
  pageSize: number;
}

export interface TabState {
  id: string;
  tableKey: string;
  filter: string;
  sort: SortState;
  pagination: PaginationState;
}
