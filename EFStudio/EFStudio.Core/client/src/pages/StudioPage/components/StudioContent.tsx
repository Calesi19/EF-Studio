import { AppShell } from "@/components/layout/AppShell";
import { TabBar } from "@/components/layout/TabBar";
import { Sidebar } from "@/components/sidebar/Sidebar";
import { useStudioContext } from "@/pages/StudioPage/context/StudioContext";
import { StudioStatus } from "@/pages/StudioPage/components/StudioStatus";
import { StudioWorkspace } from "@/pages/StudioPage/components/StudioWorkspace";

export function StudioContent() {
  const {
    activeTab,
    activeTabId,
    closeAllTabs,
    closeTab,
    effectiveSidebarOpen,
    error,
    loading,
    selectedTable,
    selectTable,
    setActiveTabId,
    tabs,
    tables,
    toggleSidebar,
  } = useStudioContext();

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
        <StudioStatus
          title="Loading EFStudio data"
          message="Loading database schema and records..."
        />
      ) : error ? (
        <StudioStatus title="Unable to load EFStudio data" message={error} />
      ) : selectedTable && activeTab ? (
        <StudioWorkspace />
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
    </AppShell>
  );
}
