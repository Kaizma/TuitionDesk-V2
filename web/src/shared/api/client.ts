export interface ProblemDetails {
  type?: string;
  title?: string;
  status?: number;
  detail?: string;
  code?: string;
  traceId?: string;
  errors?: Record<string, string[]>;
}

export class ApiError extends Error {
  constructor(
    message: string,
    public readonly status: number,
    public readonly code?: string,
    public readonly errors?: Record<string, string[]>,
  ) {
    super(message);
    this.name = "ApiError";
  }
}

function configuration() {
  return typeof window !== "undefined" ? window.__TUITIONDESK__ : undefined;
}

export async function api<T>(path: string, init: RequestInit = {}): Promise<T> {
  const config = configuration();
  const headers = new Headers(init.headers);

  headers.set("Accept", "application/json");
  if (init.body && !headers.has("Content-Type")) {
    headers.set("Content-Type", "application/json");
  }
  if (config?.token) {
    headers.set("X-TuitionDesk-Token", config.token);
  }

  let response: Response;
  try {
    response = await fetch(`${config?.apiBaseUrl ?? ""}${path}`, { ...init, headers });
  } catch {
    throw new ApiError(
      "The local TuitionDesk service is not reachable. Restart the application and try again.",
      0,
      "network.unreachable",
    );
  }

  if (!response.ok) {
    let problem: ProblemDetails | undefined;
    try {
      problem = (await response.json()) as ProblemDetails;
    } catch {
      problem = undefined;
    }

    throw new ApiError(
      problem?.detail ?? problem?.title ?? `The request failed (${response.status}).`,
      response.status,
      problem?.code,
      problem?.errors,
    );
  }

  if (response.status === 204) {
    return undefined as T;
  }

  return (await response.json()) as T;
}
