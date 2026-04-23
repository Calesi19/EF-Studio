import { Badge } from "@/components/ui/badge";
import type { ColumnDef, FieldValue } from "@/types";

interface DataTableCellProps {
  value: FieldValue;
  column: ColumnDef;
}

export function DataTableCell({ value, column }: DataTableCellProps) {
  if (value === null || value === undefined) {
    return <span className="text-muted-foreground italic text-xs">null</span>;
  }

  if (column.type === "boolean") {
    return value ? (
      <Badge className="bg-emerald-500/15 text-emerald-700 dark:text-emerald-400 border-emerald-500/30 font-mono text-xs" variant="outline">
        true
      </Badge>
    ) : (
      <Badge className="text-muted-foreground font-mono text-xs" variant="outline">
        false
      </Badge>
    );
  }

  if (column.isPrimaryKey) {
    return (
      <span className="font-mono text-xs text-primary truncate block max-w-[160px]" title={String(value)}>
        {String(value)}
      </span>
    );
  }

  if (column.isForeignKey) {
    return (
      <span className="font-mono text-xs text-blue-600 dark:text-blue-400 truncate block max-w-[160px]" title={String(value)}>
        {String(value)}
      </span>
    );
  }

  if (column.type === "datetime") {
    const date = new Date(String(value));
    const formatted = isNaN(date.getTime()) ? String(value) : date.toLocaleString();
    return <span className="text-xs tabular-nums text-muted-foreground">{formatted}</span>;
  }

  if (column.type === "number") {
    return <span className="text-xs tabular-nums font-mono">{String(value)}</span>;
  }

  return (
    <span className="text-xs truncate block max-w-[240px]" title={String(value)}>
      {String(value)}
    </span>
  );
}
