import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";

interface DataTableToolbarProps {
  filter: string;
  onFilterChange: (value: string) => void;
  onAddRecord: () => void;
}

export function DataTableToolbar({ filter, onFilterChange, onAddRecord }: DataTableToolbarProps) {
  return (
    <div className="flex items-center justify-between gap-3 px-4 py-2 border-b border-border">
      <Input
        placeholder="Filter records..."
        value={filter}
        onChange={(e) => onFilterChange(e.target.value)}
        className="h-7 w-56 text-xs"
      />
      <Button size="sm" onClick={onAddRecord} className="h-7 text-xs gap-1">
        <span>+</span>
        Add record
      </Button>
    </div>
  );
}
