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

export interface GetAvailableDatesResponse {
  dates: string[]  // "YYYY-MM-DD"
}

export async function getAvailableDates(
  masterProfileId: string,
  serviceId: string,
  year: number,
  month: number,
): Promise<GetAvailableDatesResponse> {
  const { data } = await apiClient.get<GetAvailableDatesResponse>(
    `/api/master-profiles/available-dates/${masterProfileId}`,
    { params: { serviceId, year, month } },
  )
  return data
}
