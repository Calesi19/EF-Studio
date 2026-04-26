import type { ColumnDef, FieldValue, RecordRow, TableDef } from "@/types";
import { RecordFormField } from "./RecordFormField";

interface RecordFormProps {
  columns: ColumnDef[];
  data: RecordRow;
  mode: "create" | "edit";
  allTables: TableDef[];
  onChange: (field: string, value: FieldValue) => void;
}

export function RecordForm({ columns, data, mode, allTables, onChange }: RecordFormProps) {
  const visibleColumns = mode === "create"
    ? columns.filter((column) => column.isEditableOnCreate)
    : columns;

  return (
    <div className="flex flex-col gap-4">
      {visibleColumns.map((col) => (
        <RecordFormField
          key={col.name}
          column={col}
          value={data[col.name] ?? null}
          onChange={(value) => onChange(col.name, value)}
          mode={mode}
          allTables={allTables}
        />
      ))}
    </div>
  );
}
