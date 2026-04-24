import { useEffect, useState } from "react";
import { TooltipProvider } from "@/components/ui/tooltip";
import { AppShell } from "@/components/layout/AppShell";
import { TabBar } from "@/components/layout/TabBar";
import { Sidebar } from "@/components/sidebar/Sidebar";
import { DataTable } from "@/components/table/DataTable";
import { fetchTables } from "@/lib/api";
import type { FieldValue, PaginationState, SortState, TableDef, TabState } from "@/types";

const DEFAULT_SORT: SortState = { column: null, direction: "asc" };
const DEFAULT_PAGINATION: PaginationState = { page: 1, pageSize: 10 };

export default function App() {
  const [tables, setTables] = useState<TableDef[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [tabs, setTabs] = useState<TabState[]>([]);
  const [activeTabId, setActiveTabId] = useState<string | null>(null);
  const [sidebarOpen, setSidebarOpen] = useState(true);

  const activeTab = tabs.find((t) => t.id === activeTabId) ?? null;
  const selectedTable = activeTab ? (tables.find((t) => t.name === activeTab.tableName) ?? null) : null;
  const currentRows = selectedTable?.rows ?? [];

  useEffect(() => {
    const controller = new AbortController();

    async function loadTables() {
      try {
        setLoading(true);
        setError(null);
        const nextTables = await fetchTables(controller.signal);
        setTables(nextTables);
      } catch (err) {
        if (controller.signal.aborted) return;
        setError(err instanceof Error ? err.message : "Failed to load EFStudio data.");
      } finally {
        if (!controller.signal.aborted) {
          setLoading(false);
        }
      }
    }

    void loadTables();

    return () => controller.abort();
  }, []);

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
      setTabs((prev) => [
        ...prev,
        {
          id,
          tableName: name,
          filter: "",
          sort: DEFAULT_SORT,
          pagination: DEFAULT_PAGINATION,
        },
      ]);
      setActiveTabId(id);
    }
  }

  function handleJumpToRef(tableName: string, filterValue: FieldValue) {
    const filter = filterValue !== null ? String(filterValue) : "";
    const existing = tabs.find((t) => t.tableName === tableName);
    if (existing) {
      setTabs((prev) =>
        prev.map((t) =>
          t.id === existing.id ? { ...t, filter, pagination: DEFAULT_PAGINATION } : t,
        ),
      );
      setActiveTabId(existing.id);
    } else {
      const id = crypto.randomUUID();
      setTabs((prev) => [
        ...prev,
        { id, tableName, filter, sort: DEFAULT_SORT, pagination: DEFAULT_PAGINATION },
      ]);
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
      newSort =
        sort.direction === "asc"
          ? { column, direction: "desc" }
          : { column: null, direction: "asc" };
    } else {
      newSort = { column, direction: "asc" };
    }
    updateActiveTab({ sort: newSort, pagination: { ...activeTab.pagination, page: 1 } });
  }

  const recordCounts = new Map(tables.map((t) => [t.name, t.rows.length]));
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
        {loading ? (
          <div className="flex flex-1 items-center justify-center text-sm text-muted-foreground">
            Loading database schema and records...
          </div>
        ) : error ? (
          <div className="flex flex-1 flex-col items-center justify-center gap-2 px-6 text-center">
            <p className="text-sm font-medium text-foreground">Unable to load EFStudio data</p>
            <p className="max-w-md text-sm text-muted-foreground">{error}</p>
          </div>
        ) : selectedTable && activeTab ? (
          <DataTable
            key={activeTab.id}
            columns={selectedTable.columns}
            rows={currentRows}
            filter={activeTab.filter}
            sort={activeTab.sort}
            pagination={activeTab.pagination}
            onFilterChange={(filter) => updateActiveTab({ filter })}
            onSortChange={handleSortChange}
            onPageChange={(page) =>
              updateActiveTab({ pagination: { ...activeTab.pagination, page } })
            }
            onPageSizeChange={(pageSize) =>
              updateActiveTab({ pagination: { page: 1, pageSize } })
            }
            onAddRecord={() => {}}
            onEditRecord={() => {}}
            onDeleteRecord={() => {}}
            onBulkDelete={() => {}}
            onJumpToRef={handleJumpToRef}
            allTables={tables}
            readOnly
          />
        ) : (
          <div className="flex flex-1 flex-col items-center justify-center gap-2 text-muted-foreground">
            <p className="text-sm font-medium text-foreground">
              {tables.length === 0 ? "No tables found" : "Select a table to browse data"}
            </p>
            <p className="text-sm">
              {tables.length === 0
                ? "The middleware returned an empty schema."
                : "EFStudio is currently connected in read-only mode."}
            </p>
          </div>
        )}
      </AppShell>
    </TooltipProvider>
  );
}
