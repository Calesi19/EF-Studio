import { Button } from "@/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { ScrollArea } from "@/components/ui/scroll-area";
import type { ColumnDef, FieldValue, RecordRow, TableDef } from "@/types";
import { RecordForm } from "./RecordForm";

const DEFAULT_NUMBER_VALUE = 0;

interface CreateRecordsDialogProps {
  open: boolean;
  tableDef: TableDef;
  draftRows: RecordRow[];
  creatingRows: boolean;
  onOpenChange: (open: boolean) => void;
  onAddRow: () => void;
  onRemoveRow: (index: number) => void;
  onChangeRow: (index: number, field: string, value: FieldValue) => void;
  onSubmit: () => void;
}

function createInitialValue(column: ColumnDef): FieldValue {
  if (column.isPrimaryKey && column.type === "uuid") {
    return crypto.randomUUID();
  }

  if (column.type === "boolean") {
    return false;
  }

  if (column.type === "number") {
    return DEFAULT_NUMBER_VALUE;
  }

  return column.isNullable ? null : "";
}

export function buildCreateDraftRow(tableDef: TableDef): RecordRow {
  return Object.fromEntries(
    tableDef.columns
      .filter((column) => column.isEditableOnCreate)
      .map((column) => [column.name, createInitialValue(column)]),
  );
}

export function CreateRecordsDialog({
  open,
  tableDef,
  draftRows,
  creatingRows,
  onOpenChange,
  onAddRow,
  onRemoveRow,
  onChangeRow,
  onSubmit,
}: CreateRecordsDialogProps) {
  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-w-4xl">
        <DialogHeader>
          <DialogTitle>Create {draftRows.length} {draftRows.length === 1 ? "record" : "records"} in {tableDef.name}</DialogTitle>
        </DialogHeader>
        <ScrollArea className="max-h-[70vh] pr-4">
          <div className="flex flex-col gap-4">
            {draftRows.map((row, index) => (
              <section key={index} className="rounded-lg border border-border/70 bg-muted/10 p-4">
                <div className="mb-4 flex items-center justify-between gap-3">
                  <div className="text-sm font-medium">Row {index + 1}</div>
                  <Button
                    type="button"
                    size="sm"
                    variant="outline"
                    onClick={() => onRemoveRow(index)}
                    disabled={draftRows.length === 1 || creatingRows}
                  >
                    Remove
                  </Button>
                </div>
                <RecordForm
                  columns={tableDef.columns}
                  data={row}
                  mode="create"
                  onChange={(field, value) => onChangeRow(index, field, value)}
                />
              </section>
            ))}
          </div>
        </ScrollArea>
        <DialogFooter className="justify-between sm:justify-between">
          <Button type="button" variant="outline" onClick={onAddRow} disabled={creatingRows}>
            Add another row
          </Button>
          <div className="flex gap-2">
            <Button type="button" variant="outline" onClick={() => onOpenChange(false)} disabled={creatingRows}>
              Cancel
            </Button>
            <Button type="button" onClick={onSubmit} disabled={creatingRows}>
              {creatingRows ? "Creating..." : `Create ${draftRows.length}`}
            </Button>
          </div>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
