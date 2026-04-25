import { useMutation, useQueryClient } from "@tanstack/react-query";
import { EFSTUDIO_API_BASE } from "@/api/constants";
import type { RecordRow } from "@/types";

interface DeleteRecordsRequest {
  tableKey: string;
  keys: Record<string, RecordRow[string]>[];
}

interface DeleteRecordsResponse {
  tableKey: string;
  deletedCount: number;
}

async function toErrorMessage(response: Response): Promise<string> {
  try {
    const payload = (await response.json()) as { message?: string };
    if (payload.message) {
      return payload.message;
    }
  } catch {
    // Fall back to a generic message when the server does not return JSON.
  }

  return `Failed to delete records (${response.status})`;
}

export async function deleteRecords(request: DeleteRecordsRequest): Promise<DeleteRecordsResponse> {
  const response = await fetch(`${EFSTUDIO_API_BASE}/data`, {
    method: "DELETE",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify(request),
  });

  if (!response.ok) {
    throw new Error(await toErrorMessage(response));
  }

  return (await response.json()) as DeleteRecordsResponse;
}

export function useDeleteRecords() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: deleteRecords,
    onSuccess: async (response) => {
      await queryClient.invalidateQueries({
        queryKey: ["table-data", response.tableKey],
      });
    },
  });
}
