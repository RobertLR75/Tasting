export const SESSION_STORAGE_KEY = 'tasting.participant.session';
export const SESSION_INVALIDATED_EVENT = 'tasting:participant-session-invalidated';

export interface ParticipantSession {
  token: string;
  email: string;
  firstName: string;
  lastName: string;
  role: 'Admin' | 'User';
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
    if (!isCompleteSession(session) || isExpired(session.token)) {
      storage.removeItem(SESSION_STORAGE_KEY);
      return null;
    }

    return session as ParticipantSession;
  } catch {
    storage.removeItem(SESSION_STORAGE_KEY);
    return null;
  }
}

function isCompleteSession(session: Partial<ParticipantSession>): session is ParticipantSession {
  return typeof session.token === 'string' && session.token.length > 0
    && typeof session.email === 'string' && session.email.length > 0
    && typeof session.firstName === 'string' && session.firstName.length > 0
    && typeof session.lastName === 'string' && session.lastName.length > 0
    && (session.role === 'Admin' || session.role === 'User');
}

function isExpired(token: string): boolean {
  try {
    const payload = token.split('.')[1];
    if (!payload) return true;
    const normalized = payload.replace(/-/g, '+').replace(/_/g, '/');
    const claims = JSON.parse(atob(normalized)) as { exp?: unknown };
    return typeof claims.exp !== 'number' || claims.exp * 1000 <= Date.now();
  } catch {
    return true;
  }
}

export function persistSession(storage: SessionStorage, session: ParticipantSession): void {
  storage.setItem(SESSION_STORAGE_KEY, JSON.stringify(session));
}

export function removeSession(storage: SessionStorage): void {
  storage.removeItem(SESSION_STORAGE_KEY);
}
