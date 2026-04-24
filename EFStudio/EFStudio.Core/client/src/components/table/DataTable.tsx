import { TableBody } from "@/components/ui/table";
import { useTableState } from "@/hooks/useTableState";
import type { ColumnDef, FieldValue, PaginationState, RecordRow, SortState, TableDef } from "@/types";
import { useEffect, useState } from "react";
import { DeleteConfirmDialog } from "../records/DeleteConfirmDialog";
import { DataTableHeader } from "./DataTableHeader";
import { DataTablePagination } from "./DataTablePagination";
import { DataTableRow } from "./DataTableRow";
import { DataTableToolbar } from "./DataTableToolbar";

function colWidth(col: ColumnDef): number {
  if (col.type === "boolean") return 72;
  if (col.type === "number") return 90;
  if (col.type === "datetime") return 160;
  if (col.type === "uuid" || col.isPrimaryKey || col.isForeignKey) return 155;
  if (col.type === "json") return 220;
  return 180;
}

function initialWidths(columns: ColumnDef[]): Record<string, number> {
  return Object.fromEntries(columns.map((col) => [col.name, colWidth(col)]));
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
  onBulkDelete: (rows: RecordRow[]) => void;
  onJumpToRef: (tableName: string, value: FieldValue) => void;
  allTables: TableDef[];
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
  onBulkDelete,
  onJumpToRef,
  allTables,
}: DataTableProps) {
  const [selectedKeys, setSelectedKeys] = useState<Set<string>>(new Set());
  const [bulkDeleteOpen, setBulkDeleteOpen] = useState(false);
  const [columnWidths, setColumnWidths] = useState<Record<string, number>>(() => initialWidths(columns));

  useEffect(() => { setSelectedKeys(new Set()); }, [columns]);
  useEffect(() => { setColumnWidths(initialWidths(columns)); }, [columns]);

  function handleResizeColumn(name: string, width: number) {
    setColumnWidths((prev) => ({ ...prev, [name]: width }));
  }

  const pkCol = columns.find((c) => c.isPrimaryKey);
  function getRowKey(row: RecordRow): string {
    return pkCol ? String(row[pkCol.name]) : JSON.stringify(row);
  }

  const { paginatedRows, totalRows, totalPages } = useTableState(rows, columns, filter, sort, pagination);

  const paginatedKeys = paginatedRows.map(getRowKey);
  const allSelected = paginatedKeys.length > 0 && paginatedKeys.every((k) => selectedKeys.has(k));
  const someSelected = paginatedKeys.some((k) => selectedKeys.has(k));

  function toggleAll() {
    setSelectedKeys((prev) => {
      const next = new Set(prev);
      if (allSelected) {
        paginatedKeys.forEach((k) => next.delete(k));
      } else {
        paginatedKeys.forEach((k) => next.add(k));
      }
      return next;
    });
  }

  function toggleRow(key: string) {
    setSelectedKeys((prev) => {
      const next = new Set(prev);
      next.has(key) ? next.delete(key) : next.add(key);
      return next;
    });
  }

  function handleBulkDeleteConfirm() {
    onBulkDelete(rows.filter((r) => selectedKeys.has(getRowKey(r))));
    setSelectedKeys(new Set());
  }

  const tableWidth = 36 + columns.reduce((sum, col) => sum + (columnWidths[col.name] ?? colWidth(col)), 0);
  const colgroup = (
    <colgroup>
      <col style={{ width: 36 }} />
      {columns.map((col) => (
        <col key={col.name} style={{ width: columnWidths[col.name] ?? colWidth(col) }} />
      ))}
    </colgroup>
  );

  return (
    <div className="flex flex-1 flex-col overflow-hidden">
      <DataTableToolbar
        filter={filter}
        onFilterChange={(v) => { onFilterChange(v); onPageChange(1); }}
        onAddRecord={onAddRecord}
        selectedCount={selectedKeys.size}
        onBulkDelete={() => setBulkDeleteOpen(true)}
      />
      <div className="flex-1 overflow-auto min-h-0">
        <table className="table-fixed" style={{ minWidth: tableWidth, width: tableWidth }}>
          {colgroup}
          <DataTableHeader
            columns={columns}
            sort={sort}
            onSortChange={onSortChange}
            allSelected={allSelected}
            someSelected={someSelected}
            onToggleAll={toggleAll}
            columnWidths={columnWidths}
            onResizeColumn={handleResizeColumn}
          />
          <TableBody>
            {paginatedRows.length === 0 ? (
              <tr>
                <td colSpan={columns.length + 1} className="py-16 text-center text-xs text-muted-foreground">
                  {filter ? "No records match your filter." : "No records found."}
                </td>
              </tr>
            ) : (
              paginatedRows.map((row, i) => (
                <DataTableRow
                  key={i}
                  row={row}
                  columns={columns}
                  isSelected={selectedKeys.has(getRowKey(row))}
                  onToggleSelect={() => toggleRow(getRowKey(row))}
                  onEdit={onEditRecord}
                  onDelete={onDeleteRecord}
                  onJumpToRef={onJumpToRef}
                  allTables={allTables}
                />
              ))
            )}
          </TableBody>
        </table>
      </div>
      <DataTablePagination
        pagination={pagination}
        totalRows={totalRows}
        totalPages={totalPages}
        onPageChange={onPageChange}
        onPageSizeChange={onPageSizeChange}
      />
      <DeleteConfirmDialog
        open={bulkDeleteOpen}
        onOpenChange={setBulkDeleteOpen}
        count={selectedKeys.size}
        onConfirm={handleBulkDeleteConfirm}
      />
    </div>
  );
}
