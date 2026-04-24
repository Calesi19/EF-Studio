import { keepPreviousData, queryOptions, useQuery } from "@tanstack/react-query";
import { DEFAULT_QUERY_STALE_TIME_MS, EFSTUDIO_API_BASE } from "@/api/constants";
import type { RecordRow } from "@/types";

interface ApiTableDataResponse {
  name: string;
  rows: Record<string, unknown>[];
}

export const tableDataQueryKey = (tableName: string) => ["table-data", tableName] as const;

function normalizeCellValue(value: unknown): RecordRow[string] {
  if (value === null || value === undefined) {
    return null;
  }

  if (typeof value === "string" || typeof value === "number" || typeof value === "boolean") {
    return value;
  }

  return JSON.stringify(value);
}

function normalizeRows(rows: Record<string, unknown>[]): RecordRow[] {
  return rows.map((row) =>
    Object.fromEntries(Object.entries(row).map(([key, value]) => [key, normalizeCellValue(value)])),
  );
}

export async function fetchTableData(tableName: string, signal?: AbortSignal): Promise<RecordRow[]> {
  const tableResponse = await fetch(`${EFSTUDIO_API_BASE}/data?table=${encodeURIComponent(tableName)}`, {
    signal,
  });

  if (!tableResponse.ok) {
    throw new Error(`Failed to load data for ${tableName} (${tableResponse.status})`);
  }

  const tableData = (await tableResponse.json()) as ApiTableDataResponse;

  return normalizeRows(tableData.rows);
}

export function tableDataQueryOptions(tableName: string) {
  return queryOptions({
    queryKey: tableDataQueryKey(tableName),
    queryFn: ({ signal }) => fetchTableData(tableName, signal),
    staleTime: DEFAULT_QUERY_STALE_TIME_MS,
    placeholderData: keepPreviousData,
    enabled: !!tableName,
  });
}

export function useTableData(tableName: string, enabled = true) {
  return useQuery({
    ...tableDataQueryOptions(tableName),
    enabled: enabled && !!tableName,
  });
}
