import { Badge } from "@/components/ui/badge";

interface AppHeaderProps {
  tableName: string;
  rowCount: number;
}

export function AppHeader({ tableName, rowCount }: AppHeaderProps) {
  return (
    <div className="flex items-center gap-3 border-b border-border px-6 py-4">
      <h1 className="text-xl font-semibold text-foreground">{tableName}</h1>
      <Badge variant="secondary" className="text-xs font-normal">
        {rowCount.toLocaleString()} {rowCount === 1 ? "record" : "records"}
      </Badge>
    </div>
  );
}
