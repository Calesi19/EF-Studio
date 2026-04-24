import { cn } from "@/lib/utils";
import type { TabState, TableDef } from "@/types";
import { SidebarLeft01Icon } from "@hugeicons/core-free-icons";
import { HugeiconsIcon } from "@hugeicons/react";

interface TabBarProps {
  tabs: TabState[];
  activeTabId: string | null;
  tables: TableDef[];
  recordCounts: Map<string, number>;
  onActivate: (id: string) => void;
  onClose: (id: string) => void;
  onCloseAll: () => void;
  sidebarOpen: boolean;
  onToggleSidebar: () => void;
}

export function TabBar({ tabs, activeTabId, tables, recordCounts: _recordCounts, onActivate, onClose, onCloseAll, sidebarOpen, onToggleSidebar }: TabBarProps) {
  return (
    <div className="flex border-b border-border bg-muted/30 overflow-x-auto shrink-0 [scrollbar-width:none] [&::-webkit-scrollbar]:hidden">
      {tabs.length > 0 && (
        <button
          onClick={onToggleSidebar}
          className="flex h-9 w-9 shrink-0 items-center justify-center border-r border-border text-muted-foreground hover:text-foreground hover:bg-muted/50 transition-colors"
          title={sidebarOpen ? "Collapse sidebar" : "Expand sidebar"}
        >
          <HugeiconsIcon icon={SidebarLeft01Icon} size={14} />
        </button>
      )}
      {tabs.map((tab) => {
        const table = tables.find((t) => t.name === tab.tableName);
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
          className="flex items-center gap-1 px-3 shrink-0 border-r border-border text-xs font-normal text-muted-foreground hover:text-foreground hover:bg-muted/50 transition-colors h-9 select-none"
        >
          Close all
        </button>
      )}
    </div>
  );
}
