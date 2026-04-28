import { act, render, renderHook, screen, waitFor } from "@testing-library/react";
import { http, HttpResponse } from "msw";
import { describe, expect, it } from "vitest";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { StudioContextProvider, useStudioContext } from "../StudioContext";
import { server } from "@/test/setup";
import {
  defaultSchemaResponse,
  defaultTableDataResponse,
} from "@/test/handlers";

function makeTestQueryClient() {
  return new QueryClient({
    defaultOptions: { queries: { retry: false, staleTime: 0 }, mutations: { retry: false } },
  });
}

function Wrapper({ children }: { children: React.ReactNode }) {
  const queryClient = makeTestQueryClient();
  return (
    <QueryClientProvider client={queryClient}>
      <StudioContextProvider>{children}</StudioContextProvider>
    </QueryClientProvider>
  );
}

async function setupWithContext() {
  const { result } = renderHook(() => useStudioContext(), { wrapper: Wrapper });
  await waitFor(() => expect(result.current.contexts).toHaveLength(1));
  // Wait for auto-selection of the context
  await waitFor(() => expect(result.current.selectedContextName).toBe("AppDbContext"));
  await waitFor(() => expect(result.current.tables).toHaveLength(1));
  return result;
}

describe("StudioContext — initial loading", () => {
  it("auto-selects the default context on mount", async () => {
    const result = await setupWithContext();
    expect(result.current.selectedContextName).toBe("AppDbContext");
  });

  it("loads schema tables after context is selected", async () => {
    const result = await setupWithContext();
    expect(result.current.tables).toHaveLength(1);
    expect(result.current.tables[0].key).toBe("Users");
  });

  it("starts with no tabs open", async () => {
    const result = await setupWithContext();
    expect(result.current.tabs).toHaveLength(0);
    expect(result.current.activeTabId).toBeNull();
  });
});

describe("StudioContext — tab management", () => {
  it("selectTable creates a new tab", async () => {
    const result = await setupWithContext();
    act(() => result.current.selectTable("Users"));
    expect(result.current.tabs).toHaveLength(1);
    expect(result.current.tabs[0].tableKey).toBe("Users");
    expect(result.current.activeTabId).toBe(result.current.tabs[0].id);
  });

  it("selectTable on same table focuses existing tab instead of creating new one", async () => {
    const result = await setupWithContext();
    act(() => result.current.selectTable("Users"));
    const firstTabId = result.current.tabs[0].id;
    act(() => result.current.selectTable("Users"));
    expect(result.current.tabs).toHaveLength(1);
    expect(result.current.activeTabId).toBe(firstTabId);
  });

  it("closeTab removes the tab and updates activeTabId", async () => {
    const result = await setupWithContext();
    act(() => result.current.selectTable("Users"));
    const tabId = result.current.tabs[0].id;
    act(() => result.current.closeTab(tabId));
    expect(result.current.tabs).toHaveLength(0);
    expect(result.current.activeTabId).toBeNull();
  });

  it("closeAllTabs clears all tabs", async () => {
    const result = await setupWithContext();
    act(() => result.current.selectTable("Users"));
    act(() => result.current.closeAllTabs());
    expect(result.current.tabs).toHaveLength(0);
    expect(result.current.activeTabId).toBeNull();
  });
});

describe("StudioContext — sort and filter", () => {
  it("changeSort sets sort column and resets pagination to page 1", async () => {
    const result = await setupWithContext();
    act(() => result.current.selectTable("Users"));
    act(() => {
      result.current.changePage(3);
      result.current.changeSort("Name");
    });
    expect(result.current.activeTab?.sort.column).toBe("Name");
    expect(result.current.activeTab?.sort.direction).toBe("asc");
    expect(result.current.activeTab?.pagination.page).toBe(1);
  });

  it("changeSort on same column toggles to desc then resets to null", async () => {
    const result = await setupWithContext();
    act(() => result.current.selectTable("Users"));
    act(() => result.current.changeSort("Name"));
    expect(result.current.activeTab?.sort).toEqual({ column: "Name", direction: "asc" });
    act(() => result.current.changeSort("Name"));
    expect(result.current.activeTab?.sort).toEqual({ column: "Name", direction: "desc" });
    act(() => result.current.changeSort("Name"));
    expect(result.current.activeTab?.sort).toEqual({ column: null, direction: "asc" });
  });

  it("changeFilter updates filter and resets pagination", async () => {
    const result = await setupWithContext();
    act(() => result.current.selectTable("Users"));
    act(() => result.current.changeFilter("alice"));
    expect(result.current.activeTab?.filter).toBe("alice");
    expect(result.current.activeTab?.pagination.page).toBe(1);
  });

  it("changePage updates page in active tab", async () => {
    const result = await setupWithContext();
    act(() => result.current.selectTable("Users"));
    act(() => result.current.changePage(2));
    expect(result.current.activeTab?.pagination.page).toBe(2);
  });

  it("changePageSize updates pageSize across all tabs", async () => {
    const result = await setupWithContext();
    act(() => result.current.selectTable("Users"));
    act(() => result.current.changePageSize(50));
    expect(result.current.activeTab?.pagination.pageSize).toBe(50);
    expect(localStorage.getItem("ef-studio-page-size")).toBe("50");
  });
});

describe("StudioContext — pending edits", () => {
  it("setCellEdit accumulates pending edit for a row", async () => {
    const result = await setupWithContext();
    act(() => result.current.selectTable("Users"));
    // Wait for table data to load
    await waitFor(() => expect(result.current.tables[0]).toBeDefined());

    const row = { Id: 1, Name: "Alice", Email: "alice@example.com" };
    act(() => result.current.setCellEdit(row, "Name", "Alicia"));
    expect(result.current.pendingEditCount).toBe(1);
  });

  it("setCellEdit removes edit when value reverts to original", async () => {
    const result = await setupWithContext();
    act(() => result.current.selectTable("Users"));
    await waitFor(() => expect(result.current.tables[0]).toBeDefined());

    const row = { Id: 1, Name: "Alice", Email: "alice@example.com" };
    act(() => result.current.setCellEdit(row, "Name", "Alicia"));
    expect(result.current.pendingEditCount).toBe(1);
    act(() => result.current.setCellEdit(row, "Name", "Alice"));
    expect(result.current.pendingEditCount).toBe(0);
  });

  it("discardEdits clears all pending edits", async () => {
    const result = await setupWithContext();
    act(() => result.current.selectTable("Users"));
    await waitFor(() => expect(result.current.tables[0]).toBeDefined());

    const row = { Id: 1, Name: "Alice", Email: "alice@example.com" };
    act(() => result.current.setCellEdit(row, "Name", "Alicia"));
    act(() => result.current.discardEdits());
    expect(result.current.pendingEditCount).toBe(0);
  });
});

describe("StudioContext — jumpToReference", () => {
  it("creates new tab with pre-filled filter when table not already open", async () => {
    const result = await setupWithContext();
    act(() => result.current.jumpToReference("Users", 42));
    expect(result.current.tabs).toHaveLength(1);
    expect(result.current.tabs[0].tableKey).toBe("Users");
    expect(result.current.tabs[0].filter).toBe("42");
  });

  it("updates filter on existing tab when table already open", async () => {
    const result = await setupWithContext();
    act(() => result.current.selectTable("Users"));
    const originalTabId = result.current.tabs[0].id;
    act(() => result.current.jumpToReference("Users", "alice"));
    expect(result.current.tabs).toHaveLength(1);
    expect(result.current.tabs[0].id).toBe(originalTabId);
    expect(result.current.tabs[0].filter).toBe("alice");
  });

  it("uses empty string filter for null reference value", async () => {
    const result = await setupWithContext();
    act(() => result.current.jumpToReference("Users", null));
    expect(result.current.tabs[0].filter).toBe("");
  });
});

describe("StudioContext — CRUD operations", () => {
  it("deleteRows fires DELETE mutation and increments deleteSelectionResetKey", async () => {
    const result = await setupWithContext();
    act(() => result.current.selectTable("Users"));

    // Set up table def with PK column
    server.use(
      http.get("/efstudio/api/schema", () =>
        HttpResponse.json(defaultSchemaResponse),
      ),
    );
    await waitFor(() => expect(result.current.tables[0]).toBeDefined());

    let deleteCalled = false;
    server.use(
      http.delete("/efstudio/api/data", () => {
        deleteCalled = true;
        return HttpResponse.json({ tableKey: "Users", deletedCount: 1 });
      }),
    );

    await act(async () => {
      await result.current.deleteRows([{ Id: 1, Name: "Alice", Email: "alice@example.com" }]);
    });

    expect(deleteCalled).toBe(true);
    expect(result.current.deleteSelectionResetKey).toBe(1);
  });

  it("deleteRows sets error when table has no primary key", async () => {
    // Override schema with table that has no PK
    server.use(
      http.get("/efstudio/api/schema", () =>
        HttpResponse.json([
          {
            key: "Logs",
            name: "Logs",
            modelName: "Log",
            schema: null,
            columns: [
              {
                name: "Message",
                dataType: "nvarchar",
                isPrimaryKey: false,
                isNullable: true,
                isForeignKey: false,
                isGeneratedOnAdd: false,
                isEditableOnCreate: true,
              },
            ],
          },
        ]),
      ),
    );

    const { result } = renderHook(() => useStudioContext(), { wrapper: Wrapper });
    await waitFor(() => expect(result.current.selectedContextName).toBe("AppDbContext"));
    await waitFor(() => expect(result.current.tables).toHaveLength(1));
    act(() => result.current.selectTable("Logs"));

    await expect(
      act(async () => {
        await result.current.deleteRows([{ Message: "hello" }]);
      }),
    ).rejects.toThrow();

    await waitFor(() =>
      expect(result.current.activeTableDeleteError).toContain("no primary key"),
    );
  });

  it("submitCreateRow fires POST mutation", async () => {
    const result = await setupWithContext();
    act(() => result.current.selectTable("Users"));
    await waitFor(() => expect(result.current.selectedTable).not.toBeNull());
    act(() => result.current.openCreateDrawer());

    let postCalled = false;
    server.use(
      http.post("/efstudio/api/data", () => {
        postCalled = true;
        return HttpResponse.json({ tableKey: "Users", createdCount: 1, records: [] });
      }),
    );

    await act(async () => {
      await result.current.submitCreateRow();
    });

    expect(postCalled).toBe(true);
  });
});

describe("StudioContext — selectContext", () => {
  it("clears tabs and resets activeTabId when selecting a new context", async () => {
    const result = await setupWithContext();
    act(() => result.current.selectTable("Users"));
    expect(result.current.tabs).toHaveLength(1);

    await act(async () => {
      await result.current.selectContext("AppDbContext");
    });

    expect(result.current.tabs).toHaveLength(0);
    expect(result.current.activeTabId).toBeNull();
  });
});

describe("StudioContext — effectiveSidebarOpen", () => {
  it("is true when no tabs are open regardless of toggle", async () => {
    const result = await setupWithContext();
    expect(result.current.effectiveSidebarOpen).toBe(true);
    act(() => result.current.toggleSidebar());
    expect(result.current.effectiveSidebarOpen).toBe(true);
  });

  it("toggles when tabs are open", async () => {
    const result = await setupWithContext();
    act(() => result.current.selectTable("Users"));
    const initialOpen = result.current.effectiveSidebarOpen;
    act(() => result.current.toggleSidebar());
    expect(result.current.effectiveSidebarOpen).toBe(!initialOpen);
  });
});

// Smoke test that the provider renders children without crashing
describe("StudioContextProvider", () => {
  it("renders children", () => {
    const queryClient = makeTestQueryClient();
    render(
      <QueryClientProvider client={queryClient}>
        <StudioContextProvider>
          <div data-testid="child">hello</div>
        </StudioContextProvider>
      </QueryClientProvider>,
    );
    expect(screen.getByTestId("child")).toBeInTheDocument();
  });

  it("useStudioContext throws outside provider", () => {
    const consoleError = console.error;
    console.error = () => {};
    expect(() => renderHook(() => useStudioContext())).toThrow(
      "useStudioContext must be used within StudioContextProvider",
    );
    console.error = consoleError;
  });
});

// Integration: table data loads for open tabs
describe("StudioContext — table data", () => {
  it("fetches table data after selecting a table", async () => {
    server.use(
      http.get("/efstudio/api/data", () =>
        HttpResponse.json(defaultTableDataResponse),
      ),
    );

    const result = await setupWithContext();
    act(() => result.current.selectTable("Users"));

    await waitFor(() => expect(result.current.currentRows).toHaveLength(2));
    expect(result.current.currentTotalRows).toBe(2);
  });
});
