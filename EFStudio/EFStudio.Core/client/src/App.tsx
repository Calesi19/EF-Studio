import { useState } from "react";
import { TooltipProvider } from "@/components/ui/tooltip";
import { AppShell } from "@/components/layout/AppShell";
import { TabBar } from "@/components/layout/TabBar";
import { Sidebar } from "@/components/sidebar/Sidebar";
import { DataTable } from "@/components/table/DataTable";
import { RecordDialog } from "@/components/records/RecordDialog";
import { DeleteConfirmDialog } from "@/components/records/DeleteConfirmDialog";
import { MOCK_TABLES } from "@/data/mock";
import type { FieldValue, PaginationState, RecordRow, SortState, TabState } from "@/types";

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
  const [tabs, setTabs] = useState<TabState[]>([]);
  const [activeTabId, setActiveTabId] = useState<string | null>(null);
  const [sidebarOpen, setSidebarOpen] = useState(true);

  const [createOpen, setCreateOpen] = useState(false);
  const [editOpen, setEditOpen] = useState(false);
  const [editingRow, setEditingRow] = useState<RecordRow | null>(null);
  const [deleteOpen, setDeleteOpen] = useState(false);
  const [deletingRow, setDeletingRow] = useState<RecordRow | null>(null);

  const activeTab = tabs.find((t) => t.id === activeTabId) ?? null;
  const selectedTable = activeTab ? (tables.find((t) => t.name === activeTab.tableName) ?? null) : null;
  const currentRows = selectedTable ? (records.get(selectedTable.name) ?? []) : [];

  function updateActiveTab(updates: Partial<Pick<TabState, "filter" | "sort" | "pagination">>) {
    if (!activeTabId) return;
    setTabs((prev) => prev.map((t) => (t.id === activeTabId ? { ...t, ...updates } : t)));
  }

  function handleSelectTable(name: string) {
    const existing = tabs.find((t) => t.tableName === name);
    if (existing) {
      setActiveTabId(existing.id);
    } else {
      const id = crypto.randomUUID();
      setTabs((prev) => [...prev, {
        id,
        tableName: name,
        filter: "",
        sort: DEFAULT_SORT,
        pagination: DEFAULT_PAGINATION,
      }]);
      setActiveTabId(id);
    }
  }

  function handleJumpToRef(tableName: string, filterValue: FieldValue) {
    const filter = filterValue !== null ? String(filterValue) : "";
    const existing = tabs.find((t) => t.tableName === tableName);
    if (existing) {
      setTabs((prev) => prev.map((t) => t.id === existing.id ? { ...t, filter, pagination: DEFAULT_PAGINATION } : t));
      setActiveTabId(existing.id);
    } else {
      const id = crypto.randomUUID();
      setTabs((prev) => [...prev, { id, tableName, filter, sort: DEFAULT_SORT, pagination: DEFAULT_PAGINATION }]);
      setActiveTabId(id);
    }
  }

  function handleCloseTab(id: string) {
    setTabs((prev) => {
      const idx = prev.findIndex((t) => t.id === id);
      const next = prev.filter((t) => t.id !== id);
      if (id === activeTabId) {
        setActiveTabId(next.length > 0 ? next[Math.min(idx, next.length - 1)].id : null);
      }
      return next;
    });
  }

  function handleCloseAll() {
    setTabs([]);
    setActiveTabId(null);
  }

  function handleToggleSidebar() {
    if (tabs.length === 0) return;
    setSidebarOpen((v) => !v);
  }

  function handleSortChange(column: string) {
    if (!activeTab) return;
    const { sort } = activeTab;
    let newSort: SortState;
    if (sort.column === column) {
      newSort = sort.direction === "asc"
        ? { column, direction: "desc" }
        : { column: null, direction: "asc" };
    } else {
      newSort = { column, direction: "asc" };
    }
    updateActiveTab({ sort: newSort, pagination: { ...activeTab.pagination, page: 1 } });
  }

  function handleCreateRecord(row: RecordRow) {
    if (!selectedTable) return;
    setRecords((prev) => {
      const next = new Map(prev);
      next.set(selectedTable.name, [...(prev.get(selectedTable.name) ?? []), row]);
      return next;
    });
  }

  function handleUpdateRecord(row: RecordRow) {
    if (!selectedTable) return;
    const pkCol = selectedTable.columns.find((c) => c.isPrimaryKey);
    setRecords((prev) => {
      const next = new Map(prev);
      const rows = prev.get(selectedTable.name) ?? [];
      next.set(
        selectedTable.name,
        pkCol ? rows.map((r) => (r[pkCol.name] === row[pkCol.name] ? row : r)) : rows
      );
      return next;
    });
  }

  function handleDeleteRecord() {
    if (!selectedTable || !deletingRow) return;
    const pkCol = selectedTable.columns.find((c) => c.isPrimaryKey);
    setRecords((prev) => {
      const next = new Map(prev);
      const rows = prev.get(selectedTable.name) ?? [];
      next.set(
        selectedTable.name,
        pkCol
          ? rows.filter((r) => r[pkCol.name] !== deletingRow[pkCol.name])
          : rows.filter((r) => r !== deletingRow)
      );
      return next;
    });
    setDeletingRow(null);
  }

  function handleBulkDelete(selectedRows: RecordRow[]) {
    if (!selectedTable) return;
    const pkCol = selectedTable.columns.find((c) => c.isPrimaryKey);
    setRecords((prev) => {
      const next = new Map(prev);
      const rows = prev.get(selectedTable.name) ?? [];
      if (pkCol) {
        const pks = new Set(selectedRows.map((r) => r[pkCol.name]));
        next.set(selectedTable.name, rows.filter((r) => !pks.has(r[pkCol.name])));
      } else {
        const set = new Set(selectedRows);
        next.set(selectedTable.name, rows.filter((r) => !set.has(r)));
      }
      return next;
    });
  }

  const recordCounts = new Map(tables.map((t) => [t.name, records.get(t.name)?.length ?? 0]));
  const effectiveSidebarOpen = tabs.length === 0 ? true : sidebarOpen;

  return (
    <TooltipProvider>
      <AppShell
        sidebarOpen={effectiveSidebarOpen}
        sidebar={
          <Sidebar
            tables={tables}
            recordCounts={recordCounts}
            selectedTableName={activeTab?.tableName ?? null}
            onSelectTable={handleSelectTable}
          />
        }
      >
        <TabBar
          tabs={tabs}
          activeTabId={activeTabId}
          tables={tables}
          recordCounts={recordCounts}
          onActivate={setActiveTabId}
          onClose={handleCloseTab}
          onCloseAll={handleCloseAll}
          sidebarOpen={effectiveSidebarOpen}
          onToggleSidebar={handleToggleSidebar}
        />
        {selectedTable && activeTab ? (
          <>
            <DataTable
              key={activeTab.id}
              columns={selectedTable.columns}
              rows={currentRows}
              filter={activeTab.filter}
              sort={activeTab.sort}
              pagination={activeTab.pagination}
              onFilterChange={(filter) => updateActiveTab({ filter })}
              onSortChange={handleSortChange}
              onPageChange={(page) => updateActiveTab({ pagination: { ...activeTab.pagination, page } })}
              onPageSizeChange={(pageSize) => updateActiveTab({ pagination: { page: 1, pageSize } })}
              onAddRecord={() => setCreateOpen(true)}
              onEditRecord={(row) => { setEditingRow(row); setEditOpen(true); }}
              onDeleteRecord={(row) => { setDeletingRow(row); setDeleteOpen(true); }}
              onBulkDelete={handleBulkDelete}
              onJumpToRef={handleJumpToRef}
              allTables={tables}
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
          <div className="flex flex-1 flex-col items-center justify-center gap-2 text-muted-foreground">
            <p className="text-sm">Open a model from the sidebar to get started.</p>
          </div>
        )}
      </AppShell>
    </TooltipProvider>
  );
}
