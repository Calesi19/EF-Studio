import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";

interface DataTableToolbarProps {
  filter: string;
  onFilterChange: (value: string) => void;
  onAddRecord: () => void;
  selectedCount: number;
  onBulkDelete: () => void;
}

export function DataTableToolbar({ filter, onFilterChange, onAddRecord, selectedCount, onBulkDelete }: DataTableToolbarProps) {
  return (
    <div className="flex items-center justify-between gap-3 px-4 py-2 border-b border-border">
      <div className="flex items-center gap-2">
        {selectedCount > 0 ? (
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
          <Input
            placeholder="Filter records..."
            value={filter}
            onChange={(e) => onFilterChange(e.target.value)}
            className="h-7 w-56 text-xs"
          />
        )}
      </div>
      <Button size="sm" onClick={onAddRecord} className="h-7 text-xs gap-1">
        <span>+</span>
        Add record
      </Button>
    </div>
  );
}
