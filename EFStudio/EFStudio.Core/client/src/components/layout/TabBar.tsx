import { cn } from "@/lib/utils";
import type { TabState, TableDef } from "@/types";
import { SidebarLeft01Icon } from "@hugeicons/core-free-icons";
import { HugeiconsIcon } from "@hugeicons/react";
import { useStudioContext } from "@/pages/StudioPage/context/StudioContext";

interface TabBarProps {
  tabs: TabState[];
  activeTabId: string | null;
  tables: TableDef[];
  onActivate: (id: string) => void;
  onClose: (id: string) => void;
  onCloseAll: () => void;
  sidebarOpen: boolean;
  onToggleSidebar: () => void;
}

export function TabBar({ tabs, activeTabId, tables, onActivate, onClose, onCloseAll, sidebarOpen, onToggleSidebar }: TabBarProps) {
  const { nameDisplay } = useStudioContext();

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
        const table = tables.find((t) => t.key === tab.tableKey);
        const isActive = tab.id === activeTabId;
        const tabLabel = nameDisplay === "model" ? (table?.modelDisplayName ?? tab.tableKey) : (table?.name ?? tab.tableKey);

        return (
          <div
            key={tab.id}
            onClick={() => onActivate(tab.id)}
            className={cn(
              "group flex h-9 w-44 shrink-0 items-center gap-2 border-r border-border px-3 text-xs transition-colors cursor-pointer select-none",
              isActive
                ? "bg-background text-foreground font-medium"
                : "text-muted-foreground hover:bg-muted/50 hover:text-foreground"
            )}
            title={tabLabel}
          >
            <span className="min-w-0 flex-1 truncate">{tabLabel}</span>
            <button
              onClick={(e) => { e.stopPropagation(); onClose(tab.id); }}
              className={cn(
                "flex h-3.5 w-3.5 shrink-0 items-center justify-center rounded text-muted-foreground hover:text-foreground hover:bg-muted transition-colors leading-none",
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
