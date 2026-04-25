import { Badge } from "@/components/ui/badge";
import {
  Calendar03Icon,
  CheckmarkSquare03Icon,
  CodeIcon,
  FingerPrintIcon,
  HashtagIcon,
  TextSmallcapsIcon,
} from "@hugeicons/core-free-icons";
import { HugeiconsIcon } from "@hugeicons/react";
import type { ColumnDef } from "@/types";

interface ColumnTypeBadgeProps {
  column: ColumnDef;
}

function TypeIcon({ column }: { column: ColumnDef }) {
  const cls = "shrink-0 text-muted-foreground/70";
  const size = 11;
  switch (column.type) {
    case "datetime": return <HugeiconsIcon icon={Calendar03Icon} size={size} className={cls} />;
    case "number":   return <HugeiconsIcon icon={HashtagIcon} size={size} className={cls} />;
    case "boolean":  return <HugeiconsIcon icon={CheckmarkSquare03Icon} size={size} className={cls} />;
    case "uuid":     return <HugeiconsIcon icon={FingerPrintIcon} size={size} className={cls} />;
    case "json":     return <HugeiconsIcon icon={CodeIcon} size={size} className={cls} />;
    case "string":   return <HugeiconsIcon icon={TextSmallcapsIcon} size={size} className={cls} />;
    default:         return null;
  }
}

export function ColumnTypeBadge({ column }: ColumnTypeBadgeProps) {
  return (
    <>
      <TypeIcon column={column} />
      {column.isPrimaryKey && (
        <Badge variant="outline" className="text-[10px] px-1 py-0 font-mono border-amber-500/50 text-amber-600 dark:text-amber-400 shrink-0">
          PK
        </Badge>
      )}
      {column.isForeignKey && (
        <Badge variant="outline" className="text-[10px] px-1 py-0 font-mono border-blue-500/50 text-blue-600 dark:text-blue-400 shrink-0">
          FK
        </Badge>
      )}
      {!column.isPrimaryKey && !column.isForeignKey && column.isNullable && (
        <Badge variant="outline" className="text-[10px] px-1 py-0 font-mono text-muted-foreground shrink-0">
          ?
        </Badge>
      )}
    </>
  );
}
