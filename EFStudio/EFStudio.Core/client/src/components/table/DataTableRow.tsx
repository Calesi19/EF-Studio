import { Checkbox } from "@/components/ui/checkbox";
import { TableCell, TableRow } from "@/components/ui/table";
import type { ColumnDef, FieldValue, RecordRow, TableDef } from "@/types";
import { DataTableCell } from "./DataTableCell";

interface DataTableRowProps {
  row: RecordRow;
  columns: ColumnDef[];
  isSelected: boolean;
  onToggleSelect: () => void;
  onEdit: (row: RecordRow) => void;
  onDelete: (row: RecordRow) => void;
  onJumpToRef: (tableName: string, value: FieldValue) => void;
  allTables: TableDef[];
}

export function DataTableRow({ row, columns, isSelected, onToggleSelect, onJumpToRef, allTables }: DataTableRowProps) {
  return (
    <TableRow className={`group h-9 ${isSelected ? "bg-primary/5 hover:bg-primary/10" : "hover:bg-muted/30"}`}>
      <TableCell
        className={`sticky left-0 z-10 px-0 py-0 align-middle shadow-[inset_-1px_0_0_var(--color-border)] ${
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
      {columns.map((col) => (
        <TableCell key={col.name} className="px-3 py-0 border-r border-border overflow-hidden">
          <div className="flex items-center h-9">
            <DataTableCell
              value={row[col.name] ?? null}
              column={col}
              onJumpToRef={col.isForeignKey && col.foreignKeyTable ? onJumpToRef : undefined}
              allTables={allTables}
            />
          </div>
        </TableCell>
      ))}
    </TableRow>
  );
}
