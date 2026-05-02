import apiClient from './client'

export interface TimeSlot {
  time: string      // "HH:MM:SS"
  isActive: boolean
}

export const getDayAvailability = async (
  masterProfileId: string,
  date: string,            // "YYYY-MM-DD"
  serviceDuration: string, // "HH:MM:SS"
): Promise<TimeSlot[]> => {
  const res = await apiClient.get(
    `/master-profiles/get/${masterProfileId}/day-availability`,
    { params: { date, serviceDuration } },
  )
  return res.data.dateSlots
}
