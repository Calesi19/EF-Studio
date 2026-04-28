import { renderHook, waitFor } from "@testing-library/react";
import { http, HttpResponse } from "msw";
import { describe, expect, it } from "vitest";
import { fetchSchema, useSchema } from "../fetchSchema";
import { server } from "@/test/setup";
import { makeTestQueryClient } from "@/test/utils";
import { QueryClientProvider } from "@tanstack/react-query";

function wrapper({ children }: { children: React.ReactNode }) {
  return <QueryClientProvider client={makeTestQueryClient()}>{children}</QueryClientProvider>;
}

describe("fetchSchema — column type mapping", () => {
  const typeMapping: [string, string][] = [
    ["int", "number"],
    ["bigint", "number"],
    ["smallint", "number"],
    ["real", "number"],
    ["float", "number"],
    ["double", "number"],
    ["decimal", "number"],
    ["numeric", "number"],
    ["boolean", "boolean"],
    ["bool", "boolean"],
    ["date", "datetime"],
    ["datetime", "datetime"],
    ["timestamp", "datetime"],
    ["time", "datetime"],
    ["uuid", "uuid"],
    ["guid", "uuid"],
    ["uniqueidentifier", "uuid"],
    ["json", "json"],
    ["jsonb", "json"],
    ["nvarchar", "string"],
    ["text", "string"],
    ["character varying", "string"],
    ["bytea", "string"],
  ];

  it.each(typeMapping)("maps '%s' → '%s'", async (dataType, expectedType) => {
    server.use(
      http.get("/efstudio/api/schema", () =>
        HttpResponse.json([
          {
            key: "T",
            name: "T",
            modelName: "T",
            schema: null,
            columns: [
              {
                name: "Col",
                dataType,
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

    const tables = await fetchSchema("AppDbContext");
    expect(tables[0].columns[0].type).toBe(expectedType);
  });
});

describe("fetchSchema — display names", () => {
  it("generates displayName from table name with PascalCase splitting", async () => {
    server.use(
      http.get("/efstudio/api/schema", () =>
        HttpResponse.json([
          {
            key: "UserProfiles",
            name: "UserProfiles",
            modelName: "UserProfile",
            schema: null,
            columns: [],
          },
        ]),
      ),
    );
    const tables = await fetchSchema("AppDbContext");
    expect(tables[0].displayName).toBe("User Profiles");
    expect(tables[0].modelDisplayName).toBe("User Profile");
  });
});

describe("fetchSchema — columns normalization", () => {
  it("normalizes FK column with foreignKeyTable", async () => {
    server.use(
      http.get("/efstudio/api/schema", () =>
        HttpResponse.json([
          {
            key: "Posts",
            name: "Posts",
            modelName: "Post",
            schema: null,
            columns: [
              {
                name: "UserId",
                dataType: "int",
                isPrimaryKey: false,
                isNullable: false,
                isForeignKey: true,
                isGeneratedOnAdd: false,
                isEditableOnCreate: true,
                foreignKeyTable: "Users",
              },
            ],
          },
        ]),
      ),
    );
    const tables = await fetchSchema("AppDbContext");
    const col = tables[0].columns[0];
    expect(col.isForeignKey).toBe(true);
    expect(col.foreignKeyTable).toBe("Users");
  });

  it("converts null foreignKeyTable to undefined", async () => {
    server.use(
      http.get("/efstudio/api/schema", () =>
        HttpResponse.json([
          {
            key: "Users",
            name: "Users",
            modelName: "User",
            schema: null,
            columns: [
              {
                name: "Id",
                dataType: "int",
                isPrimaryKey: true,
                isNullable: false,
                isForeignKey: false,
                isGeneratedOnAdd: true,
                isEditableOnCreate: false,
                foreignKeyTable: null,
              },
            ],
          },
        ]),
      ),
    );
    const tables = await fetchSchema("AppDbContext");
    expect(tables[0].columns[0].foreignKeyTable).toBeUndefined();
  });
});

describe("fetchSchema — error handling", () => {
  it("throws on non-ok response", async () => {
    server.use(
      http.get("/efstudio/api/schema", () => HttpResponse.json({}, { status: 404 })),
    );
    await expect(fetchSchema("Missing")).rejects.toThrow("Failed to load schema (404)");
  });
});

describe("useSchema", () => {
  it("is disabled when contextName is null", () => {
    const { result } = renderHook(() => useSchema(null), { wrapper });
    expect(result.current.fetchStatus).toBe("idle");
  });

  it("is disabled when contextName is empty string", () => {
    const { result } = renderHook(() => useSchema(""), { wrapper });
    expect(result.current.fetchStatus).toBe("idle");
  });

  it("fetches schema when contextName is set", async () => {
    const { result } = renderHook(() => useSchema("AppDbContext"), { wrapper });
    await waitFor(() => expect(result.current.isSuccess).toBe(true));
    expect(result.current.data).toHaveLength(1);
    expect(result.current.data![0].key).toBe("Users");
  });
});
