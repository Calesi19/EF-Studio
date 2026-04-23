import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { TableCell, TableRow } from "@/components/ui/table";
import type { ColumnDef, RecordRow } from "@/types";
import { DataTableCell } from "./DataTableCell";

interface DataTableRowProps {
  row: RecordRow;
  columns: ColumnDef[];
  onEdit: (row: RecordRow) => void;
  onDelete: (row: RecordRow) => void;
}

export function DataTableRow({ row, columns, onEdit, onDelete }: DataTableRowProps) {
  return (
    <TableRow className="hover:bg-muted/30 group">
      {columns.map((col) => (
        <TableCell key={col.name} className="px-3 py-1 align-middle">
          <DataTableCell value={row[col.name] ?? null} column={col} />
        </TableCell>
      ))}
      <TableCell className="px-1 py-1 align-middle w-8">
        <DropdownMenu>
          <DropdownMenuTrigger asChild>
            <button className="opacity-0 group-hover:opacity-100 flex h-5 w-5 items-center justify-center rounded text-muted-foreground hover:text-foreground hover:bg-muted transition-all text-xs">
              ⋯
            </button>
          </DropdownMenuTrigger>
          <DropdownMenuContent align="end">
            <DropdownMenuItem onClick={() => onEdit(row)}>
              Edit
            </DropdownMenuItem>
            <DropdownMenuSeparator />
            <DropdownMenuItem
              onClick={() => onDelete(row)}
              className="text-destructive focus:text-destructive"
            >
              Delete
            </DropdownMenuItem>
          </DropdownMenuContent>
        </DropdownMenu>
      </TableCell>
    </TableRow>
  );
}
