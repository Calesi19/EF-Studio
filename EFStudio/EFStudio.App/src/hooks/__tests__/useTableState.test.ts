import { renderHook } from "@testing-library/react";
import { describe, expect, it } from "vitest";
import { useTableState } from "../useTableState";
import type { ColumnDef, PaginationState, RecordRow, SortState } from "@/types";

const NO_SORT: SortState = { column: null, direction: "asc" };
const DEFAULT_PAGINATION: PaginationState = { page: 1, pageSize: 10 };
const NO_COLUMNS: ColumnDef[] = [];

const ROWS: RecordRow[] = [
  { Id: 1, Name: "Alice", Score: 30 },
  { Id: 2, Name: "Bob", Score: 10 },
  { Id: 3, Name: "Charlie", Score: 20 },
];

describe("useTableState", () => {
  describe("filtering", () => {
    it("returns all rows when filter is empty", () => {
      const { result } = renderHook(() =>
        useTableState(ROWS, NO_COLUMNS, "", NO_SORT, DEFAULT_PAGINATION),
      );
      expect(result.current.totalRows).toBe(3);
      expect(result.current.paginatedRows).toHaveLength(3);
    });

    it("filters rows by text match (case-insensitive)", () => {
      const { result } = renderHook(() =>
        useTableState(ROWS, NO_COLUMNS, "alice", NO_SORT, DEFAULT_PAGINATION),
      );
      expect(result.current.totalRows).toBe(1);
      expect(result.current.paginatedRows[0].Name).toBe("Alice");
    });

    it("matches across any column value", () => {
      const { result } = renderHook(() =>
        useTableState(ROWS, NO_COLUMNS, "20", NO_SORT, DEFAULT_PAGINATION),
      );
      expect(result.current.totalRows).toBe(1);
      expect(result.current.paginatedRows[0].Name).toBe("Charlie");
    });

    it("skips null values without throwing", () => {
      const rowsWithNull: RecordRow[] = [{ Id: 1, Name: null }];
      const { result } = renderHook(() =>
        useTableState(rowsWithNull, NO_COLUMNS, "alice", NO_SORT, DEFAULT_PAGINATION),
      );
      expect(result.current.totalRows).toBe(0);
    });

    it("returns no rows when filter matches nothing", () => {
      const { result } = renderHook(() =>
        useTableState(ROWS, NO_COLUMNS, "zzz", NO_SORT, DEFAULT_PAGINATION),
      );
      expect(result.current.totalRows).toBe(0);
      expect(result.current.paginatedRows).toHaveLength(0);
    });
  });

  describe("sorting", () => {
    it("preserves original order when no sort column set", () => {
      const { result } = renderHook(() =>
        useTableState(ROWS, NO_COLUMNS, "", NO_SORT, DEFAULT_PAGINATION),
      );
      expect(result.current.paginatedRows.map((r) => r.Name)).toEqual(["Alice", "Bob", "Charlie"]);
    });

    it("sorts ascending by string column", () => {
      const sort: SortState = { column: "Name", direction: "asc" };
      const { result } = renderHook(() =>
        useTableState(ROWS, NO_COLUMNS, "", sort, DEFAULT_PAGINATION),
      );
      expect(result.current.paginatedRows.map((r) => r.Name)).toEqual(["Alice", "Bob", "Charlie"]);
    });

    it("sorts descending by string column", () => {
      const sort: SortState = { column: "Name", direction: "desc" };
      const { result } = renderHook(() =>
        useTableState(ROWS, NO_COLUMNS, "", sort, DEFAULT_PAGINATION),
      );
      expect(result.current.paginatedRows.map((r) => r.Name)).toEqual(["Charlie", "Bob", "Alice"]);
    });

    it("sorts numerically by numeric column ascending", () => {
      const sort: SortState = { column: "Score", direction: "asc" };
      const { result } = renderHook(() =>
        useTableState(ROWS, NO_COLUMNS, "", sort, DEFAULT_PAGINATION),
      );
      expect(result.current.paginatedRows.map((r) => r.Score)).toEqual([10, 20, 30]);
    });

    it("sorts numerically by numeric column descending", () => {
      const sort: SortState = { column: "Score", direction: "desc" };
      const { result } = renderHook(() =>
        useTableState(ROWS, NO_COLUMNS, "", sort, DEFAULT_PAGINATION),
      );
      expect(result.current.paginatedRows.map((r) => r.Score)).toEqual([30, 20, 10]);
    });

    it("places null values last when sorting ascending", () => {
      const rowsWithNull: RecordRow[] = [
        { Id: 1, Name: null },
        { Id: 2, Name: "Alpha" },
      ];
      const sort: SortState = { column: "Name", direction: "asc" };
      const { result } = renderHook(() =>
        useTableState(rowsWithNull, NO_COLUMNS, "", sort, DEFAULT_PAGINATION),
      );
      expect(result.current.paginatedRows[0].Name).toBe("Alpha");
      expect(result.current.paginatedRows[1].Name).toBeNull();
    });
  });

  describe("pagination", () => {
    it("paginates to page 1", () => {
      const pagination: PaginationState = { page: 1, pageSize: 2 };
      const { result } = renderHook(() =>
        useTableState(ROWS, NO_COLUMNS, "", NO_SORT, pagination),
      );
      expect(result.current.paginatedRows).toHaveLength(2);
      expect(result.current.paginatedRows[0].Name).toBe("Alice");
    });

    it("paginates to page 2", () => {
      const pagination: PaginationState = { page: 2, pageSize: 2 };
      const { result } = renderHook(() =>
        useTableState(ROWS, NO_COLUMNS, "", NO_SORT, pagination),
      );
      expect(result.current.paginatedRows).toHaveLength(1);
      expect(result.current.paginatedRows[0].Name).toBe("Charlie");
    });

    it("returns empty array for page beyond last page", () => {
      const pagination: PaginationState = { page: 10, pageSize: 10 };
      const { result } = renderHook(() =>
        useTableState(ROWS, NO_COLUMNS, "", NO_SORT, pagination),
      );
      expect(result.current.paginatedRows).toHaveLength(0);
    });

    it("computes totalPages correctly", () => {
      const pagination: PaginationState = { page: 1, pageSize: 2 };
      const { result } = renderHook(() =>
        useTableState(ROWS, NO_COLUMNS, "", NO_SORT, pagination),
      );
      expect(result.current.totalPages).toBe(2);
    });

    it("totalPages is at least 1 for empty rows", () => {
      const { result } = renderHook(() =>
        useTableState([], NO_COLUMNS, "", NO_SORT, DEFAULT_PAGINATION),
      );
      expect(result.current.totalPages).toBe(1);
      expect(result.current.totalRows).toBe(0);
    });
  });
});
