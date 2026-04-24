import { keepPreviousData, queryOptions, useQuery } from "@tanstack/react-query";
import { DEFAULT_QUERY_STALE_TIME_MS, EFSTUDIO_API_BASE } from "@/api/constants";
import type { ColumnDef, ColumnType } from "@/types";

export interface SchemaTable {
  key: string;
  name: string;
  schema?: string;
  displayName: string;
  modelDisplayName: string;
  columns: ColumnDef[];
}

interface ApiColumnInfo {
  name: string;
  dataType: string;
  isPrimaryKey: boolean;
  isNullable: boolean;
  isForeignKey: boolean;
  foreignKeyTable?: string | null;
}

interface ApiTableInfo {
  key: string;
  name: string;
  modelName: string;
  schema?: string | null;
  columns: ApiColumnInfo[];
}

export const schemaQueryKey = ["schema"] as const;

function toDisplayName(name: string): string {
  return name.replace(/([a-z0-9])([A-Z])/g, "$1 $2");
}

function getTableDisplayName(name: string, schema?: string): string {
  const baseName = toDisplayName(name);
  return schema ? `${baseName} (${schema})` : baseName;
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

function normalizeColumn(column: ApiColumnInfo): ColumnDef {
  return {
    name: column.name,
    type: mapColumnType(column.dataType),
    isPrimaryKey: column.isPrimaryKey,
    isForeignKey: column.isForeignKey,
    isNullable: column.isNullable,
    foreignKeyTable: column.foreignKeyTable ?? undefined,
  };
}

export async function fetchSchema(signal?: AbortSignal): Promise<SchemaTable[]> {
  const response = await fetch(`${EFSTUDIO_API_BASE}/schema`, { signal });

  if (!response.ok) {
    throw new Error(`Failed to load schema (${response.status})`);
  }

  const schema = (await response.json()) as ApiTableInfo[];

  return schema.map((table) => ({
    key: table.key,
    name: table.name,
    schema: table.schema ?? undefined,
    displayName: getTableDisplayName(table.name, table.schema ?? undefined),
    modelDisplayName: toDisplayName(table.modelName),
    columns: table.columns.map(normalizeColumn),
  }));
}

export function schemaQueryOptions() {
  return queryOptions({
    queryKey: schemaQueryKey,
    queryFn: ({ signal }) => fetchSchema(signal),
    staleTime: DEFAULT_QUERY_STALE_TIME_MS,
    placeholderData: keepPreviousData,
  });
}

export function useSchema(enabled = true) {
  return useQuery({
    ...schemaQueryOptions(),
    enabled,
  });
}
