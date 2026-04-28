import { Button } from "@/components/ui/button";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import type { PaginationState } from "@/types";

const PAGE_SIZE_OPTIONS = [10, 20, 50, 100] as const;

interface DataTablePaginationProps {
  pagination: PaginationState;
  totalRows: number;
  totalPages: number | undefined;
  onPageChange: (page: number) => void;
  onPageSizeChange: (size: number) => void;
}

export function DataTablePagination({
  pagination,
  totalRows,
  totalPages,
  onPageChange,
  onPageSizeChange,
}: DataTablePaginationProps) {
  const start = Math.min((pagination.page - 1) * pagination.pageSize + 1, totalRows);
  const end = Math.min(pagination.page * pagination.pageSize, totalRows);

  return (
    <div className="flex items-center justify-between border-t border-border px-4 py-1.5">
      <span className="text-xs text-muted-foreground">
        {totalRows === 0 ? "No records" : `${start}–${end} of ${totalRows.toLocaleString()} records`}
      </span>
      <div className="flex items-center gap-4">
        <div className="flex items-center gap-1.5">
          <span className="text-xs text-muted-foreground">Rows per page</span>
          <Select
            value={String(pagination.pageSize)}
            onValueChange={(v) => onPageSizeChange(Number(v))}
          >
            <SelectTrigger className="h-7 w-20 text-xs">
              <SelectValue />
            </SelectTrigger>
            <SelectContent>
              {PAGE_SIZE_OPTIONS.map((size) => (
                <SelectItem key={size} value={String(size)} className="text-xs">
                  {size}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        </div>
        <div className="flex items-center gap-1">
          <Button
            variant="outline"
            size="sm"
            className="h-7 w-7 p-0 text-xs"
            disabled={pagination.page <= 1}
            onClick={() => onPageChange(1)}
          >
            «
          </Button>
          <Button
            variant="outline"
            size="sm"
            className="h-7 w-7 p-0 text-xs"
            disabled={pagination.page <= 1}
            onClick={() => onPageChange(pagination.page - 1)}
          >
            ‹
          </Button>
          <span className="px-2 text-xs text-muted-foreground">
            {pagination.page} / {totalPages ?? "–"}
          </span>
          <Button
            variant="outline"
            size="sm"
            className="h-7 w-7 p-0 text-xs"
            disabled={totalPages === undefined || pagination.page >= totalPages}
            onClick={() => onPageChange(pagination.page + 1)}
          >
            ›
          </Button>
          <Button
            variant="outline"
            size="sm"
            className="h-7 w-7 p-0 text-xs"
            disabled={totalPages === undefined || pagination.page >= totalPages}
            onClick={() => totalPages !== undefined && onPageChange(totalPages)}
          >
            »
          </Button>
        </div>
      </div>
    </div>
  );
}
