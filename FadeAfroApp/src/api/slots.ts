import { apiClient } from './client'

export interface DayAvailability {
  date: string    // "YYYY-MM-DD"
  slots: string[] // ["09:00:00", "09:30:00", ...]
}

export interface GetMasterAvailabilityResponse {
  days: DayAvailability[]
}

export async function getMasterAvailability(
  masterProfileId: string,
  serviceId: string,
): Promise<GetMasterAvailabilityResponse> {
  const { data } = await apiClient.get<GetMasterAvailabilityResponse>(
    `/api/master-profiles/availability/${masterProfileId}`,
    { params: { serviceId } },
  )
  return data
}
