import { renderHook, waitFor } from "@testing-library/react";
import { http, HttpResponse } from "msw";
import { describe, expect, it } from "vitest";
import { fetchDbContexts, useDbContexts } from "../fetchDbContexts";
import { server } from "@/test/setup";
import { makeTestQueryClient } from "@/test/utils";
import { QueryClientProvider } from "@tanstack/react-query";

function wrapper({ children }: { children: React.ReactNode }) {
  return <QueryClientProvider client={makeTestQueryClient()}>{children}</QueryClientProvider>;
}

describe("fetchDbContexts", () => {
  it("returns normalized DbContextDef array", async () => {
    const contexts = await fetchDbContexts();
    expect(contexts).toHaveLength(1);
    expect(contexts[0]).toMatchObject({
      name: "AppDbContext",
      displayName: "App Db Context",
      isSelected: true,
      isDefault: true,
      isAvailable: true,
    });
    expect(contexts[0].activationError).toBeUndefined();
  });

  it("converts null activationError to undefined", async () => {
    const contexts = await fetchDbContexts();
    expect(contexts[0].activationError).toBeUndefined();
  });

  it("throws on non-ok response", async () => {
    server.use(
      http.get("/efstudio/api/contexts", () => HttpResponse.json({}, { status: 500 })),
    );
    await expect(fetchDbContexts()).rejects.toThrow("Failed to load DbContexts (500)");
  });
});

describe("useDbContexts", () => {
  it("fetches contexts on mount", async () => {
    const { result } = renderHook(() => useDbContexts(), { wrapper });
    await waitFor(() => expect(result.current.isSuccess).toBe(true));
    expect(result.current.data).toHaveLength(1);
    expect(result.current.data![0].name).toBe("AppDbContext");
  });

  it("does not fetch when enabled is false", () => {
    const { result } = renderHook(() => useDbContexts(false), { wrapper });
    expect(result.current.fetchStatus).toBe("idle");
    expect(result.current.data).toBeUndefined();
  });
});
