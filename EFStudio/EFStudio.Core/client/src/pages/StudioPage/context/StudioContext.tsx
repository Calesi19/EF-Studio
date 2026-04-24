/* eslint-disable react-refresh/only-export-components */
import { createContext, type ReactNode, useContext, useState } from "react";
import { useQueries } from "@tanstack/react-query";
import { tableDataQueryOptions } from "@/api/data/fetchTableData";
import { useSchema } from "@/api/schema/fetchSchema";
import type { FieldValue, SortState, TableDef, TabState } from "@/types";
import { DEFAULT_PAGE_NUMBER, DEFAULT_PAGINATION, DEFAULT_SORT } from "@/pages/StudioPage/constants";

type StudioContextType = {
  tables: TableDef[];
  loading: boolean;
  error: string | null;
  tabs: TabState[];
  activeTabId: string | null;
  activeTab: TabState | null;
  selectedTable: TableDef | null;
  currentRows: TableDef["rows"];
  effectiveSidebarOpen: boolean;
  tableLoadErrors: { tableKey: string; tableName: string; message: string }[];
  activeTableError: string | null;
  activeTableLoading: boolean;
  setActiveTabId: (id: string) => void;
  selectTable: (tableKey: string) => void;
  jumpToReference: (tableKey: string, filterValue: FieldValue) => void;
  closeTab: (id: string) => void;
  closeAllTabs: () => void;
  toggleSidebar: () => void;
  updateActiveTab: (updates: Partial<Pick<TabState, "filter" | "sort" | "pagination">>) => void;
  changeSort: (column: string) => void;
  changePage: (page: number) => void;
  changePageSize: (pageSize: number) => void;
  changeFilter: (filter: string) => void;
};

const StudioContext = createContext<StudioContextType | undefined>(undefined);

function toErrorMessage(error: unknown): string {
  if (error instanceof Error) {
    return error.message;
  }

  return "Failed to load EFStudio data.";
}

export function StudioContextProvider({ children }: { children: ReactNode }) {
  const [tabs, setTabs] = useState<TabState[]>([]);
  const [activeTabId, setActiveTabId] = useState<string | null>(null);
  const [sidebarOpen, setSidebarOpen] = useState(true);
  const { data: schema = [], isLoading: isSchemaLoading, error: schemaError } = useSchema();

  const tableDataQueries = useQueries({
    queries: schema.map((table) => tableDataQueryOptions(table.key)),
  });

  const tables = schema.map((table, index) => ({
    ...table,
    rows: tableDataQueries[index]?.data ?? [],
  }));

  const tableLoadErrors = schema.flatMap((table, index) => {
    const query = tableDataQueries[index];

    return query?.error
      ? [
          {
            tableKey: table.key,
            tableName: table.displayName,
            message: toErrorMessage(query.error),
          },
        ]
      : [];
  });
  const loading = isSchemaLoading;
  const error = schemaError ? toErrorMessage(schemaError) : null;
  const activeTab = tabs.find((tab) => tab.id === activeTabId) ?? null;
  const selectedTable = activeTab ? (tables.find((table) => table.key === activeTab.tableKey) ?? null) : null;
  const currentRows = selectedTable?.rows ?? [];
  const effectiveSidebarOpen = tabs.length === 0 ? true : sidebarOpen;
  const activeTabIndex = activeTab ? schema.findIndex((table) => table.key === activeTab.tableKey) : -1;
  const activeTableQuery = activeTabIndex >= 0 ? tableDataQueries[activeTabIndex] : undefined;
  const activeTableError = activeTableQuery?.error ? toErrorMessage(activeTableQuery.error) : null;
  const activeTableLoading = activeTableQuery?.isLoading ?? false;

  function createTab(tableKey: string, filter = ""): TabState {
    return {
      id: crypto.randomUUID(),
      tableKey,
      filter,
      sort: DEFAULT_SORT,
      pagination: DEFAULT_PAGINATION,
    };
  }

  function updateActiveTab(updates: Partial<Pick<TabState, "filter" | "sort" | "pagination">>) {
    if (!activeTabId) {
      return;
    }

    setTabs((previousTabs) =>
      previousTabs.map((tab) => (tab.id === activeTabId ? { ...tab, ...updates } : tab)),
    );
  }

  function selectTable(tableKey: string) {
    const existingTab = tabs.find((tab) => tab.tableKey === tableKey);

    if (existingTab) {
      setActiveTabId(existingTab.id);
      return;
    }

    const nextTab = createTab(tableKey);
    setTabs((previousTabs) => [...previousTabs, nextTab]);
    setActiveTabId(nextTab.id);
  }

  function jumpToReference(tableKey: string, filterValue: FieldValue) {
    const filter = filterValue !== null ? String(filterValue) : "";
    const existingTab = tabs.find((tab) => tab.tableKey === tableKey);

    if (existingTab) {
      setTabs((previousTabs) =>
        previousTabs.map((tab) =>
          tab.id === existingTab.id
            ? { ...tab, filter, pagination: DEFAULT_PAGINATION }
            : tab,
        ),
      );
      setActiveTabId(existingTab.id);
      return;
    }

    const nextTab = createTab(tableKey, filter);
    setTabs((previousTabs) => [...previousTabs, nextTab]);
    setActiveTabId(nextTab.id);
  }

  function closeTab(id: string) {
    setTabs((previousTabs) => {
      const closedTabIndex = previousTabs.findIndex((tab) => tab.id === id);
      const nextTabs = previousTabs.filter((tab) => tab.id !== id);

      if (id === activeTabId) {
        const nextActiveTab = nextTabs.length > 0 ? nextTabs[Math.min(closedTabIndex, nextTabs.length - 1)] : null;
        setActiveTabId(nextActiveTab?.id ?? null);
      }

      return nextTabs;
    });
  }

  function closeAllTabs() {
    setTabs([]);
    setActiveTabId(null);
  }

  function toggleSidebar() {
    if (tabs.length === 0) {
      return;
    }

    setSidebarOpen((currentValue) => !currentValue);
  }

  function changeSort(column: string) {
    if (!activeTab) {
      return;
    }

    const nextSort: SortState =
      activeTab.sort.column === column
        ? activeTab.sort.direction === "asc"
          ? { column, direction: "desc" }
          : DEFAULT_SORT
        : { column, direction: "asc" };

    updateActiveTab({
      sort: nextSort,
      pagination: { ...activeTab.pagination, page: DEFAULT_PAGE_NUMBER },
    });
  }

  function changePage(page: number) {
    if (!activeTab) {
      return;
    }

    updateActiveTab({
      pagination: { ...activeTab.pagination, page },
    });
  }

  function changePageSize(pageSize: number) {
    updateActiveTab({
      pagination: { page: DEFAULT_PAGE_NUMBER, pageSize },
    });
  }

  function changeFilter(filter: string) {
    if (!activeTab) {
      return;
    }

    updateActiveTab({
      filter,
      pagination: { ...activeTab.pagination, page: DEFAULT_PAGE_NUMBER },
    });
  }

  return (
    <StudioContext.Provider
      value={{
        tables,
        loading,
        error,
        tabs,
        activeTabId,
        activeTab,
        selectedTable,
        currentRows,
        effectiveSidebarOpen,
        tableLoadErrors,
        activeTableError,
        activeTableLoading,
        setActiveTabId,
        selectTable,
        jumpToReference,
        closeTab,
        closeAllTabs,
        toggleSidebar,
        updateActiveTab,
        changeSort,
        changePage,
        changePageSize,
        changeFilter,
      }}
    >
      {children}
    </StudioContext.Provider>
  );
}

export function useStudioContext() {
  const context = useContext(StudioContext);

  if (!context) {
    throw new Error("useStudioContext must be used within StudioContextProvider");
  }

  return context;
}
