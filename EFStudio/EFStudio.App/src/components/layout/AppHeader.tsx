import { Badge } from "@/components/ui/badge";

interface AppHeaderProps {
  tableName: string;
  rowCount: number;
}

export function AppHeader({ tableName, rowCount }: AppHeaderProps) {
  return (
    <div className="flex items-center gap-2 border-b border-border px-4 py-2">
      <h1 className="text-sm font-semibold text-foreground">{tableName}</h1>
      <Badge variant="secondary" className="text-[11px] font-normal py-0">
        {rowCount.toLocaleString()} {rowCount === 1 ? "record" : "records"}
      </Badge>
    </div>
  );
}
