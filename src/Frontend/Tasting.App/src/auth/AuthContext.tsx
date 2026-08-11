import { createContext, type PropsWithChildren, useContext, useEffect, useMemo, useState } from 'react';
import {
  persistSession,
  removeSession,
  restoreSession,
  SESSION_INVALIDATED_EVENT,
  type ParticipantSession,
} from './session';

interface AuthContextValue {
  session: ParticipantSession | null;
  login(session: ParticipantSession): void;
  logout(): void;
}

const AuthContext = createContext<AuthContextValue | undefined>(undefined);

export function AuthProvider({ children }: PropsWithChildren) {
  const [session, setSession] = useState<ParticipantSession | null>(() => restoreSession(localStorage));
  useEffect(() => {
    const invalidate = () => setSession(null);
    window.addEventListener(SESSION_INVALIDATED_EVENT, invalidate);
    return () => window.removeEventListener(SESSION_INVALIDATED_EVENT, invalidate);
  }, []);
  const value = useMemo<AuthContextValue>(() => ({
    session,
    login(nextSession) {
      persistSession(localStorage, nextSession);
      setSession(nextSession);
    },
    logout() {
      removeSession(localStorage);
      setSession(null);
    },
  }), [session]);

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth(): AuthContextValue {
  const value = useContext(AuthContext);
  if (!value) throw new Error('useAuth must be used inside AuthProvider');
  return value;
}
