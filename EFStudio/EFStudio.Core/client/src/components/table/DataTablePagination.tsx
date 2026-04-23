import { Button } from "@/components/ui/button";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import type { PaginationState } from "@/types";

interface DataTablePaginationProps {
  pagination: PaginationState;
  totalRows: number;
  totalPages: number;
  onPageChange: (page: number) => void;
  onPageSizeChange: (size: number) => void;
}

const PAGE_SIZES = [10, 20, 50, 100];

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
    <div className="flex items-center justify-between border-t border-border px-6 py-3">
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
            <SelectTrigger className="h-7 w-16 text-xs">
              <SelectValue />
            </SelectTrigger>
            <SelectContent>
              {PAGE_SIZES.map((size) => (
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
            {pagination.page} / {totalPages}
          </span>
          <Button
            variant="outline"
            size="sm"
            className="h-7 w-7 p-0 text-xs"
            disabled={pagination.page >= totalPages}
            onClick={() => onPageChange(pagination.page + 1)}
          >
            ›
          </Button>
          <Button
            variant="outline"
            size="sm"
            className="h-7 w-7 p-0 text-xs"
            disabled={pagination.page >= totalPages}
            onClick={() => onPageChange(totalPages)}
          >
            »
          </Button>
        </div>
      </div>
    </div>
  );
}
