import type { ComponentType } from 'react';
import { Redirect, Route, type RouteProps } from 'react-router-dom';
import { useAuth } from './AuthContext';

interface ProtectedRouteProps extends RouteProps {
  component: ComponentType;
}

export function ProtectedRoute({ component: Component, ...routeProps }: ProtectedRouteProps) {
  const { session } = useAuth();

  return (
    <Route
      {...routeProps}
      render={({ location }) => session
        ? <Component />
        : <Redirect to={{ pathname: '/login', state: { from: location } }} />}
    />
  );
}
