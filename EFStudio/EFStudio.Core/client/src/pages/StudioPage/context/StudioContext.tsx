/* eslint-disable react-refresh/only-export-components */
import { createContext, type ReactNode, useContext, useEffect, useState } from "react";
import { useQueries, useQueryClient } from "@tanstack/react-query";
import { useDbContexts, useSelectDbContext } from "@/api/contexts/fetchDbContexts";
import { tableDataQueryOptions } from "@/api/data/fetchTableData";
import { useSchema } from "@/api/schema/fetchSchema";
import type { DbContextDef, FieldValue, SortState, TableDef, TabState } from "@/types";
import { DEFAULT_PAGE_NUMBER, DEFAULT_PAGE_SIZE, DEFAULT_SORT } from "@/pages/StudioPage/constants";
import { useSettings, type NameDisplay } from "@/hooks/useSettings";

const STORAGE_KEY_PAGE_SIZE = "ef-studio-page-size";

type StudioContextType = {
  contexts: DbContextDef[];
  selectedContextName: string | null;
  tables: TableDef[];
  loading: boolean;
  error: string | null;
  tabs: TabState[];
  activeTabId: string | null;
  activeTab: TabState | null;
  selectedTable: TableDef | null;
  currentRows: TableDef["rows"];
  currentTotalRows: number;
  currentTotalPages: number;
  effectiveSidebarOpen: boolean;
  tableLoadErrors: { tableKey: string; tableName: string; message: string }[];
  activeTableError: string | null;
  activeTableLoading: boolean;
  setActiveTabId: (id: string) => void;
  selectContext: (contextName: string) => Promise<void>;
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
  const [selectedContextName, setSelectedContextName] = useState<string | null>(null);
  const { nameDisplay, setNameDisplay } = useSettings();
  const queryClient = useQueryClient();
  const [pageSize, setPageSize] = useState<number>(() => {
    const stored = localStorage.getItem(STORAGE_KEY_PAGE_SIZE);
    const parsed = stored ? Number(stored) : Number.NaN;
    return Number.isFinite(parsed) ? parsed : DEFAULT_PAGE_SIZE;
  });

  const { data: contexts = [], isLoading: isContextsLoading, error: contextsError } = useDbContexts();
  const selectDbContextMutation = useSelectDbContext();

  useEffect(() => {
    if (selectedContextName) {
      return;
    }

    const preferredContext = contexts.find((context) => context.isSelected || context.isDefault)
      ?? (contexts.length === 1 ? contexts[0] : null);

    if (preferredContext?.isAvailable) {
      setSelectedContextName(preferredContext.name);
    }
  }, [contexts, selectedContextName]);

  const { data: schema = [], isLoading: isSchemaLoading, error: schemaError } = useSchema(selectedContextName);

  const tableDataQueries = useQueries({
    queries: tabs.map((tab) =>
      tableDataQueryOptions(
        selectedContextName,
        tab.tableKey,
        tab.pagination,
        tab.filter,
        tab.sort,
      ),
    ),
  });

  const tableDataMap = new Map(tabs.map((tab, index) => [tab.id, tableDataQueries[index]]));

  const tables = schema.map((table) => ({
    ...table,
    rows: [],
  }));

  const tableLoadErrors = tabs.flatMap((tab) => {
    const query = tableDataMap.get(tab.id);
    const table = schema.find((candidate) => candidate.key === tab.tableKey);

    return query?.error && table
      ? [{
          tableKey: tab.tableKey,
          tableName: nameDisplay === "model" ? table.modelDisplayName : table.name,
          message: toErrorMessage(query.error),
        }]
      : [];
  });

  const loading = isContextsLoading || (!!selectedContextName && isSchemaLoading);
  const error = contextsError
    ? toErrorMessage(contextsError)
    : schemaError
      ? toErrorMessage(schemaError)
      : null;
  const activeTab = tabs.find((tab) => tab.id === activeTabId) ?? null;
  const selectedTable = activeTab ? (tables.find((table) => table.key === activeTab.tableKey) ?? null) : null;
  const activeTableQuery = activeTab ? tableDataMap.get(activeTab.id) : undefined;
  const currentRows = activeTableQuery?.data?.rows ?? [];
  const currentTotalRows = activeTableQuery?.data?.totalRows ?? 0;
  const currentTotalPages = activeTableQuery?.data?.totalPages ?? 1;
  const effectiveSidebarOpen = tabs.length === 0 ? true : sidebarOpen;
  const activeTableError = activeTableQuery?.error ? toErrorMessage(activeTableQuery.error) : null;
  const activeTableLoading = activeTableQuery?.isLoading ?? false;

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

  async function selectContext(contextName: string) {
    await selectDbContextMutation.mutateAsync(contextName);
    setSelectedContextName(contextName);
    setTabs([]);
    setActiveTabId(null);
    await queryClient.invalidateQueries({ queryKey: ["schema"] });
    await queryClient.invalidateQueries({ queryKey: ["table-data"] });
  }

  function selectTable(tableKey: string) {
    if (!selectedContextName) {
      return;
    }

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
    if (!selectedContextName) {
      return;
    }

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
    setTabs((previousTabs) =>
      previousTabs.map((tab) => ({
        ...tab,
        pagination: { page: DEFAULT_PAGE_NUMBER, pageSize: newSize },
      })),
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

  return (
    <StudioContext.Provider
      value={{
        contexts,
        selectedContextName,
        tables,
        loading,
        error,
        tabs,
        activeTabId,
        activeTab,
        selectedTable,
        currentRows,
        currentTotalRows,
        currentTotalPages,
        effectiveSidebarOpen,
        tableLoadErrors,
        activeTableError,
        activeTableLoading,
        setActiveTabId,
        selectContext,
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
