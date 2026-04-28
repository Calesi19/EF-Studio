import { http, HttpResponse } from "msw";
import { describe, expect, it } from "vitest";
import { createRecords } from "../createRecords";
import { updateRecords } from "../updateRecords";
import { deleteRecords } from "../deleteRecords";
import { server } from "@/test/setup";

describe("createRecords", () => {
  it("posts the correct payload and returns response", async () => {
    let capturedBody: unknown;
    server.use(
      http.post("/efstudio/api/data", async ({ request }) => {
        capturedBody = await request.json();
        return HttpResponse.json({ tableKey: "Users", createdCount: 1, records: [{ Id: 3 }] });
      }),
    );

    const result = await createRecords({
      tableKey: "Users",
      records: [{ Name: "Charlie", Email: null }],
    });

    expect(result.createdCount).toBe(1);
    expect(capturedBody).toEqual({
      tableKey: "Users",
      records: [{ Name: "Charlie", Email: null }],
    });
  });

  it("extracts message from JSON error response", async () => {
    server.use(
      http.post("/efstudio/api/data", () =>
        HttpResponse.json({ message: "Validation failed" }, { status: 422 }),
      ),
    );
    await expect(createRecords({ tableKey: "T", records: [{}] })).rejects.toThrow(
      "Validation failed",
    );
  });

  it("falls back to generic error message for non-JSON error response", async () => {
    server.use(
      http.post("/efstudio/api/data", () => new HttpResponse("Internal Error", { status: 500 })),
    );
    await expect(createRecords({ tableKey: "T", records: [{}] })).rejects.toThrow(
      "Failed to create records (500)",
    );
  });
});

describe("updateRecords", () => {
  it("puts the correct payload and returns response", async () => {
    let capturedBody: unknown;
    server.use(
      http.put("/efstudio/api/data", async ({ request }) => {
        capturedBody = await request.json();
        return HttpResponse.json({ tableKey: "Users", updatedCount: 1 });
      }),
    );

    const result = await updateRecords({
      tableKey: "Users",
      updates: [{ keys: { Id: 1 }, values: { Name: "Updated" } }],
    });

    expect(result.updatedCount).toBe(1);
    expect(capturedBody).toEqual({
      tableKey: "Users",
      updates: [{ keys: { Id: 1 }, values: { Name: "Updated" } }],
    });
  });

  it("throws with message from error response", async () => {
    server.use(
      http.put("/efstudio/api/data", () =>
        HttpResponse.json({ message: "Row not found" }, { status: 404 }),
      ),
    );
    await expect(
      updateRecords({ tableKey: "T", updates: [{ keys: { Id: 99 }, values: { Name: "X" } }] }),
    ).rejects.toThrow("Row not found");
  });
});

describe("deleteRecords", () => {
  it("sends delete request with correct payload", async () => {
    let capturedBody: unknown;
    server.use(
      http.delete("/efstudio/api/data", async ({ request }) => {
        capturedBody = await request.json();
        return HttpResponse.json({ tableKey: "Users", deletedCount: 2 });
      }),
    );

    const result = await deleteRecords({
      tableKey: "Users",
      keys: [{ Id: 1 }, { Id: 2 }],
    });

    expect(result.deletedCount).toBe(2);
    expect(capturedBody).toEqual({
      tableKey: "Users",
      keys: [{ Id: 1 }, { Id: 2 }],
    });
  });

  it("throws with message from error response", async () => {
    server.use(
      http.delete("/efstudio/api/data", () =>
        HttpResponse.json({ message: "FK constraint violated" }, { status: 409 }),
      ),
    );
    await expect(deleteRecords({ tableKey: "T", keys: [{ Id: 1 }] })).rejects.toThrow(
      "FK constraint violated",
    );
  });
});
