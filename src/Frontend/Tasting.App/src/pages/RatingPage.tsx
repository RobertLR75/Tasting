import { IonButton, IonContent, IonHeader, IonItem, IonLabel, IonPage, IonRange, IonSpinner, IonTitle, IonToolbar } from '@ionic/react';
import { useState } from 'react';
import { useHistory, useParams } from 'react-router-dom';
import { ApiError } from '../api/apiClient';
import { submitRating, type RatingScores } from '../api/arrangementsApi';
import { useStartedArrangement } from './useStartedArrangement';

type ScoreName = keyof RatingScores;
const fields: Array<{ name: ScoreName; label: string }> = [
  { name: 'visibility', label: 'Utseende' },
  { name: 'smell', label: 'Lukt' },
  { name: 'taste', label: 'Smak' },
  { name: 'toast', label: 'Skål' },
];

export function RatingPage() {
  const { arrangementId, beerIndex } = useParams<{ arrangementId: string; beerIndex: string }>();
  const history = useHistory();
  const { arrangement, error: loadError } = useStartedArrangement(arrangementId);
  const index = Number(beerIndex) - 1;
  const beer = arrangement?.beers[index];
  const [scores, setScores] = useState<Partial<RatingScores>>({});
  const [error, setError] = useState('');
  const [saving, setSaving] = useState(false);
  const complete = fields.every(field => scores[field.name] !== undefined);

  async function save() {
    if (!beer || !complete) return;
    setSaving(true);
    setError('');
    try {
      await submitRating(arrangementId, beer.id, scores as RatingScores);
      const nextIndex = index + 2;
      history.push(nextIndex <= arrangement!.beers.length
        ? `/arrangements/${arrangementId}/beers/${nextIndex}`
        : `/arrangements/${arrangementId}/beers/1`);
    } catch (reason) {
      setError(reason instanceof ApiError && reason.status === 409
        ? 'Ratingen ble oppdatert av en annen instans — last siden på nytt.'
        : reason instanceof ApiError ? reason.message : 'Kunne ikke lagre ratingen.');
    } finally {
      setSaving(false);
    }
  }

  return <IonPage>
    <IonHeader><IonToolbar><IonTitle>Tasting</IonTitle></IonToolbar></IonHeader>
    <IonContent className="ion-padding"><main className="participant-shell">
      {!arrangement && !loadError && <IonSpinner aria-label="Laster vurdering" />}
      {loadError && <p role="alert" className="form-error">{loadError}</p>}
      {arrangement && !beer && <p role="alert" className="form-error">Ølet finnes ikke i dette arrangementet.</p>}
      {beer && <>
        <p className="eyebrow">Øl {index + 1} av {arrangement!.beers.length} — vurdering</p>
        <h1>{beer.name}</h1>
        <section className="rating-form">
          {fields.map(field => <IonItem key={field.name} lines="none" className="rating-field">
            <IonLabel>{field.label}: {scores[field.name]?.toFixed(1) ?? 'Ikke satt'}</IonLabel>
            <IonRange aria-label={field.label} min={0} max={10} step={0.5} snaps ticks
              onIonChange={event => setScores(current => ({ ...current, [field.name]: Number(event.detail.value) }))} />
          </IonItem>)}
        </section>
        {error && <p role="alert" className="form-error">{error}</p>}
        <IonButton expand="block" disabled={!complete || saving} onClick={() => void save()}>
          {saving ? 'Lagrer…' : 'Lagre og gå videre'}
        </IonButton>
      </>}
    </main></IonContent>
  </IonPage>;
}
