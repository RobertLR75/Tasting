import { IonButton, IonContent, IonHeader, IonPage, IonSpinner, IonTitle, IonToolbar } from '@ionic/react';
import { useEffect, useState } from 'react';
import { useHistory, useParams } from 'react-router-dom';
import { ApiError } from '../api/apiClient';
import {
  getArrangementResults,
  getParticipantArrangement,
  type ArrangementResult,
  type ParticipantArrangement,
} from '../api/arrangementsApi';

export function ArrangementResultsPage() {
  const { arrangementId } = useParams<{ arrangementId: string }>();
  const history = useHistory();
  const [results, setResults] = useState<ArrangementResult[]>();
  const [arrangement, setArrangement] = useState<ParticipantArrangement>();
  const [error, setError] = useState('');

  useEffect(() => {
    let active = true;
    Promise.all([getParticipantArrangement(arrangementId), getArrangementResults(arrangementId)])
      .then(([arrangementResponse, resultsResponse]) => {
        if (active) {
          setArrangement(arrangementResponse);
          setResults(resultsResponse);
        }
      })
      .catch(reason => {
        if (active) setError(reason instanceof ApiError ? reason.message : 'Kunne ikke laste resultatene.');
      });
    return () => { active = false; };
  }, [arrangementId]);

  const breweryByBeerId = new Map(arrangement?.beers.map(beer => [beer.id, beer.breweryName]));

  return (
    <IonPage>
      <IonHeader><IonToolbar><IonTitle>Tasting</IonTitle></IonToolbar></IonHeader>
      <IonContent className="ion-padding">
        <main className="participant-shell">
          {(!results || !arrangement) && !error && <IonSpinner aria-label="Laster resultater" />}
          {error && <p role="alert" className="form-error">{error}</p>}
          {results && arrangement && <>
            <h1>{arrangement.name} — Resultater</h1>
            <table className="results-table">
              <thead><tr><th>#</th><th>Øl</th><th>Bryggeri</th><th>Rating</th></tr></thead>
              <tbody>
                {results.map(result => <tr key={result.beerId}>
                  <td>{result.rank}</td>
                  <td>{result.beerNameSnapshot}</td>
                  <td>{breweryByBeerId.get(result.beerId) ?? '—'}</td>
                  <td>{result.totalRating.toFixed(2)}</td>
                </tr>)}
              </tbody>
            </table>
            {results.length === 0 && <p className="empty-state">Ingen resultater er tilgjengelige.</p>}
            <IonButton expand="block" onClick={() => history.push('/arrangements')}>Ferdig</IonButton>
          </>}
        </main>
      </IonContent>
    </IonPage>
  );
}
