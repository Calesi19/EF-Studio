import { Input } from "@/components/ui/input";
import type { TableDef } from "@/types";
import { useState } from "react";
import { TableListItem } from "./TableListItem";
import { SettingsButton } from "./SettingsButton";
import { useStudioContext } from "@/pages/StudioPage/context/StudioContext";

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
  const { nameDisplay } = useStudioContext();

  const q = modelFilter.toLowerCase();
  const filteredTables = modelFilter.trim()
    ? tables.filter(
        (t) =>
          t.displayName.toLowerCase().includes(q) ||
          t.name.toLowerCase().includes(q) ||
          t.modelDisplayName.toLowerCase().includes(q),
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
          {filteredTables.map((table) => {
            const effectiveName = nameDisplay === "model" ? table.modelDisplayName : table.name;
            return (
              <TableListItem
                key={table.key}
                name={table.name}
                schema={table.schema}
                displayName={effectiveName}
                isSelected={selectedTableKey === table.key}
                onClick={() => onSelectTable(table.key)}
              />
            );
          })}
        </div>
      </div>
      <div className="px-1.5 py-2.5 border-t border-sidebar-border">
        <SettingsButton />
      </div>
    </div>
  );
}
