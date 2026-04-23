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
import { useState, useEffect } from "react";
import { RecordForm } from "./RecordForm";

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
      row[col.name] = Date.now();
    } else {
      row[col.name] = col.isNullable ? null : col.type === "boolean" ? false : col.type === "number" ? 0 : "";
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
  const [formData, setFormData] = useState<RecordRow>(() =>
    buildInitialRow(tableDef, mode, initialData)
  );

  useEffect(() => {
    if (open) {
      setFormData(buildInitialRow(tableDef, mode, initialData));
    }
  }, [open, tableDef, mode, initialData]);

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
