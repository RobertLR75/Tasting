import { afterEach, describe, expect, it, vi } from 'vitest';
import { persistSession, SESSION_INVALIDATED_EVENT, SESSION_STORAGE_KEY, type ParticipantSession } from '../auth/session';
import { ApiError, authenticatedApiRequest } from './apiClient';

const session: ParticipantSession = {
  token: createToken(Date.now() + 60_000),
  email: 'participant@tasting.no',
  firstName: 'Pat',
  lastName: 'Ticipant',
  role: 'User',
};

describe('authenticated API requests', () => {
  afterEach(() => vi.unstubAllGlobals());

  it('sends the persisted participant token as a bearer token', async () => {
    persistSession(localStorage, session);
    const fetchMock = vi.fn().mockResolvedValue(new Response(JSON.stringify({ value: 42 }), {
      status: 200,
      headers: { 'Content-Type': 'application/json' },
    }));
    vi.stubGlobal('fetch', fetchMock);

    await authenticatedApiRequest('/api/v1/example');

    const [, init] = fetchMock.mock.calls[0] as [string, RequestInit];
    expect(new Headers(init.headers).get('Authorization')).toBe(`Bearer ${session.token}`);
  });

  it('invalidates the live session when an authenticated request returns 401', async () => {
    persistSession(localStorage, session);
    const invalidated = vi.fn();
    window.addEventListener(SESSION_INVALIDATED_EVENT, invalidated, { once: true });
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue(new Response(JSON.stringify({
      code: 'unauthorized',
      message: 'Authentication is required.',
      correlationId: 'corr-1',
    }), { status: 401, headers: { 'Content-Type': 'application/json' } })));

    await expect(authenticatedApiRequest('/api/v1/example')).rejects.toBeInstanceOf(ApiError);

    expect(localStorage.getItem(SESSION_STORAGE_KEY)).toBeNull();
    expect(invalidated).toHaveBeenCalledOnce();
  });
});

function createToken(expiresAt: number): string {
  return `header.${btoa(JSON.stringify({ exp: Math.floor(expiresAt / 1000) }))}.signature`;
}
