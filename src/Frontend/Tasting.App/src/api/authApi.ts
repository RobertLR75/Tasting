import type { ParticipantSession } from '../auth/session';

interface LoginRequest {
  email: string;
  password: string;
}

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

export async function login(request: LoginRequest): Promise<ParticipantSession> {
  const response = await fetch(`${apiBaseUrl}/api/v1/users/login`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(request),
  });

  if (!response.ok) {
    const error = await readError(response);
    throw new ApiError(response.status, error.code, error.message, error.correlationId);
  }

  return await response.json() as ParticipantSession;
}

async function readError(response: Response): Promise<ErrorResponse> {
  try {
    return await response.json() as ErrorResponse;
  } catch {
    return {
      code: 'request_failed',
      message: 'Ugyldig e-post eller passord.',
      correlationId: '',
    };
  }
}
