import { useState } from "react";
import { Button } from "@/components/ui/button";
import { ScrollArea } from "@/components/ui/scroll-area";
import {
  Sheet,
  SheetContent,
  SheetFooter,
  SheetHeader,
  SheetTitle,
} from "@/components/ui/sheet";
import { Switch } from "@/components/ui/switch";
import { Label } from "@/components/ui/label";
import type { ColumnDef, FieldValue, RecordRow, TableDef } from "@/types";
import { RecordForm } from "./RecordForm";
import { FKPickerDrawer } from "./FKPickerDrawer";

const DEFAULT_NUMBER_VALUE = 0;

interface CreateRecordDrawerProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  tableDef: TableDef;
  draftRow: RecordRow;
  allTables: TableDef[];
  creating: boolean;
  error: string | null;
  onChangeField: (field: string, value: FieldValue) => void;
  onReset: () => void;
  onSubmit: () => Promise<void>;
}

function createInitialValue(column: ColumnDef): FieldValue {
  if (column.isPrimaryKey && column.type === "uuid") {
    return crypto.randomUUID();
  }
  if (column.type === "boolean") return false;
  if (column.type === "number") return DEFAULT_NUMBER_VALUE;
  return column.isNullable ? null : "";
}

export function buildCreateDraftRow(tableDef: TableDef): RecordRow {
  return Object.fromEntries(
    tableDef.columns
      .filter((column) => column.isEditableOnCreate)
      .map((column) => [column.name, createInitialValue(column)]),
  );
}

export function CreateRecordDrawer({
  open,
  onOpenChange,
  tableDef,
  draftRow,
  allTables,
  creating,
  error,
  onChangeField,
  onReset,
  onSubmit,
}: CreateRecordDrawerProps) {
  const [createMultiple, setCreateMultiple] = useState(false);
  const [fkPickerOpen, setFkPickerOpen] = useState(false);
  const [fkPickerColumn, setFkPickerColumn] = useState<string | null>(null);

  const fkRefTable = fkPickerColumn
    ? allTables.find((t) => {
        const col = tableDef.columns.find((c) => c.name === fkPickerColumn);
        return col?.foreignKeyTable === t.key;
      }) ?? null
    : null;

  function handleBrowseFK(columnName: string, _refTableKey: string) {
    setFkPickerColumn(columnName);
    setFkPickerOpen(true);
  }

  function handleFKSelect(value: FieldValue) {
    if (fkPickerColumn) {
      onChangeField(fkPickerColumn, value);
    }
    setFkPickerColumn(null);
  }

  async function handleSubmit() {
    await onSubmit();
    if (createMultiple) {
      onReset();
    } else {
      onOpenChange(false);
    }
  }

  function handleOpenChange(nextOpen: boolean) {
    if (!nextOpen) {
      setCreateMultiple(false);
      setFkPickerOpen(false);
      setFkPickerColumn(null);
    }
    onOpenChange(nextOpen);
  }

  return (
    <>
      <Sheet open={open} onOpenChange={handleOpenChange}>
        <SheetContent
          side="right"
          className="sm:max-w-2xl flex flex-col p-0 gap-0"
          showCloseButton={false}
        >
          <SheetHeader className="px-6 pt-5 pb-4 border-b shrink-0">
            <div className="flex items-center justify-between">
              <div>
                <SheetTitle>Insert row</SheetTitle>
                <p className="text-xs text-muted-foreground mt-0.5">
                  {tableDef.displayName ?? tableDef.name}
                </p>
              </div>
              <Button
                type="button"
                variant="ghost"
                size="sm"
                onClick={() => handleOpenChange(false)}
              >
                Close
              </Button>
            </div>
          </SheetHeader>

          <ScrollArea className="flex-1 min-h-0">
            <div className="px-6 py-5">
              <RecordForm
                columns={tableDef.columns}
                data={draftRow}
                mode="create"
                onChange={onChangeField}
                onBrowseFK={handleBrowseFK}
              />
            </div>
          </ScrollArea>

          {error && (
            <div className="px-6 py-3 border-t bg-destructive/5 shrink-0">
              <p className="text-xs text-destructive">{error}</p>
            </div>
          )}

          <SheetFooter className="flex-row items-center justify-between px-6 py-4 border-t shrink-0">
            <div className="flex items-center gap-2">
              <Switch
                id="create-multiple"
                size="sm"
                checked={createMultiple}
                onCheckedChange={setCreateMultiple}
              />
              <Label htmlFor="create-multiple" className="text-xs cursor-pointer">
                Create multiple
              </Label>
            </div>
            <div className="flex gap-2">
              <Button
                type="button"
                variant="outline"
                size="sm"
                onClick={() => handleOpenChange(false)}
                disabled={creating}
              >
                Cancel
              </Button>
              <Button
                type="button"
                size="sm"
                onClick={handleSubmit}
                disabled={creating}
              >
                {creating ? "Saving..." : "Save"}
              </Button>
            </div>
          </SheetFooter>
        </SheetContent>
      </Sheet>

      {fkRefTable && (
        <FKPickerDrawer
          open={fkPickerOpen}
          onOpenChange={setFkPickerOpen}
          tableDef={fkRefTable}
          onSelect={handleFKSelect}
        />
      )}
    </>
  );
}
