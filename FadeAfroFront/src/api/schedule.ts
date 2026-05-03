import apiClient from './client'

export interface MasterScheduleItem {
  id: string
  dayOfWeek: number | string   // число или строка ("Monday" …) в зависимости от сериализатора
  startTime: string
  endTime: string
}

/** .NET DayOfWeek строка → JS getDay() (0=Sun … 6=Sat) */
const DAY_NAME_TO_NUM: Record<string, number> = {
  Sunday: 0, Monday: 1, Tuesday: 2, Wednesday: 3,
  Thursday: 4, Friday: 5, Saturday: 6,
}

/** Нормализует dayOfWeek в число */
export const normalizeDayOfWeek = (dow: number | string): number =>
  typeof dow === 'number' ? dow : (DAY_NAME_TO_NUM[dow] ?? -1)

export interface MasterUnavailabilityItem {
  id: string
  date: string        // "YYYY-MM-DD"
}

export const getMasterSchedules = async (
  masterProfileId: string,
): Promise<MasterScheduleItem[]> => {
  const res = await apiClient.get(`/master-schedules/get/${masterProfileId}`)
  return res.data.schedules
}

export const setMySchedule = async (
  dayOfWeek: number,
  startTime: string,
  endTime: string,
): Promise<void> => {
  await apiClient.post('/master-schedules/set/me', { dayOfWeek, startTime, endTime })
}

export const deleteMySchedule = async (scheduleId: string): Promise<void> => {
  await apiClient.delete(`/master-schedules/delete/me/${scheduleId}`)
}

export const getMasterUnavailabilities = async (
  masterProfileId: string,
): Promise<MasterUnavailabilityItem[]> => {
  const res = await apiClient.get(`/master-unavailabilities/get/${masterProfileId}`)
  return res.data.unavailabilities
}

export const addMyUnavailability = async (date: string): Promise<MasterUnavailabilityItem> => {
  const res = await apiClient.post('/master-unavailabilities/add/me', { date })
  return res.data
}

export const deleteMyUnavailability = async (id: string): Promise<void> => {
  await apiClient.delete(`/master-unavailabilities/delete/me/${id}`)
}
