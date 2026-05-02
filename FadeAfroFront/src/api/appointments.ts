import apiClient from './client'

export const bookAppointment = async (params: {
  masterProfileId: string
  serviceIds: string[]
  startTime: string
  comment?: string
}): Promise<void> => {
  await apiClient.post('/appointments/client/me/book', params)
}
