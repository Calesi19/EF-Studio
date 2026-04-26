import { useMutation } from "@tanstack/react-query";
import { EFSTUDIO_API_BASE } from "@/api/constants";
import type { RecordRow } from "@/types";

interface CreateRecordsRequest {
  tableKey: string;
  records: RecordRow[];
}

interface CreateRecordsResponse {
  tableKey: string;
  createdCount: number;
  records: RecordRow[];
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

  return `Failed to create records (${response.status})`;
}

export async function createRecords(request: CreateRecordsRequest): Promise<CreateRecordsResponse> {
  const response = await fetch(`${EFSTUDIO_API_BASE}/data`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify(request),
  });

  if (!response.ok) {
    throw new Error(await toErrorMessage(response));
  }

  return (await response.json()) as CreateRecordsResponse;
}

export function useCreateRecords() {
  return useMutation({ mutationFn: createRecords });
}
