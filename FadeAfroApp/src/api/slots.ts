import { apiClient } from './client'

export interface TimeSlot {
  start: string  // "HH:mm:ss"
  end: string    // "HH:mm:ss"
}

export interface GetAvailableSlotsResponse {
  slots: TimeSlot[]
}

export async function getAvailableSlots(
  masterProfileId: string,
  serviceId: string,
  date: string,  // "YYYY-MM-DD"
): Promise<GetAvailableSlotsResponse> {
  const { data } = await apiClient.get<GetAvailableSlotsResponse>(
    `/api/master-profiles/available-slots/${masterProfileId}`,
    { params: { serviceId, date } },
  )
  return data
}
