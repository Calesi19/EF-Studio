/* eslint-disable react-refresh/only-export-components */
import { createContext, type ReactNode, useContext, useState } from "react";
import { useQueries } from "@tanstack/react-query";
import { useDeleteRecords } from "@/api/data/deleteRecords";
import { tableDataQueryOptions } from "@/api/data/fetchTableData";
import { useSchema } from "@/api/schema/fetchSchema";
import type { FieldValue, SortState, TableDef, TabState } from "@/types";
import { DEFAULT_PAGE_NUMBER, DEFAULT_PAGE_SIZE, DEFAULT_SORT } from "@/pages/StudioPage/constants";
import { useSettings, type NameDisplay } from "@/hooks/useSettings";

const STORAGE_KEY_PAGE_SIZE = "ef-studio-page-size";

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
  activeTableDeleteError: string | null;
  deletingRows: boolean;
  deleteSelectionResetKey: number;
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
  deleteRows: (rows: TableDef["rows"]) => Promise<void>;
  nameDisplay: NameDisplay;
  setNameDisplay: (value: NameDisplay) => void;
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
  const [tableDeleteErrors, setTableDeleteErrors] = useState<Record<string, string>>({});
  const [deleteSelectionResetKey, setDeleteSelectionResetKey] = useState(0);
  const { nameDisplay, setNameDisplay } = useSettings();
  const [pageSize, setPageSize] = useState<number>(() => {
    const stored = localStorage.getItem(STORAGE_KEY_PAGE_SIZE);
    const parsed = stored ? Number(stored) : NaN;
    return Number.isFinite(parsed) ? parsed : DEFAULT_PAGE_SIZE;
  });
  const { data: schema = [], isLoading: isSchemaLoading, error: schemaError } = useSchema();
  const deleteRecordsMutation = useDeleteRecords();

  const openTableKeys = [...new Set(tabs.map((tab) => tab.tableKey))];

  const tableDataQueries = useQueries({
    queries: openTableKeys.map((key) => tableDataQueryOptions(key)),
  });

  const tableDataMap = new Map(openTableKeys.map((key, index) => [key, tableDataQueries[index]]));

  const tables = schema.map((table) => ({
    ...table,
    rows: tableDataMap.get(table.key)?.data ?? [],
  }));

  const tableLoadErrors = openTableKeys.flatMap((key) => {
    const query = tableDataMap.get(key);
    const table = schema.find((t) => t.key === key);

    return query?.error && table
      ? [{ tableKey: key, tableName: nameDisplay === "model" ? table.modelDisplayName : table.name, message: toErrorMessage(query.error) }]
      : [];
  });

  const loading = isSchemaLoading;
  const error = schemaError ? toErrorMessage(schemaError) : null;
  const activeTab = tabs.find((tab) => tab.id === activeTabId) ?? null;
  const selectedTable = activeTab ? (tables.find((table) => table.key === activeTab.tableKey) ?? null) : null;
  const currentRows = selectedTable?.rows ?? [];
  const effectiveSidebarOpen = tabs.length === 0 ? true : sidebarOpen;
  const activeTableQuery = activeTab ? tableDataMap.get(activeTab.tableKey) : undefined;
  const activeTableError = activeTableQuery?.error ? toErrorMessage(activeTableQuery.error) : null;
  const activeTableLoading = activeTableQuery?.isLoading ?? false;
  const activeTableDeleteError = activeTab ? (tableDeleteErrors[activeTab.tableKey] ?? null) : null;

  function createTab(tableKey: string, filter = ""): TabState {
    return {
      id: crypto.randomUUID(),
      tableKey,
      filter,
      sort: DEFAULT_SORT,
      pagination: { page: DEFAULT_PAGE_NUMBER, pageSize },
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
            ? { ...tab, filter, pagination: { page: DEFAULT_PAGE_NUMBER, pageSize } }
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

  function changePageSize(newSize: number) {
    setPageSize(newSize);
    localStorage.setItem(STORAGE_KEY_PAGE_SIZE, String(newSize));
    setTabs((prev) =>
      prev.map((tab) => ({
        ...tab,
        pagination: { page: DEFAULT_PAGE_NUMBER, pageSize: newSize },
      }))
    );
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

  async function deleteRows(rows: TableDef["rows"]) {
    if (!activeTab || !selectedTable) {
      return;
    }

    const primaryKeyColumns = selectedTable.columns.filter((column) => column.isPrimaryKey);
    if (primaryKeyColumns.length === 0) {
      const message = `The table '${selectedTable.name}' does not support delete operations because it has no primary key.`;
      setTableDeleteErrors((current) => ({
        ...current,
        [activeTab.tableKey]: message,
      }));
      throw new Error(message);
    }

    setTableDeleteErrors((current) => {
      const next = { ...current };
      delete next[activeTab.tableKey];
      return next;
    });

    try {
      await deleteRecordsMutation.mutateAsync({
        tableKey: activeTab.tableKey,
        keys: rows.map((row) =>
          Object.fromEntries(primaryKeyColumns.map((column) => [column.name, row[column.name] ?? null])),
        ),
      });
      setDeleteSelectionResetKey((current) => current + 1);
    } catch (error) {
      const message = toErrorMessage(error);
      setTableDeleteErrors((current) => ({
        ...current,
        [activeTab.tableKey]: message,
      }));
      throw error;
    }
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
        activeTableDeleteError,
        deletingRows: deleteRecordsMutation.isPending,
        deleteSelectionResetKey,
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
        deleteRows,
        nameDisplay,
        setNameDisplay,
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
