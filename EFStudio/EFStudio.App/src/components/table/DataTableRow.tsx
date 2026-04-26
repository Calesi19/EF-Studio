import { Checkbox } from "@/components/ui/checkbox";
import { TableCell, TableRow } from "@/components/ui/table";
import type { ColumnDef, FieldValue, RecordRow } from "@/types";
import { DataTableCell } from "./DataTableCell";

interface DataTableRowProps {
  row: RecordRow;
  columns: ColumnDef[];
  isSelected: boolean;
  onToggleSelect: () => void;
  onEdit: (row: RecordRow) => void;
  onDelete: (row: RecordRow) => void;
  onJumpToRef: (tableKey: string, value: FieldValue) => void;
  readOnly?: boolean;
  hideCheckboxColumn?: boolean;
  onRowClick?: (row: RecordRow) => void;
  pendingCellEdits?: Record<string, FieldValue>;
  editingColumnName?: string | null;
  onCellDoubleClick?: (columnName: string) => void;
  onCommitCellEdit?: (columnName: string, value: FieldValue) => void;
  onCancelCellEdit?: () => void;
}

export function DataTableRow({
  row,
  columns,
  isSelected,
  onToggleSelect,
  onJumpToRef,
  readOnly = false,
  hideCheckboxColumn = false,
  onRowClick,
  pendingCellEdits,
  editingColumnName,
  onCellDoubleClick,
  onCommitCellEdit,
  onCancelCellEdit,
}: DataTableRowProps) {
  const hasPending = pendingCellEdits && Object.keys(pendingCellEdits).length > 0;

  return (
    <TableRow
      onClick={onRowClick ? () => onRowClick(row) : undefined}
      className={`group h-9 ${onRowClick ? "cursor-pointer" : ""} ${
        isSelected
          ? "bg-primary/5 hover:bg-primary/10"
          : hasPending
            ? "hover:bg-amber-500/5"
            : "hover:bg-muted/30"
      }`}
    >
      {!readOnly && !hideCheckboxColumn && (
        <TableCell
          className={`sticky left-0 z-10 border-b border-border px-0 py-0 align-middle shadow-[inset_-1px_0_0_var(--color-border)] ${
            isSelected
              ? "bg-[color-mix(in_oklch,var(--color-primary)_5%,var(--color-background))] group-hover:bg-[color-mix(in_oklch,var(--color-primary)_10%,var(--color-background))]"
              : "bg-background group-hover:bg-[color-mix(in_oklch,var(--color-muted)_30%,var(--color-background))]"
          }`}
        >
          <div className="flex items-center justify-center h-9">
            <Checkbox
              checked={isSelected}
              onCheckedChange={onToggleSelect}
              className="h-3.5 w-3.5"
            />
          </div>
        </TableCell>
      )}
      {columns.map((col) => {
        const effectiveValue =
          pendingCellEdits && col.name in pendingCellEdits
            ? pendingCellEdits[col.name]
            : (row[col.name] ?? null);
        const isPendingEdit = !!(pendingCellEdits && col.name in pendingCellEdits);
        const isRequiredAndEmpty =
          isPendingEdit &&
          !col.isNullable &&
          !col.isPrimaryKey &&
          (effectiveValue === null || effectiveValue === "");
        const isEditing = editingColumnName === col.name;

        return (
          <TableCell
            key={col.name}
            className={`overflow-hidden border-r border-b border-border px-0 py-0 ${isPendingEdit ? "bg-amber-500/8 border-l-2 border-l-amber-400/60" : ""} ${isEditing ? "p-0" : ""}`}
          >
            <div className={`flex items-center h-9 ${isEditing ? "px-0" : "px-3"}`}>
              <DataTableCell
                value={effectiveValue}
                column={col}
                onJumpToRef={col.isForeignKey && col.foreignKeyTable ? onJumpToRef : undefined}
                isEditing={isEditing}
                isRequiredAndEmpty={isRequiredAndEmpty}
                onDoubleClick={
                  !readOnly && !col.isPrimaryKey && onCellDoubleClick
                    ? () => onCellDoubleClick(col.name)
                    : undefined
                }
                onCommitEdit={
                  isEditing && onCommitCellEdit
                    ? (value) => onCommitCellEdit(col.name, value)
                    : undefined
                }
                onCancelEdit={isEditing ? onCancelCellEdit : undefined}
              />
            </div>
          </TableCell>
        );
      })}
    </TableRow>
  );
}
