import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";
import { ColumnTypeBadge } from "../ColumnTypeBadge";
import type { ColumnDef } from "@/types";

function makeColumn(overrides: Partial<ColumnDef> & { name: string }): ColumnDef {
  return {
    type: "string",
    isPrimaryKey: false,
    isForeignKey: false,
    isNullable: false,
    isGeneratedOnAdd: false,
    isEditableOnCreate: true,
    ...overrides,
  };
}

describe("ColumnTypeBadge", () => {
  it("shows PK badge for primary key column", () => {
    render(<ColumnTypeBadge column={makeColumn({ name: "Id", isPrimaryKey: true })} />);
    expect(screen.getByText("PK")).toBeInTheDocument();
  });

  it("shows FK badge for foreign key column", () => {
    render(<ColumnTypeBadge column={makeColumn({ name: "UserId", isForeignKey: true })} />);
    expect(screen.getByText("FK")).toBeInTheDocument();
  });

  it("shows nullable '?' badge when column is nullable and not PK/FK", () => {
    render(<ColumnTypeBadge column={makeColumn({ name: "Email", isNullable: true })} />);
    expect(screen.getByText("?")).toBeInTheDocument();
  });

  it("does not show '?' badge when column is not nullable", () => {
    render(<ColumnTypeBadge column={makeColumn({ name: "Name", isNullable: false })} />);
    expect(screen.queryByText("?")).not.toBeInTheDocument();
  });

  it("does not show PK or FK badge for plain column", () => {
    render(<ColumnTypeBadge column={makeColumn({ name: "Name" })} />);
    expect(screen.queryByText("PK")).not.toBeInTheDocument();
    expect(screen.queryByText("FK")).not.toBeInTheDocument();
  });
});
