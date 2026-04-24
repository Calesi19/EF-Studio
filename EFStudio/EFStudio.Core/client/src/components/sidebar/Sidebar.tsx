import { Input } from "@/components/ui/input";
import { ScrollArea } from "@/components/ui/scroll-area";
import type { TableDef } from "@/types";
import { useState } from "react";
import { TableListItem } from "./TableListItem";
import { ThemeToggle } from "./ThemeToggle";

interface SidebarProps {
  tables: TableDef[];
  recordCounts: Map<string, number>;
  selectedTableName: string | null;
  onSelectTable: (name: string) => void;
}

export function Sidebar({
  tables,
  recordCounts,
  selectedTableName,
  onSelectTable,
}: SidebarProps) {
  const [modelFilter, setModelFilter] = useState("");

  const filteredTables = modelFilter.trim()
    ? tables.filter(
        (t) =>
          t.displayName.toLowerCase().includes(modelFilter.toLowerCase()) ||
          t.name.toLowerCase().includes(modelFilter.toLowerCase()),
      )
    : tables;

  return (
    <div className="flex h-full flex-col">
      <div className="px-2 py-1.5">
        <Input
          placeholder="Models"
          value={modelFilter}
          onChange={(e) => setModelFilter(e.target.value)}
          className="h-7 text-xs"
        />
      </div>
      <ScrollArea className="flex-1 py-1">
        <div className="px-1.5 space-y-px">
          {filteredTables.map((table) => (
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
      <div className="px-1.5 py-1.5 border-t border-sidebar-border">
        <ThemeToggle />
      </div>
    </div>
  );
}
