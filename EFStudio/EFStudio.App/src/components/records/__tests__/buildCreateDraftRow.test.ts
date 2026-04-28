import { describe, expect, it } from "vitest";
import { buildCreateDraftRow } from "../CreateRecordDrawer";
import type { ColumnDef, TableDef } from "@/types";

function makeColumn(overrides: Partial<ColumnDef> & { name: string }): ColumnDef {
  return {
    type: "string",
    isPrimaryKey: false,
    isForeignKey: false,
    isNullable: true,
    isGeneratedOnAdd: false,
    isEditableOnCreate: true,
    ...overrides,
  };
}

function makeTable(columns: ColumnDef[]): TableDef {
  return {
    key: "TestTable",
    name: "TestTable",
    displayName: "Test Table",
    modelDisplayName: "Test Table",
    columns,
    rows: [],
  };
}

describe("buildCreateDraftRow", () => {
  it("initializes number column to 0", () => {
    const table = makeTable([makeColumn({ name: "Score", type: "number" })]);
    expect(buildCreateDraftRow(table)).toEqual({ Score: 0 });
  });

  it("initializes boolean column to false", () => {
    const table = makeTable([makeColumn({ name: "IsActive", type: "boolean" })]);
    expect(buildCreateDraftRow(table)).toEqual({ IsActive: false });
  });

  it("initializes nullable string column to null", () => {
    const table = makeTable([makeColumn({ name: "Name", type: "string", isNullable: true })]);
    expect(buildCreateDraftRow(table)).toEqual({ Name: null });
  });

  it("initializes non-nullable string column to empty string", () => {
    const table = makeTable([makeColumn({ name: "Name", type: "string", isNullable: false })]);
    expect(buildCreateDraftRow(table)).toEqual({ Name: "" });
  });

  it("initializes uuid PK column to a UUID string", () => {
    const table = makeTable([
      makeColumn({ name: "Id", type: "uuid", isPrimaryKey: true }),
    ]);
    const row = buildCreateDraftRow(table);
    expect(typeof row.Id).toBe("string");
    expect(row.Id).toMatch(/^[0-9a-f]{8}-[0-9a-f]{4}-/i);
  });

  it("excludes columns not editable on create", () => {
    const table = makeTable([
      makeColumn({ name: "Id", type: "number", isEditableOnCreate: false }),
      makeColumn({ name: "Name", type: "string", isEditableOnCreate: true }),
    ]);
    const row = buildCreateDraftRow(table);
    expect("Id" in row).toBe(false);
    expect("Name" in row).toBe(true);
  });

  it("handles table with no editable columns", () => {
    const table = makeTable([
      makeColumn({ name: "Id", type: "number", isEditableOnCreate: false }),
    ]);
    expect(buildCreateDraftRow(table)).toEqual({});
  });
});
