import { Badge } from "@/components/ui/badge";
import { Tooltip, TooltipContent, TooltipTrigger } from "@/components/ui/tooltip";
import type { ColumnDef, FieldValue, TableDef } from "@/types";
import { ArrowRight01Icon } from "@hugeicons/core-free-icons";
import { HugeiconsIcon } from "@hugeicons/react";

interface DataTableCellProps {
  value: FieldValue;
  column: ColumnDef;
  onJumpToRef?: (tableName: string, value: FieldValue) => void;
  allTables: TableDef[];
}

function formatDatetime(raw: string): string {
  const d = new Date(raw);
  if (isNaN(d.getTime())) return raw;
  const date = d.toISOString().slice(0, 10);
  const time = d.toISOString().slice(11, 16);
  return `${date} ${time}`;
}

function TruncatedText({ text, className }: { text: string; className?: string }) {
  return (
    <Tooltip>
      <TooltipTrigger asChild>
        <span className={`truncate block w-full cursor-default ${className ?? ""}`}>{text}</span>
      </TooltipTrigger>
      <TooltipContent side="bottom" className="max-w-xs break-all text-xs font-mono">
        {text}
      </TooltipContent>
    </Tooltip>
  );
}

export function DataTableCell({ value, column, onJumpToRef, allTables: _allTables }: DataTableCellProps) {
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

  if (column.isForeignKey && column.foreignKeyTable && onJumpToRef) {
    return (
      <div className="flex items-center gap-1 min-w-0">
        <TruncatedText
          text={String(value)}
          className="font-mono text-xs text-blue-600 dark:text-blue-400 min-w-0"
        />
        <button
          onClick={(e) => { e.stopPropagation(); onJumpToRef(column.foreignKeyTable!, value); }}
          className="opacity-0 group-hover:opacity-100 shrink-0 flex h-4 w-4 items-center justify-center rounded text-blue-500 hover:bg-blue-500/10 transition-all"
          title={`Go to ${column.foreignKeyTable}`}
        >
          <HugeiconsIcon icon={ArrowRight01Icon} size={10} />
        </button>
      </div>
    );
  }

  if (column.isPrimaryKey) {
    return <TruncatedText text={String(value)} className="font-mono text-xs text-primary" />;
  }

  if (column.type === "datetime") {
    const formatted = formatDatetime(String(value));
    return <TruncatedText text={formatted} className="text-xs tabular-nums text-muted-foreground" />;
  }

  if (column.type === "number") {
    return <span className="text-xs tabular-nums font-mono">{String(value)}</span>;
  }

  return <TruncatedText text={String(value)} className="text-xs" />;
}
