import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import type { ColumnDef, FieldValue, RecordRow, TableDef } from "@/types";

interface RecordFormFieldProps {
  column: ColumnDef;
  value: FieldValue;
  onChange: (value: FieldValue) => void;
  mode: "create" | "edit";
  allTables: TableDef[];
}

function getLabelColumn(table: TableDef): string {
  const stringCol = table.columns.find(
    (c) => c.type === "string" && !c.isPrimaryKey && !c.isForeignKey
  );
  return stringCol?.name ?? table.columns[0]?.name ?? "id";
}

function getRowLabel(row: RecordRow, labelCol: string): string {
  const val = row[labelCol];
  return val !== null && val !== undefined ? String(val) : "(empty)";
}

export function RecordFormField({ column, value, onChange, mode, allTables }: RecordFormFieldProps) {
  const isDisabled = column.isPrimaryKey && mode === "edit";

  const fkTable = column.isForeignKey && column.foreignKeyTable
    ? allTables.find((t) => t.key === column.foreignKeyTable)
    : null;

  return (
    <div className="flex flex-col gap-1.5">
      <Label className="text-xs font-medium text-muted-foreground">
        {column.name}
        <span className="ml-1 text-muted-foreground/60 font-normal">({column.type})</span>
        {column.isNullable && <span className="ml-1 text-muted-foreground/60">nullable</span>}
      </Label>

      {fkTable ? (
        <Select
          value={value !== null ? String(value) : ""}
          onValueChange={(v) => onChange(v)}
          disabled={isDisabled}
        >
          <SelectTrigger className="h-8 text-sm">
            <SelectValue placeholder="Select..." />
          </SelectTrigger>
          <SelectContent>
            {column.isNullable && (
              <SelectItem value="">null</SelectItem>
            )}
            {fkTable.rows.map((row) => {
              const pkCol = fkTable.columns.find((c) => c.isPrimaryKey);
              const pkVal = pkCol ? row[pkCol.name] : null;
              const labelCol = getLabelColumn(fkTable);
              return (
                <SelectItem key={String(pkVal)} value={String(pkVal)}>
                  <span className="font-mono text-xs mr-2 text-muted-foreground">
                    {String(pkVal).slice(0, 8)}…
                  </span>
                  {getRowLabel(row, labelCol)}
                </SelectItem>
              );
            })}
          </SelectContent>
        </Select>
      ) : column.type === "boolean" ? (
        <div className="flex items-center gap-2">
          <input
            type="checkbox"
            checked={value === true}
            onChange={(e) => onChange(e.target.checked)}
            disabled={isDisabled}
            className="h-4 w-4 rounded border-border"
          />
          <span className="text-sm text-muted-foreground">{value ? "true" : "false"}</span>
        </div>
      ) : column.type === "json" ? (
        <textarea
          value={value !== null ? String(value) : ""}
          onChange={(e) => onChange(e.target.value || null)}
          disabled={isDisabled}
          rows={3}
          className="w-full rounded-md border border-input bg-background px-3 py-2 text-xs font-mono resize-none focus:outline-none focus:ring-1 focus:ring-ring disabled:opacity-50"
          placeholder={column.isNullable ? "null" : "{}"}
        />
      ) : column.type === "datetime" ? (
        <Input
          type="datetime-local"
          value={value !== null ? String(value).replace(" ", "T").slice(0, 16) : ""}
          onChange={(e) => onChange(e.target.value || null)}
          disabled={isDisabled}
          className="h-8 text-sm"
        />
      ) : column.type === "number" ? (
        <Input
          type="number"
          value={value !== null ? String(value) : ""}
          onChange={(e) => onChange(e.target.value === "" ? null : Number(e.target.value))}
          disabled={isDisabled}
          className="h-8 text-sm"
        />
      ) : (
        <Input
          type="text"
          value={value !== null ? String(value) : ""}
          onChange={(e) => onChange(e.target.value || (column.isNullable ? null : e.target.value))}
          disabled={isDisabled}
          placeholder={isDisabled ? "(auto)" : column.isNullable ? "null" : ""}
          className="h-8 text-sm font-mono"
        />
      )}
    </div>
  );
}
