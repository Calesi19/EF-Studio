import { Badge } from "@/components/ui/badge";
import type { ColumnDef, FieldValue } from "@/types";

interface DataTableCellProps {
  value: FieldValue;
  column: ColumnDef;
}

function formatDatetime(raw: string): string {
  const d = new Date(raw);
  if (isNaN(d.getTime())) return raw;
  const date = d.toISOString().slice(0, 10);
  const time = d.toISOString().slice(11, 16);
  return `${date} ${time}`;
}

export function DataTableCell({ value, column }: DataTableCellProps) {
  if (value === null || value === undefined) {
    return <span className="text-muted-foreground italic text-xs">null</span>;
  }

  if (column.type === "boolean") {
    return value ? (
      <Badge className="bg-emerald-500/15 text-emerald-700 dark:text-emerald-400 border-emerald-500/30 font-mono text-xs shrink-0" variant="outline">
        true
      </Badge>
    ) : (
      <Badge className="text-muted-foreground font-mono text-xs shrink-0" variant="outline">
        false
      </Badge>
    );
  }

  if (column.isPrimaryKey || column.isForeignKey) {
    const colorClass = column.isPrimaryKey
      ? "text-primary"
      : "text-blue-600 dark:text-blue-400";
    return (
      <span
        className={`font-mono text-xs ${colorClass} truncate block w-full`}
        title={String(value)}
      >
        {String(value)}
      </span>
    );
  }

  if (column.type === "datetime") {
    const formatted = formatDatetime(String(value));
    return (
      <span className="text-xs tabular-nums text-muted-foreground truncate block w-full" title={formatted}>
        {formatted}
      </span>
    );
  }

  if (column.type === "number") {
    return <span className="text-xs tabular-nums font-mono">{String(value)}</span>;
  }

  return (
    <span className="text-xs truncate block w-full" title={String(value)}>
      {String(value)}
    </span>
  );
}
