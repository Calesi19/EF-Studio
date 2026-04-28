import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, expect, it, vi } from "vitest";
import { DeleteConfirmDialog } from "../DeleteConfirmDialog";

describe("DeleteConfirmDialog", () => {
  it("renders singular title when count is 1", () => {
    render(
      <DeleteConfirmDialog open={true} onOpenChange={vi.fn()} onConfirm={vi.fn()} count={1} />,
    );
    expect(screen.getByText("Delete record?")).toBeInTheDocument();
  });

  it("renders bulk title when count is > 1", () => {
    render(
      <DeleteConfirmDialog open={true} onOpenChange={vi.fn()} onConfirm={vi.fn()} count={5} />,
    );
    expect(screen.getByText("Delete 5 records?")).toBeInTheDocument();
  });

  it("renders singular title when count is undefined", () => {
    render(
      <DeleteConfirmDialog open={true} onOpenChange={vi.fn()} onConfirm={vi.fn()} />,
    );
    expect(screen.getByText("Delete record?")).toBeInTheDocument();
  });

  it("calls onConfirm when confirm button is clicked", async () => {
    const user = userEvent.setup();
    const onConfirm = vi.fn();
    render(
      <DeleteConfirmDialog open={true} onOpenChange={vi.fn()} onConfirm={onConfirm} count={1} />,
    );
    await user.click(screen.getByText("Delete"));
    expect(onConfirm).toHaveBeenCalledOnce();
  });

  it("calls onConfirm with bulk label when multiple records", async () => {
    const user = userEvent.setup();
    const onConfirm = vi.fn();
    render(
      <DeleteConfirmDialog open={true} onOpenChange={vi.fn()} onConfirm={onConfirm} count={3} />,
    );
    await user.click(screen.getByText("Delete 3"));
    expect(onConfirm).toHaveBeenCalledOnce();
  });

  it("calls onOpenChange(false) when Cancel is clicked", async () => {
    const user = userEvent.setup();
    const onOpenChange = vi.fn();
    render(
      <DeleteConfirmDialog open={true} onOpenChange={onOpenChange} onConfirm={vi.fn()} count={1} />,
    );
    await user.click(screen.getByText("Cancel"));
    expect(onOpenChange).toHaveBeenCalledWith(false);
  });

  it("does not render content when closed", () => {
    render(
      <DeleteConfirmDialog open={false} onOpenChange={vi.fn()} onConfirm={vi.fn()} count={1} />,
    );
    expect(screen.queryByText("Delete record?")).not.toBeInTheDocument();
  });
});
