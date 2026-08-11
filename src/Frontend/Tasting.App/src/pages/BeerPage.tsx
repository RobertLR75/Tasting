import { IonButton, IonContent, IonHeader, IonPage, IonSpinner, IonTitle, IonToolbar } from '@ionic/react';
import { useHistory, useParams } from 'react-router-dom';
import { useStartedArrangement } from './useStartedArrangement';

export function BeerPage() {
  const { arrangementId, beerIndex } = useParams<{ arrangementId: string; beerIndex: string }>();
  const history = useHistory();
  const { arrangement, error } = useStartedArrangement(arrangementId);
  const index = Number(beerIndex) - 1;
  const beer = arrangement?.beers[index];

  return <IonPage>
    <IonHeader><IonToolbar><IonTitle>Tasting</IonTitle></IonToolbar></IonHeader>
    <IonContent className="ion-padding"><main className="participant-shell">
      {!arrangement && !error && <IonSpinner aria-label="Laster øl" />}
      {error && <p role="alert" className="form-error">{error}</p>}
      {arrangement && !beer && <p role="alert" className="form-error">Ølet finnes ikke i dette arrangementet.</p>}
      {beer && <>
        <p className="eyebrow">Øl {index + 1} av {arrangement!.beers.length}</p>
        <h1>{beer.name}</h1>
        <p className="beer-meta">{beer.breweryName} · {beer.beerStyle} · {beer.beerType}</p>
        <IonButton expand="block" onClick={() => history.push(`/arrangements/${arrangementId}/beers/${beerIndex}/rate`)}>
          Vurder ølet
        </IonButton>
      </>}
    </main></IonContent>
  </IonPage>;
}
