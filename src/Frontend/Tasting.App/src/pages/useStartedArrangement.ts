import { useEffect, useState } from 'react';
import { ApiError } from '../api/apiClient';
import { getParticipantArrangement, type ParticipantArrangement } from '../api/arrangementsApi';

export function useStartedArrangement(arrangementId: string) {
  const [arrangement, setArrangement] = useState<ParticipantArrangement>();
  const [error, setError] = useState('');

  useEffect(() => {
    let active = true;

    async function load() {
      try {
        const response = await getParticipantArrangement(arrangementId);
        if (!active) return;
        if (response.status !== 'Started') {
          setArrangement(undefined);
          setError('Vurdering er bare tilgjengelig mens arrangementet pågår.');
          return;
        }
        setArrangement(response);
        setError('');
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

  return { arrangement, error };
}
