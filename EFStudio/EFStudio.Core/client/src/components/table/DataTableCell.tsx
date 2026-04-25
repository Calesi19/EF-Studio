import { Badge } from "@/components/ui/badge";
import { Tooltip, TooltipContent, TooltipTrigger } from "@/components/ui/tooltip";
import type { ColumnDef, FieldValue } from "@/types";
import { useEffect, useRef, useState } from "react";
import { ArrowRight01Icon, Copy01Icon, Tick01Icon } from "@hugeicons/core-free-icons";
import { HugeiconsIcon } from "@hugeicons/react";

const COPY_FEEDBACK_DURATION_MS = 1500;

interface DataTableCellProps {
  value: FieldValue;
  column: ColumnDef;
  onJumpToRef?: (tableKey: string, value: FieldValue) => void;
  isEditing?: boolean;
  isPendingEdit?: boolean;
  isRequiredAndEmpty?: boolean;
  onDoubleClick?: () => void;
  onCommitEdit?: (value: FieldValue) => void;
  onCancelEdit?: () => void;
}

function formatDatetime(raw: string): string {
  const d = new Date(raw);
  if (isNaN(d.getTime())) return raw;
  const date = d.toISOString().slice(0, 10);
  const time = d.toISOString().slice(11, 16);
  return `${date} ${time}`;
}

function toDatetimeLocalValue(raw: string): string {
  const d = new Date(raw);
  if (isNaN(d.getTime())) return raw;
  return d.toISOString().slice(0, 16);
}

function TruncatedText({ text, className }: { text: string; className?: string }) {
  const [copied, setCopied] = useState(false);

  function handleCopy(e: React.MouseEvent) {
    e.stopPropagation();
    navigator.clipboard.writeText(text);
    setCopied(true);
    setTimeout(() => setCopied(false), COPY_FEEDBACK_DURATION_MS);
  }

  return (
    <Tooltip>
      <TooltipTrigger asChild>
        <span className={`truncate block w-full cursor-default ${className ?? ""}`}>{text}</span>
      </TooltipTrigger>
      <TooltipContent side="bottom" className="flex items-center gap-2 max-w-sm pr-1">
        <span className="break-all text-xs font-mono">{text}</span>
        <button
          onClick={handleCopy}
          className="shrink-0 flex items-center justify-center h-5 w-5 rounded hover:bg-white/15 transition-colors text-current"
        >
          <HugeiconsIcon icon={copied ? Tick01Icon : Copy01Icon} size={12} />
        </button>
      </TooltipContent>
    </Tooltip>
  );
}

function EditInput({
  value,
  column,
  onCommit,
  onCancel,
}: {
  value: FieldValue;
  column: ColumnDef;
  onCommit: (value: FieldValue) => void;
  onCancel: () => void;
}) {
  const inputRef = useRef<HTMLInputElement & HTMLTextAreaElement & HTMLSelectElement>(null);

  const initialString =
    value === null || value === undefined
      ? ""
      : column.type === "datetime"
        ? toDatetimeLocalValue(String(value))
        : String(value);

  const [draft, setDraft] = useState(initialString);

  useEffect(() => {
    inputRef.current?.focus();
    if (inputRef.current && "select" in inputRef.current) {
      inputRef.current.select();
    }
  }, []);

  function commit() {
    if (column.type === "boolean") {
      onCommit(draft === "true" ? true : draft === "false" ? false : null);
    } else if (column.type === "number") {
      const n = Number(draft);
      onCommit(draft === "" ? null : isNaN(n) ? draft : n);
    } else {
      onCommit(draft === "" ? null : draft);
    }
  }

  function handleKeyDown(e: React.KeyboardEvent) {
    if (e.key === "Enter" && column.type !== "json") {
      e.preventDefault();
      commit();
    } else if (e.key === "Escape") {
      e.preventDefault();
      onCancel();
    }
  }

  const baseClass =
    "h-full w-full bg-background border border-primary/60 rounded-sm px-1.5 text-xs focus:outline-none focus:border-primary focus:ring-1 focus:ring-primary/30";

  if (column.type === "boolean") {
    return (
      <select
        ref={inputRef as React.RefObject<HTMLSelectElement>}
        value={draft}
        onChange={(e) => setDraft(e.target.value)}
        onBlur={commit}
        onKeyDown={handleKeyDown}
        className={baseClass}
      >
        <option value="true">true</option>
        <option value="false">false</option>
      </select>
    );
  }

  if (column.type === "json") {
    return (
      <textarea
        ref={inputRef as React.RefObject<HTMLTextAreaElement>}
        value={draft}
        onChange={(e) => setDraft(e.target.value)}
        onBlur={commit}
        onKeyDown={handleKeyDown}
        rows={3}
        className={`${baseClass} font-mono resize-none min-h-[2rem]`}
      />
    );
  }

  return (
    <input
      ref={inputRef as React.RefObject<HTMLInputElement>}
      type={
        column.type === "number" ? "number" : column.type === "datetime" ? "datetime-local" : "text"
      }
      value={draft}
      onChange={(e) => setDraft(e.target.value)}
      onBlur={commit}
      onKeyDown={handleKeyDown}
      className={`${baseClass} font-mono`}
    />
  );
}

export function DataTableCell({
  value,
  column,
  onJumpToRef,
  isEditing = false,
  isPendingEdit = false,
  isRequiredAndEmpty = false,
  onDoubleClick,
  onCommitEdit,
  onCancelEdit,
}: DataTableCellProps) {
  const canEdit = !!onDoubleClick && !column.isPrimaryKey;

  const pendingClass = isPendingEdit
    ? "bg-amber-500/5 border-l-2 border-l-amber-400/60"
    : "";

  const requiredEmptyClass = isRequiredAndEmpty
    ? "ring-1 ring-inset ring-amber-400/60"
    : "";

  if (isEditing && onCommitEdit && onCancelEdit) {
    return (
      <div className={`flex items-center h-full w-full ${pendingClass}`}>
        <EditInput
          value={value}
          column={column}
          onCommit={onCommitEdit}
          onCancel={onCancelEdit}
        />
      </div>
    );
  }

  const displayContent = (() => {
    if (value === null || value === undefined) {
      return <span className="text-muted-foreground italic text-xs">null</span>;
    }

    if (column.type === "boolean") {
      return value ? (
        <Badge className="bg-emerald-500/15 text-emerald-700 dark:text-emerald-400 border-emerald-500/30 font-mono text-xs shrink-0" variant="outline">
          true
        </Badge>
      ) : (
        <Badge className="text-muted-foreground font-mono text-xs shrink-0" variant="outline">
          false
        </Badge>
      );
    }

    if (column.isForeignKey && column.foreignKeyTable && onJumpToRef) {
      return (
        <div className="flex items-center gap-1 min-w-0">
          <TruncatedText
            text={String(value)}
            className="font-mono text-xs text-blue-600 dark:text-blue-400 min-w-0"
          />
          <button
            onClick={(e) => { e.stopPropagation(); onJumpToRef(column.foreignKeyTable!, value); }}
            className="opacity-0 group-hover:opacity-100 shrink-0 flex h-4 w-4 items-center justify-center rounded text-blue-500 hover:bg-blue-500/10 transition-all"
            title={`Go to ${column.foreignKeyTable}`}
          >
            <HugeiconsIcon icon={ArrowRight01Icon} size={10} />
          </button>
        </div>
      );
    }

    if (column.isPrimaryKey) {
      return <TruncatedText text={String(value)} className="font-mono text-xs text-primary" />;
    }

    if (column.type === "datetime") {
      const formatted = formatDatetime(String(value));
      return <TruncatedText text={formatted} className="text-xs tabular-nums text-muted-foreground" />;
    }

    if (column.type === "number") {
      return <span className="text-xs tabular-nums font-mono">{String(value)}</span>;
    }

    return <TruncatedText text={String(value)} className="text-xs" />;
  })();

  return (
    <div
      className={`flex items-center h-full w-full ${pendingClass} ${requiredEmptyClass} ${canEdit ? "cursor-default" : ""}`}
      onDoubleClick={canEdit ? onDoubleClick : undefined}
    >
      {displayContent}
    </div>
  );
}
