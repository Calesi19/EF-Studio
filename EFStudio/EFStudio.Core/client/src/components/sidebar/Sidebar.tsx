import { Input } from "@/components/ui/input";
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
    <div className="grid h-full min-h-0 grid-rows-[auto_minmax(0,1fr)_auto] overflow-hidden">
      <div className="px-2 py-1.5">
        <Input
          placeholder="Models"
          value={modelFilter}
          onChange={(e) => setModelFilter(e.target.value)}
          className="h-7 text-xs"
        />
      </div>
      <div className="min-h-0 flex-1 overflow-y-auto overscroll-contain py-1">
        <div className="space-y-px px-1.5 pb-1">
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
      </div>
      <div className="px-1.5 py-2.5 border-t border-sidebar-border">
        <ThemeToggle />
      </div>
    </div>
  );
}
