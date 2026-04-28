import { act, renderHook } from "@testing-library/react";
import { afterEach, describe, expect, it } from "vitest";
import { useSettings } from "../useSettings";

const STORAGE_KEY = "ef-studio-name-display";

afterEach(() => {
  localStorage.clear();
});

describe("useSettings", () => {
  it("defaults to 'model' when localStorage has no value", () => {
    const { result } = renderHook(() => useSettings());
    expect(result.current.nameDisplay).toBe("model");
  });

  it("reads 'model' from localStorage", () => {
    localStorage.setItem(STORAGE_KEY, "model");
    const { result } = renderHook(() => useSettings());
    expect(result.current.nameDisplay).toBe("model");
  });

  it("reads 'database' from localStorage", () => {
    localStorage.setItem(STORAGE_KEY, "database");
    const { result } = renderHook(() => useSettings());
    expect(result.current.nameDisplay).toBe("database");
  });

  it("defaults to 'model' for invalid localStorage value", () => {
    localStorage.setItem(STORAGE_KEY, "invalid");
    const { result } = renderHook(() => useSettings());
    expect(result.current.nameDisplay).toBe("model");
  });

  it("persists new value to localStorage when changed", () => {
    const { result } = renderHook(() => useSettings());
    act(() => {
      result.current.setNameDisplay("database");
    });
    expect(result.current.nameDisplay).toBe("database");
    expect(localStorage.getItem(STORAGE_KEY)).toBe("database");
  });

  it("toggles back to 'model' from 'database'", () => {
    localStorage.setItem(STORAGE_KEY, "database");
    const { result } = renderHook(() => useSettings());
    act(() => {
      result.current.setNameDisplay("model");
    });
    expect(result.current.nameDisplay).toBe("model");
    expect(localStorage.getItem(STORAGE_KEY)).toBe("model");
  });
});
