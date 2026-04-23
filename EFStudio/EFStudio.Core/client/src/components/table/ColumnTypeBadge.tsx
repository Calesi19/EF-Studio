import { Badge } from "@/components/ui/badge";
import { cn } from "@/lib/utils";
import type { ColumnDef } from "@/types";

interface ColumnTypeBadgeProps {
  column: ColumnDef;
}

export function ColumnTypeBadge({ column }: ColumnTypeBadgeProps) {
  if (column.isPrimaryKey) {
    return (
      <Badge variant="outline" className="text-[10px] px-1 py-0 font-mono border-amber-500/50 text-amber-600 dark:text-amber-400">
        PK
      </Badge>
    );
  }
  if (column.isForeignKey) {
    return (
      <Badge variant="outline" className="text-[10px] px-1 py-0 font-mono border-blue-500/50 text-blue-600 dark:text-blue-400">
        FK
      </Badge>
    );
  }
  if (column.isNullable) {
    return (
      <Badge variant="outline" className={cn("text-[10px] px-1 py-0 font-mono text-muted-foreground")}>
        ?
      </Badge>
    );
  }
  return null;
}
