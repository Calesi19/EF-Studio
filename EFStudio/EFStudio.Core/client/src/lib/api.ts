import type { ColumnDef, ColumnType, RecordRow, TableDef } from "@/types";

interface ApiColumnInfo {
  Name: string;
  DataType: string;
  IsPrimaryKey: boolean;
  IsNullable: boolean;
  IsForeignKey: boolean;
  ForeignKeyTable?: string | null;
}

interface ApiTableInfo {
  Name: string;
  Columns: ApiColumnInfo[];
}

interface ApiTableDataResponse {
  Name: string;
  Rows: Record<string, unknown>[];
}

const API_BASE = import.meta.env.VITE_EFSTUDIO_API_BASE ?? "/efstudio/api";

function toDisplayName(name: string): string {
  return name.replace(/([a-z0-9])([A-Z])/g, "$1 $2");
}

function mapColumnType(dataType: string): ColumnType {
  const normalized = dataType.toLowerCase();

  if (
    normalized.includes("int") ||
    normalized.includes("real") ||
    normalized.includes("float") ||
    normalized.includes("double") ||
    normalized.includes("decimal") ||
    normalized.includes("numeric")
  ) {
    return "number";
  }

  if (normalized.includes("bool")) {
    return "boolean";
  }

  if (normalized.includes("date") || normalized.includes("time")) {
    return "datetime";
  }

  if (normalized.includes("uuid") || normalized.includes("guid")) {
    return "uuid";
  }

  if (normalized.includes("json")) {
    return "json";
  }

  return "string";
}

function normalizeCellValue(value: unknown): RecordRow[string] {
  if (value === null || value === undefined) {
    return null;
  }

  if (typeof value === "string" || typeof value === "number" || typeof value === "boolean") {
    return value;
  }

  return JSON.stringify(value);
}

function normalizeColumn(column: ApiColumnInfo): ColumnDef {
  return {
    name: column.Name,
    type: mapColumnType(column.DataType),
    isPrimaryKey: column.IsPrimaryKey,
    isForeignKey: column.IsForeignKey,
    isNullable: column.IsNullable,
    foreignKeyTable: column.ForeignKeyTable ?? undefined,
  };
}

function normalizeRows(rows: Record<string, unknown>[]): RecordRow[] {
  return rows.map((row) =>
    Object.fromEntries(
      Object.entries(row).map(([key, value]) => [key, normalizeCellValue(value)]),
    ),
  );
}

export async function fetchTables(signal?: AbortSignal): Promise<TableDef[]> {
  const response = await fetch(`${API_BASE}/schema`, { signal });
  if (!response.ok) {
    throw new Error(`Failed to load schema (${response.status})`);
  }

  const schema = (await response.json()) as ApiTableInfo[];
  const tableData = await Promise.all(
    schema.map(async (table) => {
      const tableResponse = await fetch(
        `${API_BASE}/data?table=${encodeURIComponent(table.Name)}`,
        { signal },
      );

      if (!tableResponse.ok) {
        throw new Error(`Failed to load data for ${table.Name} (${tableResponse.status})`);
      }

      return (await tableResponse.json()) as ApiTableDataResponse;
    }),
  );

  const rowsByTable = new Map(tableData.map((table) => [table.Name, normalizeRows(table.Rows)]));

  return schema.map((table) => ({
    name: table.Name,
    displayName: toDisplayName(table.Name),
    columns: table.Columns.map(normalizeColumn),
    rows: rowsByTable.get(table.Name) ?? [],
  }));
}
