import { IonContent, IonHeader, IonPage, IonSpinner, IonTitle, IonToolbar } from '@ionic/react';
import { useEffect, useState } from 'react';
import { Redirect, useParams } from 'react-router-dom';
import { ApiError } from '../api/apiClient';
import { getParticipantArrangement, type ParticipantArrangement } from '../api/arrangementsApi';

export function ArrangementLobbyPage() {
  const { arrangementId } = useParams<{ arrangementId: string }>();
  const [arrangement, setArrangement] = useState<ParticipantArrangement>();
  const [error, setError] = useState('');

  useEffect(() => {
    let active = true;

    async function load() {
      try {
        const response = await getParticipantArrangement(arrangementId);
        if (active) {
          setArrangement(response);
          setError('');
        }
      } catch (reason) {
        if (active) {
          setArrangement(undefined);
          setError(reason instanceof ApiError ? reason.message : 'Kunne ikke laste arrangementet.');
        }
      }
    }

    void load();
    const poll = window.setInterval(() => void load(), 5_000);
    return () => {
      active = false;
      window.clearInterval(poll);
    };
  }, [arrangementId]);

  return (
    <IonPage>
      <IonHeader><IonToolbar><IonTitle>Tasting</IonTitle></IonToolbar></IonHeader>
      <IonContent className="ion-padding">
        <main className="participant-shell">
          <p className="eyebrow">Arrangement</p>
          {!arrangement && !error && <IonSpinner aria-label="Laster arrangement" />}
          {error && <p role="alert" className="form-error">{error}</p>}
          {arrangement?.status === 'Started' && arrangement.beers.length > 0
            ? <Redirect to={`/arrangements/${arrangementId}/beers/1`} />
            : arrangement && <>
            <h1>{arrangement.name}</h1>
            <section className="empty-state" data-arrangement-id={arrangementId}>
              {arrangement.status === 'Created' || arrangement.status === 'Active' ? <>
                <h2>Du er med</h2>
                <p>Venter på at arrangementet starter…</p>
              </> : arrangement.status === 'Started' ? <>
                <h2>Arrangementet har startet</h2>
                <p>Ingen øl er tilgjengelige for vurdering.</p>
              </> : <>
                <h2>Arrangementet er ferdig</h2>
                <p>Resultater blir tilgjengelige her.</p>
              </>}
            </section>
          </>}
        </main>
      </IonContent>
    </IonPage>
  );
}
