import { useState, useCallback } from "react";
import { TooltipProvider } from "@/components/ui/tooltip";
import { AppShell } from "@/components/layout/AppShell";
import { AppHeader } from "@/components/layout/AppHeader";
import { Sidebar } from "@/components/sidebar/Sidebar";
import { DataTable } from "@/components/table/DataTable";
import { RecordDialog } from "@/components/records/RecordDialog";
import { DeleteConfirmDialog } from "@/components/records/DeleteConfirmDialog";
import { MOCK_TABLES } from "@/data/mock";
import type { PaginationState, RecordRow, SortState } from "@/types";

const DEFAULT_SORT: SortState = { column: null, direction: "asc" };
const DEFAULT_PAGINATION: PaginationState = { page: 1, pageSize: 10 };

function initRecords() {
  const map = new Map<string, RecordRow[]>();
  for (const table of MOCK_TABLES) {
    map.set(table.name, [...table.rows]);
  }
  return map;
}

export default function App() {
  const tables = MOCK_TABLES;
  const [records, setRecords] = useState<Map<string, RecordRow[]>>(initRecords);
  const [selectedTableName, setSelectedTableName] = useState<string | null>(tables[0]?.name ?? null);
  const [filter, setFilter] = useState("");
  const [sort, setSort] = useState<SortState>(DEFAULT_SORT);
  const [pagination, setPagination] = useState<PaginationState>(DEFAULT_PAGINATION);

  const [createOpen, setCreateOpen] = useState(false);
  const [editOpen, setEditOpen] = useState(false);
  const [editingRow, setEditingRow] = useState<RecordRow | null>(null);
  const [deleteOpen, setDeleteOpen] = useState(false);
  const [deletingRow, setDeletingRow] = useState<RecordRow | null>(null);

  const selectedTable = tables.find((t) => t.name === selectedTableName) ?? null;
  const currentRows = selectedTableName ? (records.get(selectedTableName) ?? []) : [];

  function handleSelectTable(name: string) {
    setSelectedTableName(name);
    setFilter("");
    setSort(DEFAULT_SORT);
    setPagination(DEFAULT_PAGINATION);
  }

  function handleSortChange(column: string) {
    setSort((prev) => {
      if (prev.column === column) {
        if (prev.direction === "asc") return { column, direction: "desc" };
        return { column: null, direction: "asc" };
      }
      return { column, direction: "asc" };
    });
    setPagination((p) => ({ ...p, page: 1 }));
  }

  const handleCreateRecord = useCallback((row: RecordRow) => {
    if (!selectedTableName) return;
    setRecords((prev) => {
      const next = new Map(prev);
      next.set(selectedTableName, [...(prev.get(selectedTableName) ?? []), row]);
      return next;
    });
  }, [selectedTableName]);

  const handleUpdateRecord = useCallback((row: RecordRow) => {
    if (!selectedTable) return;
    const pkCol = selectedTable.columns.find((c) => c.isPrimaryKey);
    setRecords((prev) => {
      const next = new Map(prev);
      const rows = prev.get(selectedTable.name) ?? [];
      if (pkCol) {
        next.set(
          selectedTable.name,
          rows.map((r) => (r[pkCol.name] === row[pkCol.name] ? row : r))
        );
      }
      return next;
    });
  }, [selectedTable]);

  const handleDeleteRecord = useCallback(() => {
    if (!selectedTable || !deletingRow) return;
    const pkCol = selectedTable.columns.find((c) => c.isPrimaryKey);
    setRecords((prev) => {
      const next = new Map(prev);
      const rows = prev.get(selectedTable.name) ?? [];
      if (pkCol) {
        next.set(
          selectedTable.name,
          rows.filter((r) => r[pkCol.name] !== deletingRow[pkCol.name])
        );
      } else {
        next.set(selectedTable.name, rows.filter((r) => r !== deletingRow));
      }
      return next;
    });
    setDeletingRow(null);
  }, [selectedTable, deletingRow]);

  const recordCounts = new Map(
    tables.map((t) => [t.name, records.get(t.name)?.length ?? 0])
  );

  return (
    <TooltipProvider>
      <AppShell
        sidebar={
          <Sidebar
            tables={tables}
            recordCounts={recordCounts}
            selectedTableName={selectedTableName}
            onSelectTable={handleSelectTable}
          />
        }
      >
        {selectedTable ? (
          <>
            <AppHeader tableName={selectedTable.displayName} rowCount={currentRows.length} />
            <DataTable
              columns={selectedTable.columns}
              rows={currentRows}
              filter={filter}
              sort={sort}
              pagination={pagination}
              onFilterChange={setFilter}
              onSortChange={handleSortChange}
              onPageChange={(page) => setPagination((p) => ({ ...p, page }))}
              onPageSizeChange={(pageSize) => setPagination({ page: 1, pageSize })}
              onAddRecord={() => setCreateOpen(true)}
              onEditRecord={(row) => { setEditingRow(row); setEditOpen(true); }}
              onDeleteRecord={(row) => { setDeletingRow(row); setDeleteOpen(true); }}
            />
            <RecordDialog
              mode="create"
              open={createOpen}
              onOpenChange={setCreateOpen}
              tableDef={selectedTable}
              allTables={tables}
              onSubmit={handleCreateRecord}
            />
            {editingRow && (
              <RecordDialog
                mode="edit"
                open={editOpen}
                onOpenChange={setEditOpen}
                tableDef={selectedTable}
                initialData={editingRow}
                allTables={tables}
                onSubmit={handleUpdateRecord}
              />
            )}
            <DeleteConfirmDialog
              open={deleteOpen}
              onOpenChange={setDeleteOpen}
              onConfirm={() => { handleDeleteRecord(); setDeleteOpen(false); }}
            />
          </>
        ) : (
          <div className="flex flex-1 items-center justify-center text-muted-foreground text-sm">
            Select a table from the sidebar.
          </div>
        )}
      </AppShell>
    </TooltipProvider>
  );
}
