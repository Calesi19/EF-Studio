import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, expect, it, vi } from "vitest";
import { DataTablePagination } from "../DataTablePagination";
import type { PaginationState } from "@/types";

function renderPagination(
  overrides: Partial<{
    pagination: PaginationState;
    totalRows: number;
    totalPages: number;
    onPageChange: (page: number) => void;
    onPageSizeChange: (size: number) => void;
  }> = {},
) {
  const props = {
    pagination: { page: 1, pageSize: 10 },
    totalRows: 25,
    totalPages: 3,
    onPageChange: vi.fn(),
    onPageSizeChange: vi.fn(),
    ...overrides,
  };
  render(<DataTablePagination {...props} />);
  return props;
}

describe("DataTablePagination", () => {
  it("shows 'No records' when totalRows is 0", () => {
    renderPagination({ totalRows: 0, totalPages: 1 });
    expect(screen.getByText("No records")).toBeInTheDocument();
  });

  it("shows row range for non-empty result", () => {
    renderPagination({ pagination: { page: 1, pageSize: 10 }, totalRows: 25, totalPages: 3 });
    expect(screen.getByText(/1–10 of 25 records/)).toBeInTheDocument();
  });

  it("shows correct range for page 2", () => {
    renderPagination({ pagination: { page: 2, pageSize: 10 }, totalRows: 25, totalPages: 3 });
    expect(screen.getByText(/11–20 of 25 records/)).toBeInTheDocument();
  });

  it("prev buttons are disabled on page 1", () => {
    renderPagination({ pagination: { page: 1, pageSize: 10 }, totalRows: 25, totalPages: 3 });
    const buttons = screen.getAllByRole("button");
    const prevButtons = buttons.filter((b) => ["«", "‹"].includes(b.textContent ?? ""));
    prevButtons.forEach((b) => expect(b).toBeDisabled());
  });

  it("next buttons are disabled on last page", () => {
    renderPagination({ pagination: { page: 3, pageSize: 10 }, totalRows: 25, totalPages: 3 });
    const buttons = screen.getAllByRole("button");
    const nextButtons = buttons.filter((b) => ["›", "»"].includes(b.textContent ?? ""));
    nextButtons.forEach((b) => expect(b).toBeDisabled());
  });

  it("prev buttons are enabled when not on page 1", () => {
    renderPagination({ pagination: { page: 2, pageSize: 10 }, totalRows: 25, totalPages: 3 });
    const buttons = screen.getAllByRole("button");
    const prevButtons = buttons.filter((b) => ["«", "‹"].includes(b.textContent ?? ""));
    prevButtons.forEach((b) => expect(b).not.toBeDisabled());
  });

  it("calls onPageChange with next page when clicking ›", async () => {
    const user = userEvent.setup();
    const { onPageChange } = renderPagination({
      pagination: { page: 1, pageSize: 10 },
      totalRows: 25,
      totalPages: 3,
    });
    const nextBtn = screen.getAllByRole("button").find((b) => b.textContent === "›")!;
    await user.click(nextBtn);
    expect(onPageChange).toHaveBeenCalledWith(2);
  });

  it("calls onPageChange with prev page when clicking ‹", async () => {
    const user = userEvent.setup();
    const { onPageChange } = renderPagination({
      pagination: { page: 2, pageSize: 10 },
      totalRows: 25,
      totalPages: 3,
    });
    const prevBtn = screen.getAllByRole("button").find((b) => b.textContent === "‹")!;
    await user.click(prevBtn);
    expect(onPageChange).toHaveBeenCalledWith(1);
  });

  it("calls onPageChange with 1 when clicking «", async () => {
    const user = userEvent.setup();
    const { onPageChange } = renderPagination({
      pagination: { page: 3, pageSize: 10 },
      totalRows: 25,
      totalPages: 3,
    });
    const firstBtn = screen.getAllByRole("button").find((b) => b.textContent === "«")!;
    await user.click(firstBtn);
    expect(onPageChange).toHaveBeenCalledWith(1);
  });

  it("calls onPageChange with last page when clicking »", async () => {
    const user = userEvent.setup();
    const { onPageChange } = renderPagination({
      pagination: { page: 1, pageSize: 10 },
      totalRows: 25,
      totalPages: 3,
    });
    const lastBtn = screen.getAllByRole("button").find((b) => b.textContent === "»")!;
    await user.click(lastBtn);
    expect(onPageChange).toHaveBeenCalledWith(3);
  });
});
