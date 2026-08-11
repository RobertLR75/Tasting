import { IonApp, IonRouterOutlet, setupIonicReact } from '@ionic/react';
import { IonReactRouter } from '@ionic/react-router';
import { Redirect, Route } from 'react-router-dom';
import { AuthProvider, useAuth } from './auth/AuthContext';
import { ProtectedRoute } from './auth/ProtectedRoute';
import { ArrangementsPage } from './pages/ArrangementsPage';
import { LoginPage } from './pages/LoginPage';

setupIonicReact();

function Routes() {
  const { session } = useAuth();

  return (
    <IonRouterOutlet>
      <Route exact path="/login">
        {session ? <Redirect to="/arrangements" /> : <LoginPage />}
      </Route>
      <ProtectedRoute exact path="/arrangements" component={ArrangementsPage} />
      <Route exact path="/">
        <Redirect to={session ? '/arrangements' : '/login'} />
      </Route>
    </IonRouterOutlet>
  );
}

export default function App() {
  return (
    <IonApp>
      <AuthProvider>
        <IonReactRouter>
          <Routes />
        </IonReactRouter>
      </AuthProvider>
    </IonApp>
  );
}
