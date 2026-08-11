export const SESSION_STORAGE_KEY = 'tasting.participant.session';

export interface ParticipantSession {
  token: string;
  email: string;
  firstName: string;
  lastName: string;
  role: string;
}

export interface SessionStorage {
  getItem(key: string): string | null;
  setItem(key: string, value: string): void;
  removeItem(key: string): void;
}

export function restoreSession(storage: SessionStorage): ParticipantSession | null {
  const stored = storage.getItem(SESSION_STORAGE_KEY);
  if (!stored) return null;

  try {
    const session = JSON.parse(stored) as Partial<ParticipantSession>;
    if (!session.token || !session.email || !session.role) {
      storage.removeItem(SESSION_STORAGE_KEY);
      return null;
    }

    return session as ParticipantSession;
  } catch {
    storage.removeItem(SESSION_STORAGE_KEY);
    return null;
  }
}

export function persistSession(storage: SessionStorage, session: ParticipantSession): void {
  storage.setItem(SESSION_STORAGE_KEY, JSON.stringify(session));
}

export function removeSession(storage: SessionStorage): void {
  storage.removeItem(SESSION_STORAGE_KEY);
}
