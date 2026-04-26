/* eslint-disable react-refresh/only-export-components */
import { createContext, type ReactNode, useContext, useEffect, useState } from "react";
import { useQueries, useQueryClient } from "@tanstack/react-query";
import { useDbContexts, useSelectDbContext } from "@/api/contexts/fetchDbContexts";
import { useCreateRecords } from "@/api/data/createRecords";
import { tableDataQueryOptions, type TablePageData } from "@/api/data/fetchTableData";
import { useDeleteRecords } from "@/api/data/deleteRecords";
import { useUpdateRecords } from "@/api/data/updateRecords";
import { buildCreateDraftRow } from "@/components/records/CreateRecordDrawer";
import { useSchema } from "@/api/schema/fetchSchema";
import type { ColumnDef, DbContextDef, FieldValue, PendingEdits, RecordRow, SortState, TableDef, TabState } from "@/types";
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
  deleteSelectionResetKey: number;
  activeTableDeleteError: string | null;
  deletingRows: boolean;
  deleteRows: (rows: RecordRow[]) => Promise<void>;
  createDrawerOpen: boolean;
  createDraftRow: RecordRow;
  creatingRows: boolean;
  activeTableCreateError: string | null;
  openCreateDrawer: () => void;
  closeCreateDrawer: () => void;
  updateCreateDraftRow: (field: string, value: FieldValue) => void;
  resetCreateDraftRow: () => void;
  submitCreateRow: () => Promise<void>;
  pendingEdits: PendingEdits;
  pendingEditCount: number;
  savingEdits: boolean;
  activeTableUpdateError: string | null;
  setCellEdit: (row: RecordRow, columnName: string, value: FieldValue) => void;
  saveEdits: () => Promise<void>;
  discardEdits: () => void;
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

  const [tableDeleteErrors, setTableDeleteErrors] = useState<Record<string, string>>({});
  const [deleteSelectionResetKey, setDeleteSelectionResetKey] = useState(0);
  const [createDrawerOpen, setCreateDrawerOpen] = useState(false);
  const [createDraftRow, setCreateDraftRow] = useState<RecordRow>({});
  const [tableCreateErrors, setTableCreateErrors] = useState<Record<string, string>>({});
  const [pendingEdits, setPendingEdits] = useState<PendingEdits>(new Map());
  const [tableUpdateErrors, setTableUpdateErrors] = useState<Record<string, string>>({});

  const { data: contexts = [], isLoading: isContextsLoading, error: contextsError } = useDbContexts();
  const selectDbContextMutation = useSelectDbContext();
  const createRecordsMutation = useCreateRecords();
  const deleteRecordsMutation = useDeleteRecords();
  const updateRecordsMutation = useUpdateRecords();

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
  const activeTableLoading =
    activeTableQuery != null
      ? activeTableQuery.isLoading && activeTableQuery.data === undefined
      : false;
  const activeTableDeleteError = activeTab ? (tableDeleteErrors[activeTab.tableKey] ?? null) : null;
  const activeTableCreateError = activeTab ? (tableCreateErrors[activeTab.tableKey] ?? null) : null;
  const creatingRows = createRecordsMutation.isPending;
  const deletingRows = deleteRecordsMutation.isPending;
  const pendingEditCount = pendingEdits.size;
  const savingEdits = updateRecordsMutation.isPending;
  const activeTableUpdateError = activeTab ? (tableUpdateErrors[activeTab.tableKey] ?? null) : null;

  useEffect(() => {
    setPendingEdits(new Map());
  }, [activeTabId]);

  useEffect(() => {
    setCreateDrawerOpen(false);
    setCreateDraftRow({});
  }, [activeTabId]);

  function serializeRowPk(row: RecordRow, pkColumns: ColumnDef[]): string {
    return pkColumns.map((col) => `${col.name}:${String(row[col.name])}`).join("|");
  }

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

  function clearCreateError(tableKey: string) {
    setTableCreateErrors((current) => {
      const next = { ...current };
      delete next[tableKey];
      return next;
    });
  }

  function openCreateDrawer() {
    if (!selectedTable || !activeTab) {
      return;
    }

    clearCreateError(activeTab.tableKey);
    setCreateDraftRow(buildCreateDraftRow(selectedTable));
    setCreateDrawerOpen(true);
  }

  function closeCreateDrawer() {
    setCreateDrawerOpen(false);
    setCreateDraftRow({});
  }

  function updateCreateDraftRow(field: string, value: FieldValue) {
    setCreateDraftRow((current) => ({ ...current, [field]: value }));
  }

  function resetCreateDraftRow() {
    if (!selectedTable) return;
    setCreateDraftRow(buildCreateDraftRow(selectedTable));
  }

  async function submitCreateRow() {
    if (!activeTab || !selectedTable || !selectedContextName) {
      return;
    }

    clearCreateError(activeTab.tableKey);

    try {
      await createRecordsMutation.mutateAsync({
        tableKey: activeTab.tableKey,
        records: [createDraftRow],
      });

      await queryClient.invalidateQueries({
        queryKey: ["table-data", selectedContextName, activeTab.tableKey],
      });
    } catch (error) {
      const message = toErrorMessage(error);
      setTableCreateErrors((current) => ({ ...current, [activeTab.tableKey]: message }));
      throw error;
    }
  }

  async function deleteRows(rows: RecordRow[]) {
    if (!activeTab || !selectedTable) {
      return;
    }

    const primaryKeyColumns = selectedTable.columns.filter((column) => column.isPrimaryKey);
    if (primaryKeyColumns.length === 0) {
      const message = `The table '${selectedTable.name}' does not support delete operations because it has no primary key.`;
      setTableDeleteErrors((current) => ({ ...current, [activeTab.tableKey]: message }));
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
      setTableDeleteErrors((current) => ({ ...current, [activeTab.tableKey]: message }));
      throw error;
    }
  }

  function setCellEdit(row: RecordRow, columnName: string, value: FieldValue) {
    if (!selectedTable) return;

    const pkColumns = selectedTable.columns.filter((col) => col.isPrimaryKey);
    if (pkColumns.length === 0) return;

    const rowPk = serializeRowPk(row, pkColumns);
    const originalValue = row[columnName] ?? null;

    setPendingEdits((current) => {
      const next = new Map(current);
      const rowEdits = { ...(next.get(rowPk) ?? {}) };

      if (value === originalValue || (value === "" && originalValue === null)) {
        delete rowEdits[columnName];
      } else {
        rowEdits[columnName] = value;
      }

      if (Object.keys(rowEdits).length === 0) {
        next.delete(rowPk);
      } else {
        next.set(rowPk, rowEdits);
      }

      return next;
    });
  }

  async function saveEdits() {
    if (!activeTab || !selectedTable) return;

    const pkColumns = selectedTable.columns.filter((col) => col.isPrimaryKey);
    if (pkColumns.length === 0) {
      const message = `The table '${selectedTable.name}' does not support update operations because it has no primary key.`;
      setTableUpdateErrors((current) => ({ ...current, [activeTab.tableKey]: message }));
      throw new Error(message);
    }

    setTableUpdateErrors((current) => {
      const next = { ...current };
      delete next[activeTab.tableKey];
      return next;
    });

    const updates = Array.from(pendingEdits.entries()).map(([rowPk, changedValues]) => {
      const keys = Object.fromEntries(
        rowPk.split("|").map((part) => {
          const colonIndex = part.indexOf(":");
          return [part.slice(0, colonIndex), part.slice(colonIndex + 1)] as [string, FieldValue];
        })
      );
      return { keys, values: changedValues };
    });

    try {
      await updateRecordsMutation.mutateAsync({
        tableKey: activeTab.tableKey,
        updates,
      });

      queryClient.setQueriesData<TablePageData>(
        { queryKey: ["table-data", selectedContextName, activeTab.tableKey] },
        (old) => {
          if (!old) return old;
          return {
            ...old,
            rows: old.rows.map((row) => {
              const rowPk = serializeRowPk(row, pkColumns);
              const edits = pendingEdits.get(rowPk);
              return edits ? { ...row, ...edits } : row;
            }),
          };
        },
      );

      setPendingEdits(new Map());
    } catch (error) {
      const message = error instanceof Error ? error.message : "Failed to save changes.";
      setTableUpdateErrors((current) => ({ ...current, [activeTab.tableKey]: message }));
      throw error;
    }
  }

  function discardEdits() {
    setPendingEdits(new Map());
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
        deleteSelectionResetKey,
        activeTableDeleteError,
        deletingRows,
        deleteRows,
        createDrawerOpen,
        createDraftRow,
        creatingRows,
        activeTableCreateError,
        openCreateDrawer,
        closeCreateDrawer,
        updateCreateDraftRow,
        resetCreateDraftRow,
        submitCreateRow,
        pendingEdits,
        pendingEditCount,
        savingEdits,
        activeTableUpdateError,
        setCellEdit,
        saveEdits,
        discardEdits,
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
