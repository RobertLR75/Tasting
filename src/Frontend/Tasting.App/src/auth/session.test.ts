import { describe, expect, it } from 'vitest';
import { persistSession, restoreSession, SESSION_STORAGE_KEY, type ParticipantSession } from './session';

const participant: ParticipantSession = {
  token: createToken(Date.now() + 60_000),
  email: 'participant@tasting.no',
  firstName: 'Pat',
  lastName: 'Ticipant',
  role: 'User',
};

describe('participant session', () => {
  it('restores a persisted authenticated session', () => {
    persistSession(localStorage, participant);

    expect(restoreSession(localStorage)).toEqual(participant);
  });

  it('discards malformed stored session data', () => {
    localStorage.setItem(SESSION_STORAGE_KEY, '{not-json');

    expect(restoreSession(localStorage)).toBeNull();
    expect(localStorage.getItem(SESSION_STORAGE_KEY)).toBeNull();
  });

  it('discards an expired session', () => {
    persistSession(localStorage, { ...participant, token: createToken(Date.now() - 60_000) });

    expect(restoreSession(localStorage)).toBeNull();
    expect(localStorage.getItem(SESSION_STORAGE_KEY)).toBeNull();
  });

  it('discards a session with an unknown role', () => {
    localStorage.setItem(SESSION_STORAGE_KEY, JSON.stringify({ ...participant, role: 'Guest' }));

    expect(restoreSession(localStorage)).toBeNull();
  });
});

function createToken(expiresAt: number): string {
  return `header.${btoa(JSON.stringify({ exp: Math.floor(expiresAt / 1000) }))}.signature`;
}
