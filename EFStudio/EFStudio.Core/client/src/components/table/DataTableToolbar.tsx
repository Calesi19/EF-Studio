import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Cancel01Icon } from "@hugeicons/core-free-icons";
import { HugeiconsIcon } from "@hugeicons/react";

interface DataTableToolbarProps {
  filter: string;
  onFilterChange: (value: string) => void;
  onAddRecord: () => void;
  selectedCount: number;
  onBulkDelete: () => void;
  readOnly?: boolean;
}

export function DataTableToolbar({
  filter,
  onFilterChange,
  onAddRecord,
  selectedCount,
  onBulkDelete,
  readOnly = false,
}: DataTableToolbarProps) {
  return (
    <div className="flex items-center justify-between gap-3 px-4 py-2 border-b border-border">
      <div className="flex items-center gap-2">
        {!readOnly && selectedCount > 0 ? (
          <>
            <span className="text-xs text-muted-foreground">
              {selectedCount} {selectedCount === 1 ? "row" : "rows"} selected
            </span>
            <Button
              size="sm"
              variant="destructive"
              onClick={onBulkDelete}
              className="h-7 text-xs"
            >
              Delete {selectedCount}
            </Button>
          </>
        ) : (
          <div className="relative w-56">
            <Input
              placeholder="Filter records..."
              value={filter}
              onChange={(e) => onFilterChange(e.target.value)}
              className="h-7 w-full pr-8 text-xs"
            />
            {filter && (
              <button
                type="button"
                onClick={() => onFilterChange("")}
                className="absolute right-1 top-1/2 flex h-5 w-5 -translate-y-1/2 items-center justify-center rounded-full text-muted-foreground transition-colors hover:bg-muted hover:text-foreground"
                aria-label="Clear filter"
                title="Clear filter"
              >
                <HugeiconsIcon icon={Cancel01Icon} size={12} />
              </button>
            )}
          </div>
        )}
      </div>
      {!readOnly && (
        <Button size="sm" onClick={onAddRecord} className="h-7 text-xs gap-1">
          <span>+</span>
          Add record
        </Button>
      )}
    </div>
  );
}
