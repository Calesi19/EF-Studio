import { queryOptions, useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { DEFAULT_QUERY_STALE_TIME_MS, EFSTUDIO_API_BASE } from "@/api/constants";
import type { DbContextDef } from "@/types";

interface ApiDbContext {
  name: string;
  displayName: string;
  isSelected: boolean;
  isDefault: boolean;
  isAvailable: boolean;
  activationError?: string | null;
}

interface ApiDbContextResponse {
  contexts: ApiDbContext[];
}

export const dbContextsQueryKey = ["db-contexts"] as const;

function normalizeContext(context: ApiDbContext): DbContextDef {
  return {
    name: context.name,
    displayName: context.displayName,
    isSelected: context.isSelected,
    isDefault: context.isDefault,
    isAvailable: context.isAvailable,
    activationError: context.activationError ?? undefined,
  };
}

export async function fetchDbContexts(signal?: AbortSignal): Promise<DbContextDef[]> {
  const response = await fetch(`${EFSTUDIO_API_BASE}/contexts`, { signal });

  if (!response.ok) {
    throw new Error(`Failed to load DbContexts (${response.status})`);
  }

  const payload = (await response.json()) as ApiDbContextResponse;
  return payload.contexts.map(normalizeContext);
}

export function dbContextsQueryOptions() {
  return queryOptions({
    queryKey: dbContextsQueryKey,
    queryFn: ({ signal }) => fetchDbContexts(signal),
    staleTime: DEFAULT_QUERY_STALE_TIME_MS,
  });
}

export function useDbContexts(enabled = true) {
  return useQuery({
    ...dbContextsQueryOptions(),
    enabled,
  });
}

export function useSelectDbContext() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (contextName: string) => {
      const response = await fetch(`${EFSTUDIO_API_BASE}/contexts/select`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify({ contextName }),
      });

      if (!response.ok) {
        throw new Error(`Failed to select DbContext (${response.status})`);
      }

      return contextName;
    },
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: dbContextsQueryKey });
      await queryClient.invalidateQueries({ queryKey: ["schema"] });
      await queryClient.invalidateQueries({ queryKey: ["table-data"] });
    },
  });
}
