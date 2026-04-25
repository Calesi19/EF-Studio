import { useMemo } from "react";
import type { ColumnDef, PaginationState, RecordRow, SortState } from "@/types";

export function useTableState(
  rows: RecordRow[],
  _columns: ColumnDef[],
  filter: string,
  sort: SortState,
  pagination: PaginationState
) {
  const filteredRows = useMemo(() => {
    if (!filter.trim()) return rows;
    const q = filter.toLowerCase();
    return rows.filter((row) =>
      Object.values(row).some((val) =>
        val !== null && String(val).toLowerCase().includes(q)
      )
    );
  }, [rows, filter]);

  const sortedRows = useMemo(() => {
    if (!sort.column) return filteredRows;
    return [...filteredRows].sort((a, b) => {
      const av = a[sort.column!];
      const bv = b[sort.column!];
      if (av === null && bv === null) return 0;
      if (av === null) return 1;
      if (bv === null) return -1;
      const cmp = String(av).localeCompare(String(bv), undefined, { numeric: true });
      return sort.direction === "asc" ? cmp : -cmp;
    });
  }, [filteredRows, sort]);

  const totalRows = filteredRows.length;
  const totalPages = Math.max(1, Math.ceil(totalRows / pagination.pageSize));

  const paginatedRows = useMemo(() => {
    const start = (pagination.page - 1) * pagination.pageSize;
    return sortedRows.slice(start, start + pagination.pageSize);
  }, [sortedRows, pagination]);

  return { paginatedRows, totalRows, totalPages };
}
