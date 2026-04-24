import { cn } from "@/lib/utils";

interface TableListItemProps {
  name: string;
  displayName: string;
  rowCount: number;
  isSelected: boolean;
  onClick: () => void;
}

export function TableListItem({ name: _name, displayName, rowCount, isSelected, onClick }: TableListItemProps) {
  return (
    <button
      onClick={onClick}
      className={cn(
        "flex w-full items-center justify-between rounded px-2 py-1 text-left text-xs transition-colors",
        isSelected
          ? "bg-sidebar-accent text-sidebar-accent-foreground font-medium"
          : "text-sidebar-foreground hover:bg-sidebar-accent/50"
      )}
    >
      <div className="flex items-center gap-2 min-w-0">
        <span className="text-muted-foreground text-xs font-mono shrink-0">▸</span>
        <span className="truncate">{displayName}</span>
      </div>
      <span className="ml-2 shrink-0 text-xs text-muted-foreground tabular-nums">{rowCount}</span>
    </button>
  );
}
