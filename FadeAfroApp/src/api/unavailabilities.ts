import { apiClient } from './client'

export interface UnavailabilityItem {
  id: string
  date: string        // "YYYY-MM-DD"
  startTime: string | null  // "HH:mm:ss" или null (весь день)
  endTime: string | null    // "HH:mm:ss" или null (весь день)
}

export async function getUnavailabilities(masterProfileId: string): Promise<UnavailabilityItem[]> {
  const { data } = await apiClient.get<{ unavailabilities: UnavailabilityItem[] }>(
    `/api/master-unavailabilities/get/${masterProfileId}`
  )
  return data.unavailabilities
}

export async function addUnavailability(
  masterProfileId: string,
  date: string,
  startTime: string | null,
  endTime: string | null,
): Promise<string> {
  const { data } = await apiClient.post<{ id: string }>('/api/master-unavailabilities/add', {
    masterProfileId,
    date,
    startTime,
    endTime,
  })
  return data.id
}

export async function deleteUnavailability(id: string): Promise<void> {
  await apiClient.delete(`/api/master-unavailabilities/delete/${id}`)
}
