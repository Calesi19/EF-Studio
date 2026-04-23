import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";

interface DataTableToolbarProps {
  filter: string;
  onFilterChange: (value: string) => void;
  onAddRecord: () => void;
}

export function DataTableToolbar({ filter, onFilterChange, onAddRecord }: DataTableToolbarProps) {
  return (
    <div className="flex items-center justify-between gap-3 px-6 py-3 border-b border-border">
      <Input
        placeholder="Filter records..."
        value={filter}
        onChange={(e) => onFilterChange(e.target.value)}
        className="h-8 w-72 text-sm"
      />
      <Button size="sm" onClick={onAddRecord} className="h-8 gap-1.5">
        <span>+</span>
        Add record
      </Button>
    </div>
  );
}
