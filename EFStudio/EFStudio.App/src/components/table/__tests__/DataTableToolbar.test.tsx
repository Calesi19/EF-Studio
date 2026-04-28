import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, expect, it, vi } from "vitest";
import { DataTableToolbar } from "../DataTableToolbar";

function renderToolbar(
  overrides: Partial<Parameters<typeof DataTableToolbar>[0]> = {},
) {
  const props = {
    filter: "",
    onFilterChange: vi.fn(),
    onAddRecord: vi.fn(),
    selectedCount: 0,
    onBulkDelete: vi.fn(),
    ...overrides,
  };
  render(<DataTableToolbar {...props} />);
  return props;
}

describe("DataTableToolbar", () => {
  it("renders filter input when nothing is selected and no pending edits", () => {
    renderToolbar();
    expect(screen.getByPlaceholderText("Filter records...")).toBeInTheDocument();
  });

  it("calls onFilterChange when typing in filter input", async () => {
    const user = userEvent.setup();
    const { onFilterChange } = renderToolbar();
    const input = screen.getByPlaceholderText("Filter records...");
    await user.type(input, "alice");
    expect(onFilterChange).toHaveBeenCalled();
  });

  it("shows clear button when filter has a value", () => {
    renderToolbar({ filter: "hello" });
    expect(screen.getByLabelText("Clear filter")).toBeInTheDocument();
  });

  it("clears filter when clear button is clicked", async () => {
    const user = userEvent.setup();
    const { onFilterChange } = renderToolbar({ filter: "hello" });
    await user.click(screen.getByLabelText("Clear filter"));
    expect(onFilterChange).toHaveBeenCalledWith("");
  });

  it("shows Add record button by default", () => {
    renderToolbar();
    expect(screen.getByText("Add record")).toBeInTheDocument();
  });

  it("calls onAddRecord when Add record is clicked", async () => {
    const user = userEvent.setup();
    const { onAddRecord } = renderToolbar();
    await user.click(screen.getByText("Add record"));
    expect(onAddRecord).toHaveBeenCalledOnce();
  });

  it("hides Add record when canAddRecord is false", () => {
    renderToolbar({ canAddRecord: false });
    expect(screen.queryByText("Add record")).not.toBeInTheDocument();
  });

  it("shows selection count and delete button when rows are selected", () => {
    renderToolbar({ selectedCount: 3 });
    expect(screen.getByText("3 rows selected")).toBeInTheDocument();
    expect(screen.getByText("Delete 3")).toBeInTheDocument();
  });

  it("calls onBulkDelete when Delete N is clicked", async () => {
    const user = userEvent.setup();
    const { onBulkDelete } = renderToolbar({ selectedCount: 2 });
    await user.click(screen.getByText("Delete 2"));
    expect(onBulkDelete).toHaveBeenCalledOnce();
  });

  it("shows pending edits info when pendingEditCount > 0", () => {
    renderToolbar({ pendingEditCount: 2 });
    expect(screen.getByText(/2 rows with unsaved changes/)).toBeInTheDocument();
    expect(screen.getByText("Save")).toBeInTheDocument();
    expect(screen.getByText("Discard")).toBeInTheDocument();
  });

  it("calls onSaveEdits when Save is clicked", async () => {
    const user = userEvent.setup();
    const onSaveEdits = vi.fn();
    renderToolbar({ pendingEditCount: 1, onSaveEdits });
    await user.click(screen.getByText("Save"));
    expect(onSaveEdits).toHaveBeenCalledOnce();
  });

  it("calls onDiscardEdits when Discard is clicked", async () => {
    const user = userEvent.setup();
    const onDiscardEdits = vi.fn();
    renderToolbar({ pendingEditCount: 1, onDiscardEdits });
    await user.click(screen.getByText("Discard"));
    expect(onDiscardEdits).toHaveBeenCalledOnce();
  });

  it("hides all controls in readOnly mode", () => {
    renderToolbar({ readOnly: true });
    expect(screen.queryByText("Add record")).not.toBeInTheDocument();
    expect(screen.queryByText("Delete")).not.toBeInTheDocument();
    expect(screen.queryByText("Save")).not.toBeInTheDocument();
  });
});
