import { keepPreviousData, queryOptions, useQuery } from "@tanstack/react-query";
import { DEFAULT_QUERY_STALE_TIME_MS, EFSTUDIO_API_BASE } from "@/api/constants";
import type { PaginationState, RecordRow, SortState } from "@/types";

interface ApiTableDataResponse {
  key: string;
  name: string;
  schema?: string | null;
  page: number;
  pageSize: number;
  totalRows: number;
  rows: Record<string, unknown>[];
}

export interface TablePageData {
  rows: RecordRow[];
  totalRows: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export const tableDataQueryKey = (
  contextName: string,
  tableKey: string,
  pagination: PaginationState,
  filter: string,
  sort: SortState,
) =>
  [
    "table-data",
    contextName,
    tableKey,
    pagination.page,
    pagination.pageSize,
    filter,
    sort.column,
    sort.direction,
  ] as const;

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

export async function fetchTableData(
  contextName: string,
  tableKey: string,
  pagination: PaginationState,
  filter: string,
  sort: SortState,
  signal?: AbortSignal,
): Promise<TablePageData> {
  const searchParams = new URLSearchParams({
    context: contextName,
    table: tableKey,
    page: String(pagination.page),
    pageSize: String(pagination.pageSize),
  });

  if (filter.trim()) {
    searchParams.set("filter", filter);
  }

  if (sort.column) {
    searchParams.set("sortColumn", sort.column);
    searchParams.set("sortDirection", sort.direction);
  }

  const tableResponse = await fetch(`${EFSTUDIO_API_BASE}/data?${searchParams.toString()}`, {
    signal,
  });

  if (!tableResponse.ok) {
    throw new Error(`Failed to load data for ${tableKey} (${tableResponse.status})`);
  }

  const tableData = (await tableResponse.json()) as ApiTableDataResponse;

  return {
    rows: normalizeRows(tableData.rows),
    totalRows: tableData.totalRows,
    page: tableData.page,
    pageSize: tableData.pageSize,
    totalPages: Math.max(1, Math.ceil(tableData.totalRows / Math.max(1, tableData.pageSize))),
  };
}

export function tableDataQueryOptions(
  contextName: string | null,
  tableKey: string,
  pagination: PaginationState,
  filter: string,
  sort: SortState,
) {
  return queryOptions({
    queryKey: tableDataQueryKey(contextName ?? "", tableKey, pagination, filter, sort),
    queryFn: ({ signal }) => {
      if (!contextName) {
        throw new Error("Select a DbContext before loading data.");
      }

      return fetchTableData(contextName, tableKey, pagination, filter, sort, signal);
    },
    staleTime: DEFAULT_QUERY_STALE_TIME_MS,
    placeholderData: keepPreviousData,
    enabled: !!tableKey && !!contextName,
  });
}

export function useTableData(
  contextName: string | null,
  tableKey: string,
  pagination: PaginationState,
  filter: string,
  sort: SortState,
  enabled = true,
) {
  return useQuery({
    ...tableDataQueryOptions(contextName, tableKey, pagination, filter, sort),
    enabled: enabled && !!tableKey && !!contextName,
  });
}
