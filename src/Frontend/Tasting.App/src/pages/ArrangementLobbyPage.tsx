import { IonContent, IonHeader, IonPage, IonTitle, IonToolbar } from '@ionic/react';
import { useLocation, useParams } from 'react-router-dom';

interface LobbyState { name?: string }

export function ArrangementLobbyPage() {
  const { arrangementId } = useParams<{ arrangementId: string }>();
  const location = useLocation<LobbyState>();

  return (
    <IonPage>
      <IonHeader><IonToolbar><IonTitle>Tasting</IonTitle></IonToolbar></IonHeader>
      <IonContent className="ion-padding">
        <main className="participant-shell">
          <p className="eyebrow">Arrangement</p>
          <h1>{location.state?.name ?? 'Lobby'}</h1>
          <section className="empty-state" data-arrangement-id={arrangementId}>
            <h2>Du er med</h2>
            <p>Venter på at arrangementet starter…</p>
          </section>
        </main>
      </IonContent>
    </IonPage>
  );
}
