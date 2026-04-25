import { cn } from "@/lib/utils";

interface TableListItemProps {
  name: string;
  schema?: string;
  displayName: string;
  isSelected: boolean;
  onClick: () => void;
}

export function TableListItem({ name, schema, displayName, isSelected, onClick }: TableListItemProps) {
  const title = schema ? `${schema}.${name}` : name;

  return (
    <button
      onClick={onClick}
      title={title}
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
    </button>
  );
}
