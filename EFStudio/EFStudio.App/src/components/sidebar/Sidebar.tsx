import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { Input } from "@/components/ui/input";
import type { DbContextDef, TableDef } from "@/types";
import { useState } from "react";
import { TableListItem } from "./TableListItem";
import { SettingsButton } from "./SettingsButton";
import { useStudioContext } from "@/pages/StudioPage/context/StudioContext";

const DEFAULT_SCHEMA_GROUP_LABEL = "default";

interface SidebarProps {
  contexts: DbContextDef[];
  selectedContextName: string | null;
  tables: TableDef[];
  selectedTableKey: string | null;
  onSelectTable: (tableKey: string) => void;
  onSelectContext: (contextName: string) => void;
}

export function Sidebar({
  contexts,
  selectedContextName,
  tables,
  selectedTableKey,
  onSelectTable,
  onSelectContext,
}: SidebarProps) {
  const [modelFilter, setModelFilter] = useState("");
  const { nameDisplay } = useStudioContext();

  const q = modelFilter.toLowerCase();
  const filteredTables = modelFilter.trim()
    ? tables.filter(
        (t) =>
          t.name.toLowerCase().includes(q) ||
          t.displayName.toLowerCase().includes(q) ||
          t.modelDisplayName.toLowerCase().includes(q) ||
          t.schema?.toLowerCase().includes(q) === true,
      )
    : tables;
  const hasSchemaGroups = filteredTables.some((table) => !!table.schema);
  const sortedTables = [...filteredTables].sort((left, right) => {
    const leftName = nameDisplay === "model" ? left.modelDisplayName : left.name;
    const rightName = nameDisplay === "model" ? right.modelDisplayName : right.name;

    return leftName.localeCompare(rightName, undefined, { sensitivity: "base" });
  });
  const groupedTables = sortedTables.reduce<Map<string, TableDef[]>>((groups, table) => {
    const groupKey = table.schema ?? DEFAULT_SCHEMA_GROUP_LABEL;
    const existingGroup = groups.get(groupKey);

    if (existingGroup) {
      existingGroup.push(table);
      return groups;
    }

    groups.set(groupKey, [table]);
    return groups;
  }, new Map());
  const orderedGroups = [...groupedTables.entries()].sort(([left], [right]) =>
    left.localeCompare(right, undefined, { sensitivity: "base" }),
  );

  return (
    <div className="grid h-full min-h-0 grid-rows-[auto_minmax(0,1fr)_auto] overflow-hidden">
      <div className="space-y-2 px-2 py-1.5">
        {contexts.length > 1 ? (
          <Select value={selectedContextName ?? undefined} onValueChange={onSelectContext}>
            <SelectTrigger size="sm" className="w-full rounded-lg text-xs">
              <SelectValue placeholder="Choose DbContext" />
            </SelectTrigger>
            <SelectContent align="start">
              {contexts.map((context) => (
                <SelectItem key={context.name} value={context.name} disabled={!context.isAvailable}>
                  {context.name}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        ) : null}
        <Input
          placeholder="Models"
          value={modelFilter}
          onChange={(e) => setModelFilter(e.target.value)}
          className="h-7 text-xs"
        />
      </div>
      <div className="min-h-0 flex-1 overflow-y-auto overscroll-contain py-1">
        <div className="space-y-3 px-1.5 pb-1">
          {hasSchemaGroups
            ? orderedGroups.map(([schemaName, schemaTables]) => (
                <section key={schemaName} className="space-y-1">
                  <div className="truncate px-2 py-1 text-[10px] font-semibold uppercase tracking-[0.14em] text-muted-foreground">
                    {schemaName}
                  </div>
                  <div className="space-y-px">
                    {schemaTables.map((table) => {
                      const effectiveName =
                        nameDisplay === "model" ? table.modelDisplayName : table.name;

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
                </section>
              ))
            : sortedTables.map((table) => {
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
