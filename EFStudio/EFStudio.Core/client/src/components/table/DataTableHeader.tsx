import { TableHead, TableHeader, TableRow } from "@/components/ui/table";
import type { ColumnDef, SortState } from "@/types";
import { ColumnTypeBadge } from "./ColumnTypeBadge";

interface DataTableHeaderProps {
  columns: ColumnDef[];
  sort: SortState;
  onSortChange: (column: string) => void;
}

export function DataTableHeader({ columns, sort, onSortChange }: DataTableHeaderProps) {
  return (
    <TableHeader>
      <TableRow className="bg-muted hover:bg-muted">
        {columns.map((col) => (
          <TableHead
            key={col.name}
            className="cursor-pointer select-none whitespace-nowrap px-3 py-1.5 h-auto"
            onClick={() => onSortChange(col.name)}
          >
            <div className="flex items-center gap-1.5">
              <span className="text-xs font-medium text-muted-foreground uppercase tracking-wide">
                {col.name}
              </span>
              <ColumnTypeBadge column={col} />
              {sort.column === col.name && (
                <span className="text-xs text-muted-foreground">
                  {sort.direction === "asc" ? "↑" : "↓"}
                </span>
              )}
            </div>
          </TableHead>
        ))}
        <TableHead className="w-8 px-1 py-1.5 h-auto" />
      </TableRow>
    </TableHeader>
  );
}
