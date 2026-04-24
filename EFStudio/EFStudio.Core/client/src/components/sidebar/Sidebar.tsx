import { Input } from "@/components/ui/input";
import { ScrollArea } from "@/components/ui/scroll-area";
import type { TableDef } from "@/types";
import { useState } from "react";
import { TableListItem } from "./TableListItem";
import { ThemeToggle } from "./ThemeToggle";

interface SidebarProps {
  tables: TableDef[];
  selectedTableKey: string | null;
  onSelectTable: (tableKey: string) => void;
}

export function Sidebar({
  tables,
  selectedTableKey,
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
              key={table.key}
              name={table.name}
              schema={table.schema}
              displayName={table.displayName}
              isSelected={selectedTableKey === table.key}
              onClick={() => onSelectTable(table.key)}
            />
          ))}
        </div>
      </ScrollArea>
      <div className="px-1.5 py-2.5 border-t border-sidebar-border">
        <ThemeToggle />
      </div>
    </div>
  );
}
