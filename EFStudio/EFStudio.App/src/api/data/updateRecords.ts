import { useMutation } from "@tanstack/react-query";
import { EFSTUDIO_API_BASE } from "@/api/constants";
import type { FieldValue } from "@/types";

interface UpdateRecordEntry {
  keys: Record<string, FieldValue>;
  values: Record<string, FieldValue>;
}

interface UpdateRecordsRequest {
  tableKey: string;
  updates: UpdateRecordEntry[];
}

interface UpdateRecordsResponse {
  tableKey: string;
  updatedCount: number;
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

  return `Failed to update records (${response.status})`;
}

export async function updateRecords(request: UpdateRecordsRequest): Promise<UpdateRecordsResponse> {
  const response = await fetch(`${EFSTUDIO_API_BASE}/data`, {
    method: "PUT",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify(request),
  });

  if (!response.ok) {
    throw new Error(await toErrorMessage(response));
  }

  return (await response.json()) as UpdateRecordsResponse;
}

export function useUpdateRecords() {
  return useMutation({ mutationFn: updateRecords });
}
