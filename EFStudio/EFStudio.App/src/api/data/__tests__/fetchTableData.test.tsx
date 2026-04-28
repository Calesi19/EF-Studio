import { renderHook, waitFor } from "@testing-library/react";
import { http, HttpResponse } from "msw";
import { describe, expect, it } from "vitest";
import { fetchTableData, useTableData } from "../fetchTableData";
import { server } from "@/test/setup";
import { makeTestQueryClient } from "@/test/utils";
import { QueryClientProvider } from "@tanstack/react-query";
import type { PaginationState, SortState } from "@/types";

const NO_SORT: SortState = { column: null, direction: "asc" };
const DEFAULT_PAGINATION: PaginationState = { page: 1, pageSize: 10 };

function wrapper({ children }: { children: React.ReactNode }) {
  return <QueryClientProvider client={makeTestQueryClient()}>{children}</QueryClientProvider>;
}

describe("fetchTableData", () => {
  it("returns normalized rows and pagination metadata", async () => {
    const result = await fetchTableData("AppDbContext", "Users", DEFAULT_PAGINATION, "", NO_SORT);
    expect(result.rows).toHaveLength(2);
    expect(result.totalRows).toBe(2);
    expect(result.page).toBe(1);
    expect(result.pageSize).toBe(10);
    expect(result.totalPages).toBe(1);
  });

  it("normalizes null cell values to null", async () => {
    const result = await fetchTableData("AppDbContext", "Users", DEFAULT_PAGINATION, "", NO_SORT);
    const bob = result.rows.find((r) => r.Name === "Bob");
    expect(bob!.Email).toBeNull();
  });

  it("preserves primitive cell values as-is", async () => {
    server.use(
      http.get("/efstudio/api/data", () =>
        HttpResponse.json({
          key: "T",
          name: "T",
          page: 1,
          pageSize: 10,
          totalRows: 1,
          rows: [{ Id: 42, Name: "Test", Active: true }],
        }),
      ),
    );
    const result = await fetchTableData("AppDbContext", "T", DEFAULT_PAGINATION, "", NO_SORT);
    expect(result.rows[0].Id).toBe(42);
    expect(result.rows[0].Name).toBe("Test");
    expect(result.rows[0].Active).toBe(true);
  });

  it("stringifies object cell values", async () => {
    server.use(
      http.get("/efstudio/api/data", () =>
        HttpResponse.json({
          key: "T",
          name: "T",
          page: 1,
          pageSize: 10,
          totalRows: 1,
          rows: [{ Meta: { key: "value" } }],
        }),
      ),
    );
    const result = await fetchTableData("AppDbContext", "T", DEFAULT_PAGINATION, "", NO_SORT);
    expect(result.rows[0].Meta).toBe('{"key":"value"}');
  });

  it("includes filter param when filter is non-empty", async () => {
    let capturedUrl = "";
    server.use(
      http.get("/efstudio/api/data", ({ request }) => {
        capturedUrl = request.url;
        return HttpResponse.json({
          key: "Users",
          name: "Users",
          page: 1,
          pageSize: 10,
          totalRows: 0,
          rows: [],
        });
      }),
    );
    await fetchTableData("AppDbContext", "Users", DEFAULT_PAGINATION, "alice", NO_SORT);
    expect(capturedUrl).toContain("filter=alice");
  });

  it("omits filter param when filter is empty", async () => {
    let capturedUrl = "";
    server.use(
      http.get("/efstudio/api/data", ({ request }) => {
        capturedUrl = request.url;
        return HttpResponse.json({
          key: "Users",
          name: "Users",
          page: 1,
          pageSize: 10,
          totalRows: 0,
          rows: [],
        });
      }),
    );
    await fetchTableData("AppDbContext", "Users", DEFAULT_PAGINATION, "", NO_SORT);
    expect(capturedUrl).not.toContain("filter=");
  });

  it("includes sortColumn and sortDirection when sort column is set", async () => {
    let capturedUrl = "";
    server.use(
      http.get("/efstudio/api/data", ({ request }) => {
        capturedUrl = request.url;
        return HttpResponse.json({
          key: "Users",
          name: "Users",
          page: 1,
          pageSize: 10,
          totalRows: 0,
          rows: [],
        });
      }),
    );
    await fetchTableData("AppDbContext", "Users", DEFAULT_PAGINATION, "", {
      column: "Name",
      direction: "desc",
    });
    expect(capturedUrl).toContain("sortColumn=Name");
    expect(capturedUrl).toContain("sortDirection=desc");
  });

  it("computes totalPages correctly for multiple pages", async () => {
    server.use(
      http.get("/efstudio/api/data", () =>
        HttpResponse.json({
          key: "T",
          name: "T",
          page: 1,
          pageSize: 5,
          totalRows: 11,
          rows: [],
        }),
      ),
    );
    const result = await fetchTableData("AppDbContext", "T", { page: 1, pageSize: 5 }, "", NO_SORT);
    expect(result.totalPages).toBe(3);
  });

  it("throws on non-ok response", async () => {
    server.use(
      http.get("/efstudio/api/data", () => HttpResponse.json({}, { status: 404 })),
    );
    await expect(
      fetchTableData("AppDbContext", "Missing", DEFAULT_PAGINATION, "", NO_SORT),
    ).rejects.toThrow("Failed to load data for Missing (404)");
  });
});

describe("useTableData", () => {
  it("is disabled when tableKey is empty", () => {
    const { result } = renderHook(
      () => useTableData("AppDbContext", "", DEFAULT_PAGINATION, "", NO_SORT),
      { wrapper },
    );
    expect(result.current.fetchStatus).toBe("idle");
  });

  it("is disabled when contextName is null", () => {
    const { result } = renderHook(
      () => useTableData(null, "Users", DEFAULT_PAGINATION, "", NO_SORT),
      { wrapper },
    );
    expect(result.current.fetchStatus).toBe("idle");
  });

  it("fetches data when contextName and tableKey are set", async () => {
    const { result } = renderHook(
      () => useTableData("AppDbContext", "Users", DEFAULT_PAGINATION, "", NO_SORT),
      { wrapper },
    );
    await waitFor(() => expect(result.current.isSuccess).toBe(true));
    expect(result.current.data!.rows).toHaveLength(2);
  });
});
