import type { ParticipantSession } from '../auth/session';
import { apiRequest } from './apiClient';

interface LoginRequest {
  email: string;
  password: string;
}

export async function login(request: LoginRequest): Promise<ParticipantSession> {
  return apiRequest<ParticipantSession>('/api/v1/users/login', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(request),
  });
}
