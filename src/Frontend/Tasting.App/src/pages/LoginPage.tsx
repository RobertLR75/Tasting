import { useState, type FormEvent } from 'react';
import {
  IonButton,
  IonContent,
  IonInput,
  IonItem,
  IonPage,
  IonSpinner,
  IonText,
} from '@ionic/react';
import { useHistory } from 'react-router-dom';
import { login } from '../api/authApi';
import { ApiError } from '../api/apiClient';
import { useAuth } from '../auth/AuthContext';

export function LoginPage() {
  const history = useHistory();
  const auth = useAuth();
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [submitting, setSubmitting] = useState(false);

  async function submit(event: FormEvent) {
    event.preventDefault();
    setError('');
    setSubmitting(true);

    try {
      const session = await login({ email, password });
      auth.login(session);
      history.replace('/arrangements');
    } catch (requestError) {
      setError(requestError instanceof ApiError
        ? requestError.code === 'unauthorized'
          ? 'Ugyldig e-post eller passord.'
          : requestError.message
        : 'Kunne ikke kontakte serveren. Prøv igjen.');
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <IonPage>
      <IonContent fullscreen className="login-page">
        <main className="login-card">
          <p className="eyebrow">Blindsmaking, gjort enkelt</p>
          <h1>Tasting</h1>
          <p className="intro">Logg inn for å delta i ditt neste arrangement.</p>

          <form onSubmit={submit}>
            <IonItem className="form-field">
              <IonInput
                label="E-post"
                labelPlacement="stacked"
                type="email"
                autocomplete="email"
                value={email}
                onIonInput={(event) => setEmail(event.detail.value ?? '')}
                required
              />
            </IonItem>
            <IonItem className="form-field">
              <IonInput
                label="Passord"
                labelPlacement="stacked"
                type="password"
                autocomplete="current-password"
                value={password}
                onIonInput={(event) => setPassword(event.detail.value ?? '')}
                required
              />
            </IonItem>

            {error && <IonText color="danger"><p role="alert" className="form-error">{error}</p></IonText>}

            <IonButton
              type="submit"
              expand="block"
              disabled={!email.trim() || !password.trim() || submitting}
            >
              {submitting ? <IonSpinner name="crescent" /> : 'Logg inn'}
            </IonButton>
          </form>
        </main>
      </IonContent>
    </IonPage>
  );
}
