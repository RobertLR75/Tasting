import { IonButton, IonContent, IonHeader, IonPage, IonSpinner, IonTitle, IonToolbar } from '@ionic/react';
import { useEffect, useState } from 'react';
import { useHistory } from 'react-router-dom';
import { ApiError } from '../api/apiClient';
import { joinArrangement, listVisibleArrangements, type VisibleArrangement } from '../api/arrangementsApi';
import { useAuth } from '../auth/AuthContext';

export function ArrangementsPage() {
  const { session } = useAuth();
  const history = useHistory();
  const [arrangements, setArrangements] = useState<VisibleArrangement[]>([]);
  const [loading, setLoading] = useState(true);
  const [joiningId, setJoiningId] = useState<string>();
  const [error, setError] = useState('');

  useEffect(() => {
    listVisibleArrangements()
      .then(setArrangements)
      .catch((reason: unknown) => setError(reason instanceof ApiError ? reason.message : 'Kunne ikke laste arrangementer.'))
      .finally(() => setLoading(false));
  }, []);

  async function join(arrangement: VisibleArrangement) {
    setError('');
    setJoiningId(arrangement.id);
    try {
      const joined = arrangement.joined
        ? { id: arrangement.id, name: arrangement.name }
        : await joinArrangement(arrangement.id);
      history.push(`/arrangements/${joined.id}/lobby`, { name: joined.name });
    } catch (reason) {
      setError(reason instanceof ApiError ? reason.message : 'Kunne ikke bli med i arrangementet.');
    } finally {
      setJoiningId(undefined);
    }
  }

  return (
    <IonPage>
      <IonHeader>
        <IonToolbar>
          <IonTitle>Tasting</IonTitle>
        </IonToolbar>
      </IonHeader>
      <IonContent className="ion-padding">
        <main className="participant-shell">
          <p className="eyebrow">Hei, {session?.firstName}</p>
          <h1>Aktive arrangementer</h1>
          {error && <p role="alert" className="form-error">{error}</p>}
          {loading && <IonSpinner aria-label="Laster arrangementer" />}
          {!loading && arrangements.length === 0 && <section className="empty-state"><p>Ingen aktive arrangementer.</p></section>}
          <section className="arrangement-list">
            {arrangements.map(arrangement => (
              <article className="arrangement-card" key={arrangement.id}>
                <div><h2>{arrangement.name}</h2>{arrangement.description && <p>{arrangement.description}</p>}</div>
                <IonButton onClick={() => void join(arrangement)} disabled={joiningId === arrangement.id}>
                  {joiningId === arrangement.id ? 'Blir med…' : arrangement.joined ? 'Gå til lobby' : 'Bli med'}
                </IonButton>
              </article>
            ))}
          </section>
        </main>
      </IonContent>
    </IonPage>
  );
}
