import { useState } from "react";
import { Button } from "@/components/ui/button";
import {
  Sheet,
  SheetContent,
  SheetHeader,
  SheetTitle,
} from "@/components/ui/sheet";
import { DataTable } from "@/components/table/DataTable";
import { useTableData } from "@/api/data/fetchTableData";
import { useStudioContext } from "@/pages/StudioPage/context/StudioContext";
import type { FieldValue, RecordRow, SortState, TableDef } from "@/types";

const FK_PICKER_PAGE_SIZE = 25;
const DEFAULT_SORT: SortState = { column: null, direction: "asc" };
const DEFAULT_PAGE = 1;

interface FKPickerDrawerProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  tableDef: TableDef;
  onSelect: (value: FieldValue) => void;
}

export function FKPickerDrawer({ open, onOpenChange, tableDef, onSelect }: FKPickerDrawerProps) {
  const { selectedContextName } = useStudioContext();
  const [filter, setFilter] = useState("");
  const [sort, setSort] = useState<SortState>(DEFAULT_SORT);
  const [page, setPage] = useState(DEFAULT_PAGE);

  const pagination = { page, pageSize: FK_PICKER_PAGE_SIZE };
  const { data, isLoading } = useTableData(
    selectedContextName,
    tableDef.key,
    pagination,
    filter,
    sort,
    open,
  );

  const rows = data?.rows ?? [];
  const totalRows = data?.totalRows ?? 0;
  const totalPages = data?.totalPages ?? 1;

  function handleSortChange(column: string) {
    setSort((prev): SortState => {
      if (prev.column === column) {
        return prev.direction === "asc" ? { column, direction: "desc" } : DEFAULT_SORT;
      }
      return { column, direction: "asc" };
    });
  }

  function handleFilterChange(value: string) {
    setFilter(value);
    setPage(DEFAULT_PAGE);
  }

  function handleRowClick(row: RecordRow) {
    const pkCol = tableDef.columns.find((c) => c.isPrimaryKey);
    const pkValue = pkCol ? (row[pkCol.name] ?? null) : null;
    onSelect(pkValue);
    onOpenChange(false);
  }

  function handleOpenChange(nextOpen: boolean) {
    if (!nextOpen) {
      setFilter("");
      setSort(DEFAULT_SORT);
      setPage(DEFAULT_PAGE);
    }
    onOpenChange(nextOpen);
  }

  return (
    <Sheet open={open} onOpenChange={handleOpenChange}>
      <SheetContent
        side="right"
        className="sm:max-w-2xl flex flex-col p-0 gap-0"
        showCloseButton={false}
      >
        <SheetHeader className="px-6 pt-5 pb-4 border-b shrink-0">
          <div className="flex items-center justify-between gap-4">
            <SheetTitle className="text-base">
              Select from {tableDef.displayName ?? tableDef.name}
            </SheetTitle>
            <Button
              type="button"
              variant="ghost"
              size="sm"
              onClick={() => handleOpenChange(false)}
            >
              Close
            </Button>
          </div>
        </SheetHeader>

        <div className="flex-1 overflow-hidden">
          {isLoading && rows.length === 0 ? (
            <div className="flex items-center justify-center h-full text-xs text-muted-foreground">
              Loading…
            </div>
          ) : (
            <DataTable
              columns={tableDef.columns}
              rows={rows}
              totalRows={totalRows}
              totalPages={totalPages}
              filter={filter}
              sort={sort}
              pagination={pagination}
              onFilterChange={handleFilterChange}
              onSortChange={handleSortChange}
              onPageChange={setPage}
              onPageSizeChange={() => {}}
              onAddRecord={() => {}}
              canAddRecord={false}
              onEditRecord={() => {}}
              onDeleteRecord={() => {}}
              onBulkDelete={() => {}}
              onJumpToRef={() => {}}
              readOnly
              hideCheckboxColumn
              onRowClick={handleRowClick}
            />
          )}
        </div>
      </SheetContent>
    </Sheet>
  );
}
