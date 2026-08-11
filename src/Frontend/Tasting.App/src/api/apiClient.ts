import { removeSession, restoreSession } from '../auth/session';

interface ErrorResponse {
  code: string;
  message: string;
  correlationId: string;
}

export class ApiError extends Error {
  constructor(
    public readonly status: number,
    public readonly code: string,
    message: string,
    public readonly correlationId?: string,
  ) {
    super(message);
  }
}

const apiBaseUrl = (import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5000').replace(/\/$/, '');

export function apiRequest<T>(path: string, init?: RequestInit): Promise<T> {
  return send<T>(path, init);
}

export async function authenticatedApiRequest<T>(path: string, init: RequestInit = {}): Promise<T> {
  const session = restoreSession(localStorage);
  if (!session) {
    throw new ApiError(401, 'unauthorized', 'Authentication is required.');
  }

  const headers = new Headers(init.headers);
  headers.set('Authorization', `Bearer ${session.token}`);
  try {
    return await send<T>(path, { ...init, headers });
  } catch (error) {
    if (error instanceof ApiError && error.status === 401) {
      removeSession(localStorage);
    }
    throw error;
  }
}

async function send<T>(path: string, init?: RequestInit): Promise<T> {
  const response = await fetch(`${apiBaseUrl}${path}`, init);
  if (!response.ok) {
    const error = await readError(response);
    throw new ApiError(response.status, error.code, error.message, error.correlationId);
  }
  return await response.json() as T;
}

async function readError(response: Response): Promise<ErrorResponse> {
  try {
    const error = await response.json() as Partial<ErrorResponse>;
    if (error.code && error.message && error.correlationId) return error as ErrorResponse;
  } catch {
    // The fallback below keeps non-contract responses from leaking into the UI.
  }

  return {
    code: 'request_failed',
    message: 'Kunne ikke fullføre forespørselen.',
    correlationId: '',
  };
}
