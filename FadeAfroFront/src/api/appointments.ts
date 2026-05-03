import apiClient from './client'

export interface AppointmentService {
  serviceId: string | null
  serviceName: string
  price: number
  duration: string
}

export interface AppointmentMaster {
  firstName: string
  lastName: string | null
  photoUrl: string | null
}

export interface ClientAppointment {
  id: string
  startTime: string  // UTC ISO
  endTime: string    // UTC ISO
  comment: string | null
  services: AppointmentService[]
  master: AppointmentMaster | null
}

export interface AppointmentClient {
  firstName: string
  lastName: string | null
}

export interface MasterAppointment {
  id: string
  startTime: string
  endTime: string
  comment: string | null
  services: AppointmentService[]
  client: AppointmentClient | null
}

export const getMasterAppointments = async (): Promise<MasterAppointment[]> => {
  const res = await apiClient.get('/appointments/master/get/me/appointments')
  return res.data.appointments
}

export const cancelMasterAppointment = async (appointmentId: string): Promise<void> => {
  await apiClient.patch(`/appointments/master/cancel/me/${appointmentId}`)
}

export const bookAppointment = async (params: {
  masterProfileId: string
  serviceIds: string[]
  startTime: string
  comment?: string
}): Promise<void> => {
  await apiClient.post('/appointments/client/me/book', params)
}

export const getMyAppointments = async (): Promise<ClientAppointment[]> => {
  const res = await apiClient.get('/appointments/client/get/me/appointments')
  return res.data.appointments
}

export const cancelMyAppointment = async (appointmentId: string): Promise<void> => {
  await apiClient.patch(`/appointments/client/cancel/me/${appointmentId}`)
}
