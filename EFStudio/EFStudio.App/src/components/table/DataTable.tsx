import { TableBody } from "@/components/ui/table";
import type { ColumnDef, FieldValue, PaginationState, PendingEdits, RecordRow, SortState } from "@/types";
import { useEffect, useState } from "react";
import { DeleteConfirmDialog } from "../records/DeleteConfirmDialog";
import { DataTableHeader } from "./DataTableHeader";
import { DataTablePagination } from "./DataTablePagination";
import { DataTableRow } from "./DataTableRow";
import { DataTableToolbar } from "./DataTableToolbar";

const READ_WRITE_SELECTION_COLUMN_WIDTH = 36;
const DATETIME_COLUMN_WIDTH = 160;
const RELATION_COLUMN_WIDTH = 155;
const JSON_COLUMN_WIDTH = 220;
const DEFAULT_COLUMN_WIDTH = 180;

function colWidth(col: ColumnDef): number {
  if (col.type === "datetime") return DATETIME_COLUMN_WIDTH;
  if (col.type === "uuid" || col.isPrimaryKey || col.isForeignKey) return RELATION_COLUMN_WIDTH;
  if (col.type === "json") return JSON_COLUMN_WIDTH;
  return DEFAULT_COLUMN_WIDTH;
}

function initialWidths(columns: ColumnDef[]): Record<string, number> {
  return Object.fromEntries(columns.map((col) => [col.name, colWidth(col)]));
}

function serializeRowPk(row: RecordRow, pkColumns: ColumnDef[]): string {
  return pkColumns.map((col) => `${col.name}:${String(row[col.name])}`).join("|");
}

interface DataTableProps {
  columns: ColumnDef[];
  rows: RecordRow[];
  totalRows: number;
  totalPages: number | undefined;
  filter: string;
  sort: SortState;
  pagination: PaginationState;
  onFilterChange: (value: string) => void;
  onSortChange: (column: string) => void;
  onPageChange: (page: number) => void;
  onPageSizeChange: (size: number) => void;
  onAddRecord: () => void;
  canAddRecord?: boolean;
  selectionResetKey?: number;
  onEditRecord: (row: RecordRow) => void;
  onDeleteRecord: (row: RecordRow) => void;
  onBulkDelete: (rows: RecordRow[]) => void;
  onJumpToRef: (tableKey: string, value: FieldValue) => void;
  readOnly?: boolean;
  hideCheckboxColumn?: boolean;
  onRowClick?: (row: RecordRow) => void;
  pendingEdits?: PendingEdits;
  pendingEditCount?: number;
  savingEdits?: boolean;
  onCellEdit?: (row: RecordRow, columnName: string, value: FieldValue) => void;
  onSaveEdits?: () => void;
  onDiscardEdits?: () => void;
}

export function DataTable({
  columns,
  rows,
  totalRows,
  totalPages,
  filter,
  sort,
  pagination,
  onFilterChange,
  onSortChange,
  onPageChange,
  onPageSizeChange,
  onAddRecord,
  canAddRecord = true,
  selectionResetKey = 0,
  onEditRecord,
  onDeleteRecord,
  onBulkDelete,
  onJumpToRef,
  readOnly = false,
  hideCheckboxColumn = false,
  onRowClick,
  pendingEdits,
  pendingEditCount = 0,
  savingEdits = false,
  onCellEdit,
  onSaveEdits,
  onDiscardEdits,
}: DataTableProps) {
  const [selectedKeys, setSelectedKeys] = useState<Set<string>>(new Set());
  const [bulkDeleteOpen, setBulkDeleteOpen] = useState(false);
  const [columnWidths, setColumnWidths] = useState<Record<string, number>>(() => initialWidths(columns));
  const [columnOrder, setColumnOrder] = useState<string[]>(() => columns.map((c) => c.name));
  const [draggedCol, setDraggedCol] = useState<string | null>(null);
  const [dragOverCol, setDragOverCol] = useState<string | null>(null);
  const [editingCell, setEditingCell] = useState<{ rowPk: string; columnName: string } | null>(null);

  useEffect(() => {
    setSelectedKeys(new Set());
  }, [columns]);

  useEffect(() => {
    setSelectedKeys(new Set());
  }, [selectionResetKey]);

  useEffect(() => {
    setEditingCell(null);
  }, [columns]);

  const orderedColumns = columnOrder
    .map((name) => columns.find((c) => c.name === name))
    .filter((c): c is ColumnDef => c !== undefined);

  const pkColumns = orderedColumns.filter((c) => c.isPrimaryKey);

  function handleResizeColumn(name: string, width: number) {
    setColumnWidths((prev) => ({ ...prev, [name]: width }));
  }

  function handleDragStart(colName: string) {
    setDraggedCol(colName);
  }

  function handleDragOver(colName: string) {
    if (draggedCol && colName !== draggedCol) setDragOverCol(colName);
  }

  function handleDrop(colName: string) {
    if (draggedCol && draggedCol !== colName) {
      setColumnOrder((prev) => {
        const next = [...prev];
        const from = next.indexOf(draggedCol);
        const to = next.indexOf(colName);
        next.splice(from, 1);
        next.splice(to, 0, draggedCol);
        return next;
      });
    }
    setDraggedCol(null);
    setDragOverCol(null);
  }

  function handleDragEnd() {
    setDraggedCol(null);
    setDragOverCol(null);
  }

  function getRowKey(row: RecordRow): string {
    if (pkColumns.length === 0) {
      return JSON.stringify(row);
    }

    return JSON.stringify(
      Object.fromEntries(pkColumns.map((column) => [column.name, row[column.name] ?? null])),
    );
  }

  const paginatedKeys = rows.map(getRowKey);
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
      if (next.has(key)) {
        next.delete(key);
      } else {
        next.add(key);
      }
      return next;
    });
  }

  function handleBulkDeleteConfirm() {
    onBulkDelete(rows.filter((r) => selectedKeys.has(getRowKey(r))));
    setSelectedKeys(new Set());
  }

  function handleCellDoubleClick(row: RecordRow, columnName: string) {
    if (!onCellEdit || pkColumns.length === 0) return;
    const rowPk = serializeRowPk(row, pkColumns);
    setEditingCell({ rowPk, columnName });
  }

  function handleCommitCellEdit(row: RecordRow, columnName: string, value: FieldValue) {
    if (onCellEdit) {
      onCellEdit(row, columnName, value);
    }
    setEditingCell(null);
  }

  function handleCancelCellEdit() {
    setEditingCell(null);
  }

  const showCheckbox = !readOnly && !hideCheckboxColumn;
  const tableWidth =
    (showCheckbox ? READ_WRITE_SELECTION_COLUMN_WIDTH : 0) +
    orderedColumns.reduce((sum, col) => sum + (columnWidths[col.name] ?? colWidth(col)), 0);
  const colgroup = (
    <colgroup>
      {showCheckbox && <col style={{ width: READ_WRITE_SELECTION_COLUMN_WIDTH }} />}
      {orderedColumns.map((col) => (
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
        canAddRecord={canAddRecord}
        selectedCount={selectedKeys.size}
        onBulkDelete={() => setBulkDeleteOpen(true)}
        readOnly={readOnly}
        pendingEditCount={pendingEditCount}
        savingEdits={savingEdits}
        onSaveEdits={onSaveEdits}
        onDiscardEdits={onDiscardEdits}
      />
      <div className="relative min-h-0 flex-1 overflow-auto overscroll-none">
        <table className="table-fixed border-separate border-spacing-0" style={{ minWidth: tableWidth, width: tableWidth }}>
          {colgroup}
          <DataTableHeader
            columns={orderedColumns}
            sort={sort}
            onSortChange={onSortChange}
            allSelected={allSelected}
            someSelected={someSelected}
            onToggleAll={toggleAll}
            columnWidths={columnWidths}
            onResizeColumn={handleResizeColumn}
            draggedCol={draggedCol}
            dragOverCol={dragOverCol}
            onDragStart={handleDragStart}
            onDragOver={handleDragOver}
            onDrop={handleDrop}
            onDragEnd={handleDragEnd}
            readOnly={readOnly}
            hideCheckboxColumn={hideCheckboxColumn}
          />
          <TableBody>
            {rows.length === 0 ? (
              <tr>
                <td colSpan={orderedColumns.length + (showCheckbox ? 1 : 0)} className="py-16 text-center text-xs text-muted-foreground">
                  {filter ? "No records match your filter." : "No records found."}
                </td>
              </tr>
            ) : (
              rows.map((row, i) => {
                const rowPk = pkColumns.length > 0 ? serializeRowPk(row, pkColumns) : null;
                const pendingCellEdits = rowPk ? pendingEdits?.get(rowPk) : undefined;
                const isEditingRow = rowPk !== null && editingCell?.rowPk === rowPk;

                return (
                  <DataTableRow
                    key={i}
                    row={row}
                    columns={orderedColumns}
                    isSelected={selectedKeys.has(getRowKey(row))}
                    onToggleSelect={() => toggleRow(getRowKey(row))}
                    onEdit={onEditRecord}
                    onDelete={onDeleteRecord}
                    onJumpToRef={onJumpToRef}
                    readOnly={readOnly}
                    hideCheckboxColumn={hideCheckboxColumn}
                    onRowClick={onRowClick}
                    pendingCellEdits={pendingCellEdits}
                    editingColumnName={isEditingRow ? editingCell?.columnName : null}
                    onCellDoubleClick={
                      onCellEdit ? (colName) => handleCellDoubleClick(row, colName) : undefined
                    }
                    onCommitCellEdit={
                      onCellEdit
                        ? (colName, value) => handleCommitCellEdit(row, colName, value)
                        : undefined
                    }
                    onCancelCellEdit={handleCancelCellEdit}
                  />
                );
              })
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
      {showCheckbox && (
        <DeleteConfirmDialog
          open={bulkDeleteOpen}
          onOpenChange={setBulkDeleteOpen}
          count={selectedKeys.size}
          onConfirm={handleBulkDeleteConfirm}
        />
      )}
    </div>
  );
}
