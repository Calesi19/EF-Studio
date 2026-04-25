import { Skeleton } from "@/components/ui/skeleton";
import { Input } from "@/components/ui/input";
import { TableBody, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import type { ColumnDef } from "@/types";
import { ColumnTypeBadge } from "./ColumnTypeBadge";

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

interface DataTableSkeletonProps {
  columns: ColumnDef[];
  pageSize: number;
}

export function DataTableSkeleton({ columns, pageSize }: DataTableSkeletonProps) {
  const tableWidth =
    READ_WRITE_SELECTION_COLUMN_WIDTH +
    columns.reduce((sum, col) => sum + colWidth(col), 0);

  return (
    <div className="flex flex-1 flex-col overflow-hidden">
      <div className="flex items-center justify-between gap-3 border-b border-border px-4 py-2">
        <Input
          placeholder="Filter records..."
          value=""
          className="h-7 w-56 text-xs"
        />
        <Skeleton className="h-7 w-24" />
      </div>

      <div className="flex-1 overflow-auto min-h-0 overscroll-none">
        <table className="table-fixed" style={{ minWidth: tableWidth, width: tableWidth }}>
          <colgroup>
            <col style={{ width: READ_WRITE_SELECTION_COLUMN_WIDTH }} />
            {columns.map((col) => (
              <col key={col.name} style={{ width: colWidth(col) }} />
            ))}
          </colgroup>
          <TableHeader>
            <TableRow className="hover:bg-transparent">
              <TableHead className="sticky top-0 left-0 z-20 bg-muted px-0 py-1.5 h-auto border-b border-border align-middle shadow-[inset_-1px_0_0_var(--color-border),0_1px_3px_0_oklch(0_0_0/0.08)]">
                <div className="flex justify-center">
                  <Skeleton className="h-3.5 w-3.5 rounded-sm" />
                </div>
              </TableHead>
              {columns.map((col) => (
                <TableHead
                  key={col.name}
                  className="sticky top-0 z-10 bg-muted px-3 py-1.5 h-auto border-r border-b border-border shadow-[0_1px_3px_0_oklch(0_0_0/0.08)]"
                >
                  <div className="flex items-center gap-1.5 min-w-0 overflow-hidden">
                    <span className="text-xs font-medium text-muted-foreground truncate min-w-0">
                      {col.name}
                    </span>
                    <ColumnTypeBadge column={col} />
                  </div>
                </TableHead>
              ))}
            </TableRow>
          </TableHeader>
          <TableBody>
            {Array.from({ length: pageSize }, (_, i) => (
              <TableRow key={i} className="h-9 hover:bg-transparent">
                <td className="px-0 border-b border-border">
                  <div className="flex h-9 items-center justify-center">
                    <Skeleton className="h-3.5 w-3.5 rounded-sm" />
                  </div>
                </td>
                {columns.map((col) => (
                  <td key={col.name} className="border-r border-b border-border px-3 py-0">
                    <div className="flex h-9 items-center">
                      <Skeleton className="h-4 w-full" />
                    </div>
                  </td>
                ))}
              </TableRow>
            ))}
          </TableBody>
        </table>
      </div>

      <div className="flex items-center justify-between border-t border-border px-4 py-1.5">
        <Skeleton className="h-4 w-32" />
        <div className="flex items-center gap-4">
          <div className="flex items-center gap-1.5">
            <span className="text-xs text-muted-foreground">Rows per page</span>
            <Skeleton className="h-7 w-20" />
          </div>
          <div className="flex items-center gap-1">
            <Skeleton className="h-7 w-7 rounded-md" />
            <Skeleton className="h-7 w-7 rounded-md" />
            <Skeleton className="h-4 w-12" />
            <Skeleton className="h-7 w-7 rounded-md" />
            <Skeleton className="h-7 w-7 rounded-md" />
          </div>
        </div>
      </div>
    </div>
  );
}
