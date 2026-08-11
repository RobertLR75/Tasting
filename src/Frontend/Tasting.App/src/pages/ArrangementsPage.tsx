import { IonContent, IonHeader, IonPage, IonTitle, IonToolbar } from '@ionic/react';
import { useAuth } from '../auth/AuthContext';

export function ArrangementsPage() {
  const { session } = useAuth();

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
          <h1>Mine arrangementer</h1>
          <section className="empty-state">
            <h2>Klar for neste smaking</h2>
            <p>Arrangementene dine vises her når discovery-flyten blir tilgjengelig.</p>
          </section>
        </main>
      </IonContent>
    </IonPage>
  );
}
