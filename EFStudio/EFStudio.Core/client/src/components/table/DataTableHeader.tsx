import { Checkbox } from "@/components/ui/checkbox";
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
  const checkboxRef = useRef<HTMLButtonElement>(null);

  useEffect(() => {
    if (checkboxRef.current) {
      checkboxRef.current.dataset.state = someSelected && !allSelected ? "indeterminate" : allSelected ? "checked" : "unchecked";
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
      <TableRow className="hover:bg-transparent">
        <TableHead className="sticky top-0 left-0 z-20 bg-muted px-3 py-1.5 h-auto border-b border-border shadow-[inset_-1px_0_0_var(--color-border),0_1px_3px_0_oklch(0_0_0/0.08)]">
          <Checkbox
            ref={checkboxRef}
            checked={allSelected ? true : someSelected ? "indeterminate" : false}
            onCheckedChange={onToggleAll}
            className="h-3.5 w-3.5"
          />
        </TableHead>
        {columns.map((col) => (
          <TableHead
            key={col.name}
            className="sticky top-0 z-10 bg-muted relative cursor-pointer select-none px-3 py-1.5 h-auto border-r border-b border-border overflow-hidden shadow-[0_1px_3px_0_oklch(0_0_0/0.08)]"
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
            <div
              className="absolute right-0 top-0 h-full w-1 cursor-col-resize hover:bg-primary/40 active:bg-primary/60 transition-colors z-10"
              onMouseDown={(e) => handleResizeStart(e, col.name)}
              onClick={(e) => e.stopPropagation()}
            />
          </TableHead>
        ))}
      </TableRow>
    </TableHeader>
  );
}
