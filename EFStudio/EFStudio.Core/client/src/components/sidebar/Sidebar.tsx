import { ScrollArea } from "@/components/ui/scroll-area";
import { Separator } from "@/components/ui/separator";
import type { TableDef } from "@/types";
import { SidebarHeader } from "./SidebarHeader";
import { TableListItem } from "./TableListItem";

interface SidebarProps {
  tables: TableDef[];
  recordCounts: Map<string, number>;
  selectedTableName: string | null;
  onSelectTable: (name: string) => void;
}

export function Sidebar({ tables, recordCounts, selectedTableName, onSelectTable }: SidebarProps) {
  return (
    <div className="flex h-full flex-col">
      <SidebarHeader />
      <div className="px-3 py-2">
        <p className="px-1 text-xs font-medium uppercase tracking-wider text-muted-foreground">
          Models
        </p>
      </div>
      <Separator className="bg-sidebar-border" />
      <ScrollArea className="flex-1 py-2">
        <div className="px-2 space-y-0.5">
          {tables.map((table) => (
            <TableListItem
              key={table.name}
              name={table.name}
              displayName={table.displayName}
              rowCount={recordCounts.get(table.name) ?? 0}
              isSelected={selectedTableName === table.name}
              onClick={() => onSelectTable(table.name)}
            />
          ))}
        </div>
      </ScrollArea>
    </div>
  );
}
