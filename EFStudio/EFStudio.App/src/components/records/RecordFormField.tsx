import { useState } from "react";
import { format, isValid, parseISO } from "date-fns";
import { Button } from "@/components/ui/button";
import { Calendar } from "@/components/ui/calendar";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Popover, PopoverContent, PopoverTrigger } from "@/components/ui/popover";
import { ColumnTypeBadge } from "@/components/table/ColumnTypeBadge";
import type { ColumnDef, FieldValue } from "@/types";

interface RecordFormFieldProps {
  column: ColumnDef;
  value: FieldValue;
  onChange: (value: FieldValue) => void;
  mode: "create" | "edit";
  onBrowseFK?: (columnName: string, refTableKey: string) => void;
}

function parseFieldDate(val: FieldValue): Date | undefined {
  if (!val) return undefined;
  const str = String(val).replace(" ", "T");
  const d = parseISO(str);
  return isValid(d) ? d : undefined;
}

export function RecordFormField({ column, value, onChange, mode, onBrowseFK }: RecordFormFieldProps) {
  const [calOpen, setCalOpen] = useState(false);

  const isDisabled =
    (column.isPrimaryKey && mode === "edit") ||
    (mode === "create" && !column.isEditableOnCreate);

  const isRequired = !column.isNullable && !column.isGeneratedOnAdd;

  return (
    <div className="flex flex-col gap-1.5">
      <div className="flex items-center gap-1.5 flex-wrap">
        <Label className="text-xs font-medium">{column.name}</Label>
        <ColumnTypeBadge column={column} />
        {isRequired && !column.isPrimaryKey && !column.isForeignKey && (
          <span className="text-[10px] text-destructive font-medium">required</span>
        )}
      </div>

      {column.isForeignKey && column.foreignKeyTable ? (
        <div className="flex gap-2">
          <Input
            readOnly
            value={value !== null && value !== "" ? String(value) : ""}
            placeholder="— none —"
            className="h-8 text-sm font-mono flex-1 cursor-default"
          />
          {!isDisabled && onBrowseFK && (
            <Button
              type="button"
              variant="outline"
              size="sm"
              className="shrink-0"
              onClick={() => onBrowseFK(column.name, column.foreignKeyTable!)}
            >
              Browse
            </Button>
          )}
          {column.isNullable && (value !== null && value !== "") && (
            <Button
              type="button"
              variant="ghost"
              size="sm"
              className="shrink-0"
              onClick={() => onChange(null)}
            >
              Clear
            </Button>
          )}
        </div>
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
        <DatetimePicker
          value={value}
          onChange={onChange}
          isDisabled={isDisabled}
          isNullable={column.isNullable}
          calOpen={calOpen}
          setCalOpen={setCalOpen}
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

interface DatetimePickerProps {
  value: FieldValue;
  onChange: (value: FieldValue) => void;
  isDisabled: boolean;
  isNullable: boolean;
  calOpen: boolean;
  setCalOpen: (open: boolean) => void;
}

function DatetimePicker({ value, onChange, isDisabled, isNullable, calOpen, setCalOpen }: DatetimePickerProps) {
  const dateValue = parseFieldDate(value);

  function handleDaySelect(date: Date | undefined) {
    if (!date) {
      onChange(isNullable ? null : "");
      setCalOpen(false);
      return;
    }
    const existing = dateValue ?? new Date();
    date.setHours(existing.getHours(), existing.getMinutes(), 0, 0);
    onChange(date.toISOString());
  }

  function handleTimeChange(e: React.ChangeEvent<HTMLInputElement>) {
    const parts = e.target.value.split(":").map(Number);
    const h = parts[0] ?? 0;
    const m = parts[1] ?? 0;
    const next = dateValue ? new Date(dateValue) : new Date();
    next.setHours(h, m, 0, 0);
    onChange(next.toISOString());
  }

  return (
    <Popover open={calOpen} onOpenChange={setCalOpen}>
      <PopoverTrigger asChild>
        <Button
          type="button"
          variant="outline"
          disabled={isDisabled}
          className="h-8 w-full justify-start text-left font-normal text-sm"
        >
          {dateValue ? (
            format(dateValue, "yyyy-MM-dd HH:mm")
          ) : (
            <span className="text-muted-foreground">
              {isDisabled ? "(auto)" : "Pick a date..."}
            </span>
          )}
        </Button>
      </PopoverTrigger>
      <PopoverContent className="w-auto p-0" align="start">
        <Calendar
          mode="single"
          selected={dateValue}
          onSelect={handleDaySelect}
        />
        <div className="border-t p-3">
          <input
            type="time"
            value={dateValue ? format(dateValue, "HH:mm") : ""}
            onChange={handleTimeChange}
            className="w-full h-8 rounded-md border border-input px-3 text-sm bg-background"
          />
        </div>
      </PopoverContent>
    </Popover>
  );
}
