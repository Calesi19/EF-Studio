import type { PaginationState, SortState } from "@/types";

export const DEFAULT_SORT: SortState = { column: null, direction: "asc" };
export const DEFAULT_PAGE_NUMBER = 1;
export const DEFAULT_PAGE_SIZE = 10;
export const DEFAULT_PAGINATION: PaginationState = {
  page: DEFAULT_PAGE_NUMBER,
  pageSize: DEFAULT_PAGE_SIZE,
};
