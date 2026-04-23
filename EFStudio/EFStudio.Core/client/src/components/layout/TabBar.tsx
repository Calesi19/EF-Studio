import { cn } from "@/lib/utils";
import type { TabState, TableDef } from "@/types";

interface TabBarProps {
  tabs: TabState[];
  activeTabId: string | null;
  tables: TableDef[];
  recordCounts: Map<string, number>;
  onActivate: (id: string) => void;
  onClose: (id: string) => void;
  onCloseAll: () => void;
}

export function TabBar({ tabs, activeTabId, tables, recordCounts, onActivate, onClose, onCloseAll }: TabBarProps) {
  return (
    <div className="flex border-b border-border bg-muted/30 overflow-x-auto shrink-0 [scrollbar-width:none] [&::-webkit-scrollbar]:hidden">
      {tabs.map((tab) => {
        const table = tables.find((t) => t.name === tab.tableName);
        const count = recordCounts.get(tab.tableName) ?? 0;
        const isActive = tab.id === activeTabId;

        return (
          <div
            key={tab.id}
            onClick={() => onActivate(tab.id)}
            className={cn(
              "group flex items-center gap-2 px-3 border-r border-border cursor-pointer shrink-0 select-none h-9 text-xs transition-colors",
              isActive
                ? "bg-background text-foreground font-medium"
                : "text-muted-foreground hover:bg-muted/50 hover:text-foreground"
            )}
          >
            <span>{table?.displayName ?? tab.tableName}</span>
            <span className="text-[10px] tabular-nums text-muted-foreground">{count}</span>
            <button
              onClick={(e) => { e.stopPropagation(); onClose(tab.id); }}
              className={cn(
                "flex h-3.5 w-3.5 items-center justify-center rounded text-muted-foreground hover:text-foreground hover:bg-muted transition-colors leading-none",
                isActive ? "opacity-50 hover:opacity-100" : "opacity-0 group-hover:opacity-50 hover:!opacity-100"
              )}
              tabIndex={-1}
            >
              ×
            </button>
          </div>
        );
      })}
      {tabs.length >= 2 && (
        <button
          onClick={onCloseAll}
          className="flex items-center gap-1 px-3 shrink-0 border-r border-border text-[11px] text-muted-foreground hover:text-foreground hover:bg-muted/50 transition-colors h-9 select-none"
        >
          Close all
        </button>
      )}
    </div>
  );
}
