import { TableHead, TableHeader, TableRow } from "@/components/ui/table";
import type { ColumnDef, SortState } from "@/types";
import { useEffect, useRef } from "react";
import { ColumnTypeBadge } from "./ColumnTypeBadge";

interface DataTableHeaderProps {
  columns: ColumnDef[];
  sort: SortState;
  onSortChange: (column: string) => void;
  allSelected: boolean;
  someSelected: boolean;
  onToggleAll: () => void;
  columnWidths: Record<string, number>;
  onResizeColumn: (name: string, width: number) => void;
}

const MIN_COL_WIDTH = 60;

export function DataTableHeader({
  columns,
  sort,
  onSortChange,
  allSelected,
  someSelected,
  onToggleAll,
  columnWidths,
  onResizeColumn,
}: DataTableHeaderProps) {
  const checkboxRef = useRef<HTMLInputElement>(null);

  useEffect(() => {
    if (checkboxRef.current) {
      checkboxRef.current.indeterminate = someSelected && !allSelected;
    }
  }, [someSelected, allSelected]);

  function handleResizeStart(e: React.MouseEvent, colName: string) {
    e.preventDefault();
    e.stopPropagation();

    const startX = e.clientX;
    const startWidth = columnWidths[colName] ?? MIN_COL_WIDTH;

    document.body.style.cursor = "col-resize";
    document.body.style.userSelect = "none";

    function onMouseMove(e: MouseEvent) {
      const newWidth = Math.max(MIN_COL_WIDTH, startWidth + (e.clientX - startX));
      onResizeColumn(colName, newWidth);
    }

    function onMouseUp() {
      document.body.style.cursor = "";
      document.body.style.userSelect = "";
      document.removeEventListener("mousemove", onMouseMove);
      document.removeEventListener("mouseup", onMouseUp);
    }

    document.addEventListener("mousemove", onMouseMove);
    document.addEventListener("mouseup", onMouseUp);
  }

  return (
    <TableHeader>
      <TableRow className="bg-muted hover:bg-muted">
        <TableHead className="px-3 py-1.5 h-auto border-r border-border">
          <input
            ref={checkboxRef}
            type="checkbox"
            checked={allSelected}
            onChange={onToggleAll}
            className="h-3.5 w-3.5 cursor-pointer accent-primary"
          />
        </TableHead>
        {columns.map((col) => (
          <TableHead
            key={col.name}
            className="relative cursor-pointer select-none px-3 py-1.5 h-auto border-r border-border overflow-hidden"
            onClick={() => onSortChange(col.name)}
          >
            <div className="flex items-center gap-1.5 min-w-0 overflow-hidden pr-1">
              <span className="text-xs font-medium text-muted-foreground truncate min-w-0">
                {col.name}
              </span>
              <ColumnTypeBadge column={col} />
              {sort.column === col.name && (
                <span className="text-xs text-muted-foreground shrink-0">
                  {sort.direction === "asc" ? "↑" : "↓"}
                </span>
              )}
            </div>
            {/* Resize handle */}
            <div
              className="absolute right-0 top-0 h-full w-1 cursor-col-resize hover:bg-primary/40 active:bg-primary/60 transition-colors z-10"
              onMouseDown={(e) => handleResizeStart(e, col.name)}
              onClick={(e) => e.stopPropagation()}
            />
          </TableHead>
        ))}
        <TableHead className="w-8 px-1 py-1.5 h-auto" />
      </TableRow>
    </TableHeader>
  );
}
