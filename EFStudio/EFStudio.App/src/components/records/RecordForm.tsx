import type { ColumnDef, FieldValue, RecordRow } from "@/types";
import { RecordFormField } from "./RecordFormField";

interface RecordFormProps {
  columns: ColumnDef[];
  data: RecordRow;
  mode: "create" | "edit";
  onChange: (field: string, value: FieldValue) => void;
  onBrowseFK?: (columnName: string, refTableKey: string) => void;
}

export function RecordForm({ columns, data, mode, onChange, onBrowseFK }: RecordFormProps) {
  const visibleColumns = mode === "create"
    ? columns.filter((column) => column.isEditableOnCreate)
    : columns;

  return (
    <div className="flex flex-col gap-5">
      {visibleColumns.map((col) => (
        <RecordFormField
          key={col.name}
          column={col}
          value={data[col.name] ?? null}
          onChange={(value) => onChange(col.name, value)}
          mode={mode}
          onBrowseFK={onBrowseFK}
        />
      ))}
    </div>
  );
}
