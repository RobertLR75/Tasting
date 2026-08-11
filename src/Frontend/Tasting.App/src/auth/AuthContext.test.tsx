import { act, render, screen } from '@testing-library/react';
import { MemoryRouter, Route } from 'react-router-dom';
import { describe, expect, it } from 'vitest';
import { AuthProvider } from './AuthContext';
import { ProtectedRoute } from './ProtectedRoute';
import { persistSession, SESSION_INVALIDATED_EVENT, type ParticipantSession } from './session';

const session: ParticipantSession = {
  token: `header.${btoa(JSON.stringify({ exp: Math.floor(Date.now() / 1000) + 60 }))}.signature`,
  email: 'participant@tasting.no',
  firstName: 'Pat',
  lastName: 'Ticipant',
  role: 'User',
};

describe('participant authentication state', () => {
  it('leaves a protected route immediately when the API invalidates the session', async () => {
    persistSession(localStorage, session);
    render(
      <AuthProvider>
        <MemoryRouter initialEntries={['/arrangements']}>
          <ProtectedRoute path="/arrangements" component={() => <h1>Protected</h1>} />
          <Route path="/login"><h1>Login</h1></Route>
        </MemoryRouter>
      </AuthProvider>,
    );
    expect(screen.getByRole('heading', { name: 'Protected' })).toBeInTheDocument();

    await act(async () => window.dispatchEvent(new Event(SESSION_INVALIDATED_EVENT)));

    expect(screen.getByRole('heading', { name: 'Login' })).toBeInTheDocument();
  });
});
