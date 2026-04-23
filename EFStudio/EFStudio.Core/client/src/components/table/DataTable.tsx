import { ScrollArea, ScrollBar } from "@/components/ui/scroll-area";
import { Table, TableBody } from "@/components/ui/table";
import { useTableState } from "@/hooks/useTableState";
import type { ColumnDef, PaginationState, RecordRow, SortState } from "@/types";
import { DataTableHeader } from "./DataTableHeader";
import { DataTablePagination } from "./DataTablePagination";
import { DataTableRow } from "./DataTableRow";
import { DataTableToolbar } from "./DataTableToolbar";

interface DataTableProps {
  columns: ColumnDef[];
  rows: RecordRow[];
  filter: string;
  sort: SortState;
  pagination: PaginationState;
  onFilterChange: (value: string) => void;
  onSortChange: (column: string) => void;
  onPageChange: (page: number) => void;
  onPageSizeChange: (size: number) => void;
  onAddRecord: () => void;
  onEditRecord: (row: RecordRow) => void;
  onDeleteRecord: (row: RecordRow) => void;
}

export function DataTable({
  columns,
  rows,
  filter,
  sort,
  pagination,
  onFilterChange,
  onSortChange,
  onPageChange,
  onPageSizeChange,
  onAddRecord,
  onEditRecord,
  onDeleteRecord,
}: DataTableProps) {
  const { paginatedRows, totalRows, totalPages } = useTableState(
    rows,
    columns,
    filter,
    sort,
    pagination
  );

  return (
    <div className="flex flex-1 flex-col overflow-hidden">
      <DataTableToolbar
        filter={filter}
        onFilterChange={(v) => {
          onFilterChange(v);
          onPageChange(1);
        }}
        onAddRecord={onAddRecord}
      />
      <ScrollArea className="flex-1">
        <Table>
          <DataTableHeader
            columns={columns}
            sort={sort}
            onSortChange={onSortChange}
          />
          <TableBody>
            {paginatedRows.length === 0 ? (
              <tr>
                <td
                  colSpan={columns.length + 1}
                  className="py-16 text-center text-sm text-muted-foreground"
                >
                  {filter ? "No records match your filter." : "No records found."}
                </td>
              </tr>
            ) : (
              paginatedRows.map((row, i) => (
                <DataTableRow
                  key={i}
                  row={row}
                  columns={columns}
                  onEdit={onEditRecord}
                  onDelete={onDeleteRecord}
                />
              ))
            )}
          </TableBody>
        </Table>
        <ScrollBar orientation="horizontal" />
      </ScrollArea>
      <DataTablePagination
        pagination={pagination}
        totalRows={totalRows}
        totalPages={totalPages}
        onPageChange={onPageChange}
        onPageSizeChange={onPageSizeChange}
      />
    </div>
  );
}
