import { authenticatedApiRequest } from './apiClient';

export interface VisibleArrangement {
  id: string;
  name: string;
  description?: string;
  joined: boolean;
}

interface VisibleArrangementsResponse {
  items: VisibleArrangement[];
}

export interface JoinedArrangement {
  id: string;
  name: string;
  status: 'Active';
}

export type ArrangementStatus = 'Created' | 'Active' | 'Started' | 'Completed';

export interface ParticipantBeer {
  id: string;
  name: string;
  breweryName: string;
  beerStyle: string;
  beerType: string;
}

export interface ParticipantArrangement {
  id: string;
  name: string;
  status: ArrangementStatus;
  beers: ParticipantBeer[];
}

export interface ArrangementResult {
  rank: number;
  beerId: string;
  beerNameSnapshot: string;
  totalRating: number;
  ratingCount: number;
  standardDeviation: number;
}

interface ArrangementResultsResponse {
  results: ArrangementResult[];
}

export async function listVisibleArrangements(): Promise<VisibleArrangement[]> {
  const response = await authenticatedApiRequest<VisibleArrangementsResponse>('/api/v1/participant/arrangements');
  return response.items;
}

export function joinArrangement(arrangementId: string): Promise<JoinedArrangement> {
  return authenticatedApiRequest(`/api/v1/participant/arrangements/${arrangementId}/join`, { method: 'POST' });
}

export function getParticipantArrangement(arrangementId: string): Promise<ParticipantArrangement> {
  return authenticatedApiRequest(`/api/v1/participant/arrangements/${arrangementId}`);
}

export async function getArrangementResults(arrangementId: string): Promise<ArrangementResult[]> {
  const response = await authenticatedApiRequest<ArrangementResultsResponse>(`/api/v1/arrangements/${arrangementId}/results`);
  return response.results;
}
