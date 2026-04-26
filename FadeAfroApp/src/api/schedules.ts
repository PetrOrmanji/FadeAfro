import { apiClient } from './client'

export interface ScheduleEntry {
  id: string
  dayOfWeek: string   // C# DayOfWeek enum name: "Monday", "Tuesday", etc.
  startTime: string   // "HH:mm:ss" from backend
  endTime: string     // "HH:mm:ss" from backend
}

export interface GetScheduleResponse {
  schedules: ScheduleEntry[]
}

export async function getSchedule(masterProfileId: string): Promise<GetScheduleResponse> {
  const { data } = await apiClient.get<GetScheduleResponse>(`/api/master-schedules/get/${masterProfileId}`)
  return data
}

export async function setSchedule(
  masterProfileId: string,
  dayOfWeek: number,
  startTime: string,  // "HH:mm"
  endTime: string,    // "HH:mm"
): Promise<string> {
  const { data } = await apiClient.post<{ id: string }>('/api/master-schedules/set', {
    masterProfileId,
    dayOfWeek,
    startTime: `${startTime}:00`,
    endTime: `${endTime}:00`,
  })
  return data.id
}

export async function deleteSchedule(scheduleId: string): Promise<void> {
  await apiClient.delete(`/api/master-schedules/delete/${scheduleId}`)
}
