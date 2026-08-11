import { describe, expect, it } from 'vitest';
import { persistSession, restoreSession, SESSION_STORAGE_KEY, type ParticipantSession } from './session';

const participant: ParticipantSession = {
  token: 'participant-token',
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
});
