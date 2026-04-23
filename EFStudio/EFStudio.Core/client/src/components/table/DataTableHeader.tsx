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
      <TableRow className="bg-muted/50 hover:bg-muted/50">
        {columns.map((col) => (
          <TableHead
            key={col.name}
            className="cursor-pointer select-none whitespace-nowrap px-4 py-2"
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
        <TableHead className="w-10 px-2 py-2" />
      </TableRow>
    </TableHeader>
  );
}
