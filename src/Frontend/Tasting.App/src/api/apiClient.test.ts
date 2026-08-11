import { afterEach, describe, expect, it, vi } from 'vitest';
import { persistSession, type ParticipantSession } from '../auth/session';
import { authenticatedApiRequest } from './apiClient';

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
});

function createToken(expiresAt: number): string {
  return `header.${btoa(JSON.stringify({ exp: Math.floor(expiresAt / 1000) }))}.signature`;
}
