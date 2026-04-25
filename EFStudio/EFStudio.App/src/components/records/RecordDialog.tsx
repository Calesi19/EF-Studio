import { Button } from "@/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { ScrollArea } from "@/components/ui/scroll-area";
import type { FieldValue, RecordRow, TableDef } from "@/types";
import { useState } from "react";
import { RecordForm } from "./RecordForm";

const DEFAULT_NUMBER_VALUE = 0;

interface RecordDialogProps {
  mode: "create" | "edit";
  open: boolean;
  onOpenChange: (open: boolean) => void;
  tableDef: TableDef;
  initialData?: RecordRow;
  allTables: TableDef[];
  onSubmit: (row: RecordRow) => void;
}

function buildInitialRow(tableDef: TableDef, mode: "create" | "edit", initialData?: RecordRow): RecordRow {
  const row: RecordRow = {};
  for (const col of tableDef.columns) {
    if (mode === "edit" && initialData) {
      row[col.name] = initialData[col.name] ?? null;
    } else if (col.isPrimaryKey && col.type === "uuid") {
      row[col.name] = crypto.randomUUID();
    } else if (col.isPrimaryKey && col.type === "number") {
      row[col.name] = DEFAULT_NUMBER_VALUE;
    } else {
      row[col.name] =
        col.isNullable ? null : col.type === "boolean" ? false : col.type === "number" ? DEFAULT_NUMBER_VALUE : "";
    }
  }
  return row;
}

export function RecordDialog({
  mode,
  open,
  onOpenChange,
  tableDef,
  initialData,
  allTables,
  onSubmit,
}: RecordDialogProps) {
  if (!open) {
    return null;
  }

  return (
    <RecordDialogContent
      key={`${mode}-${tableDef.name}-${JSON.stringify(initialData ?? {})}`}
      mode={mode}
      open={open}
      onOpenChange={onOpenChange}
      tableDef={tableDef}
      initialData={initialData}
      allTables={allTables}
      onSubmit={onSubmit}
    />
  );
}

function RecordDialogContent({
  mode,
  open,
  onOpenChange,
  tableDef,
  initialData,
  allTables,
  onSubmit,
}: RecordDialogProps) {
  const [formData, setFormData] = useState<RecordRow>(() =>
    buildInitialRow(tableDef, mode, initialData)
  );

  function handleChange(field: string, value: FieldValue) {
    setFormData((prev) => ({ ...prev, [field]: value }));
  }

  function handleSubmit() {
    onSubmit(formData);
    onOpenChange(false);
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-w-lg">
        <DialogHeader>
          <DialogTitle>
            {mode === "create" ? `New ${tableDef.name}` : `Edit ${tableDef.name}`}
          </DialogTitle>
        </DialogHeader>
        <ScrollArea className="max-h-[60vh] pr-4">
          <RecordForm
            columns={tableDef.columns}
            data={formData}
            mode={mode}
            allTables={allTables}
            onChange={handleChange}
          />
        </ScrollArea>
        <DialogFooter>
          <Button variant="outline" onClick={() => onOpenChange(false)}>
            Cancel
          </Button>
          <Button onClick={handleSubmit}>
            {mode === "create" ? "Create" : "Save changes"}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
