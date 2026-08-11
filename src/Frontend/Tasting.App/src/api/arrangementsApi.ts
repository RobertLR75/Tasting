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

export async function listVisibleArrangements(): Promise<VisibleArrangement[]> {
  const response = await authenticatedApiRequest<VisibleArrangementsResponse>('/api/v1/participant/arrangements');
  return response.items;
}

export function joinArrangement(arrangementId: string): Promise<JoinedArrangement> {
  return authenticatedApiRequest(`/api/v1/participant/arrangements/${arrangementId}/join`, { method: 'POST' });
}
