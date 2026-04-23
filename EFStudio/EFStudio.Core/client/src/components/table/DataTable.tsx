import { TableBody } from "@/components/ui/table";
import { useTableState } from "@/hooks/useTableState";
import type { ColumnDef, PaginationState, RecordRow, SortState } from "@/types";
import { useRef } from "react";
import { DataTableHeader } from "./DataTableHeader";
import { DataTablePagination } from "./DataTablePagination";
import { DataTableRow } from "./DataTableRow";
import { DataTableToolbar } from "./DataTableToolbar";

function colWidth(col: ColumnDef): string {
  if (col.type === "boolean") return "72px";
  if (col.type === "number") return "90px";
  if (col.type === "datetime") return "160px";
  if (col.type === "uuid" || col.isPrimaryKey || col.isForeignKey) return "155px";
  if (col.type === "json") return "220px";
  return "180px";
}

interface DataTableProps {
  columns: ColumnDef[];
  rows: RecordRow[];
  filter: string;
  sort: SortState;
  pagination: PaginationState;
  onFilterChange: (value: string) => void;
  onSortChange: (column: string) => void;
  onPageChange: (page: number) => void;
  onPageSizeChange: (size: number) => void;
  onAddRecord: () => void;
  onEditRecord: (row: RecordRow) => void;
  onDeleteRecord: (row: RecordRow) => void;
}

export function DataTable({
  columns,
  rows,
  filter,
  sort,
  pagination,
  onFilterChange,
  onSortChange,
  onPageChange,
  onPageSizeChange,
  onAddRecord,
  onEditRecord,
  onDeleteRecord,
}: DataTableProps) {
  const headerRef = useRef<HTMLDivElement>(null);
  const bodyRef = useRef<HTMLDivElement>(null);

  function syncHeaderScroll() {
    if (headerRef.current && bodyRef.current) {
      headerRef.current.scrollLeft = bodyRef.current.scrollLeft;
    }
  }

  const { paginatedRows, totalRows, totalPages } = useTableState(
    rows,
    columns,
    filter,
    sort,
    pagination
  );

  const colgroup = (
    <colgroup>
      {columns.map((col) => (
        <col key={col.name} style={{ width: colWidth(col) }} />
      ))}
      <col style={{ width: "32px" }} />
    </colgroup>
  );

  return (
    <div className="flex flex-1 flex-col overflow-hidden">
      <DataTableToolbar
        filter={filter}
        onFilterChange={(v) => {
          onFilterChange(v);
          onPageChange(1);
        }}
        onAddRecord={onAddRecord}
      />
      <div className="flex-1 min-h-0 flex flex-col overflow-hidden">
        {/* Fixed header — horizontally synced to body via ref */}
        <div ref={headerRef} className="shrink-0 overflow-hidden border-b border-border bg-muted">
          <table className="w-full table-fixed">
            {colgroup}
            <DataTableHeader
              columns={columns}
              sort={sort}
              onSortChange={onSortChange}
            />
          </table>
        </div>
        {/* Scrollable body — drives horizontal scroll for both */}
        <div ref={bodyRef} className="flex-1 overflow-auto min-h-0" onScroll={syncHeaderScroll}>
          <table className="w-full table-fixed">
            {colgroup}
            <TableBody>
                {paginatedRows.length === 0 ? (
                  <tr>
                    <td
                      colSpan={columns.length + 1}
                      className="py-16 text-center text-xs text-muted-foreground"
                    >
                      {filter ? "No records match your filter." : "No records found."}
                    </td>
                  </tr>
                ) : (
                  paginatedRows.map((row, i) => (
                    <DataTableRow
                      key={i}
                      row={row}
                      columns={columns}
                      onEdit={onEditRecord}
                      onDelete={onDeleteRecord}
                    />
                  ))
                )}
              </TableBody>
            </table>
        </div>
      </div>
      <DataTablePagination
        pagination={pagination}
        totalRows={totalRows}
        totalPages={totalPages}
        onPageChange={onPageChange}
        onPageSizeChange={onPageSizeChange}
      />
    </div>
  );
}
