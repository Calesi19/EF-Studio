import { AppShell } from "@/components/layout/AppShell";
import { TabBar } from "@/components/layout/TabBar";
import { Sidebar } from "@/components/sidebar/Sidebar";
import { useStudioContext } from "@/pages/StudioPage/context/StudioContext";
import { StudioStatus } from "@/pages/StudioPage/components/StudioStatus";
import { StudioWorkspace } from "@/pages/StudioPage/components/StudioWorkspace";
import { DataTableSkeleton } from "@/components/table/DataTableSkeleton";
import { useState } from "react";
import { HugeiconsIcon } from "@hugeicons/react";
import { Cancel01Icon } from "@hugeicons/core-free-icons";

export function StudioContent() {
  const {
    activeTab,
    activeTabId,
    activeTableError,
    activeTableLoading,
    closeAllTabs,
    closeTab,
    effectiveSidebarOpen,
    error,
    loading,
    selectedTable,
    selectTable,
    setActiveTabId,
    tableLoadErrors,
    tabs,
    tables,
    toggleSidebar,
  } = useStudioContext();

  const [tableErrorsDismissed, setTableErrorsDismissed] = useState(false);

  const failedTablesLabel =
    tableLoadErrors.length === 1
      ? tableLoadErrors[0]?.tableName ?? "One table"
      : `${tableLoadErrors.length} tables`;

  return (
    <AppShell
      sidebarOpen={effectiveSidebarOpen}
      sidebar={
        <Sidebar
          tables={tables}
          selectedTableKey={activeTab?.tableKey ?? null}
          onSelectTable={selectTable}
        />
      }
    >
      <TabBar
        tabs={tabs}
        activeTabId={activeTabId}
        tables={tables}
        onActivate={setActiveTabId}
        onClose={closeTab}
        onCloseAll={closeAllTabs}
        sidebarOpen={effectiveSidebarOpen}
        onToggleSidebar={toggleSidebar}
      />
      {loading ? (
        <StudioStatus title="Loading EFStudio data" message="Loading database schema..." />
      ) : (
        <div className="flex min-h-0 flex-1 flex-col overflow-hidden">
          {error ? (
            <div className="mx-3 mt-3 rounded-lg border border-destructive/20 bg-destructive/10 px-3 py-2 text-xs text-destructive">
              {error}
            </div>
          ) : null}
          {tableLoadErrors.length > 0 && !tableErrorsDismissed ? (
            <div className="mx-3 mt-3 flex items-start gap-2 rounded-lg border border-amber-300/60 bg-amber-50 px-3 py-2 text-xs text-amber-950 dark:border-amber-800/60 dark:bg-amber-950/30 dark:text-amber-100">
              <span className="flex-1">
                <span className="font-medium">Some tables failed to load:</span>{" "}
                {failedTablesLabel}
                {tableLoadErrors.length > 1 ? (
                  <span className="ml-1 text-amber-700 dark:text-amber-200">
                    ({tableLoadErrors.map((entry) => entry.tableName).slice(0, 3).join(", ")}
                    {tableLoadErrors.length > 3 ? `, +${tableLoadErrors.length - 3} more` : ""})
                  </span>
                ) : null}
              </span>
              <button
                onClick={() => setTableErrorsDismissed(true)}
                className="shrink-0 text-amber-700 hover:text-amber-950 dark:text-amber-300 dark:hover:text-amber-100"
              >
                <HugeiconsIcon icon={Cancel01Icon} size={14} />
              </button>
            </div>
          ) : null}
          <div className="flex min-h-0 flex-1 flex-col overflow-hidden">
            {selectedTable && activeTab ? (
              activeTableError ? (
                <StudioStatus
                  title={`Failed to load ${selectedTable.displayName}`}
                  message={activeTableError}
                />
              ) : activeTableLoading ? (
                <DataTableSkeleton
                  columns={selectedTable.columns}
                  pageSize={activeTab.pagination.pageSize}
                />
              ) : (
                <StudioWorkspace />
              )
            ) : (
              <StudioStatus
                title={tables.length === 0 ? "No tables found" : "Select a table to browse data"}
                message={
                  tables.length === 0
                    ? "The middleware returned an empty schema."
                    : "EFStudio is currently connected in read-only mode."
                }
              />
            )}
          </div>
        </div>
      )}
    </AppShell>
  );
}
