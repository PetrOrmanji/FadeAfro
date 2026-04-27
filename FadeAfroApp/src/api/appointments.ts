import { apiClient } from './client'

export type AppointmentStatus =
  | 'Pending'
  | 'Confirmed'
  | 'CancelledByClient'
  | 'CancelledByMaster'
  | 'Completed'

export interface AppointmentItem {
  id: string
  masterProfileId: string
  masterName: string
  masterPhotoUrl: string | null
  serviceId: string
  serviceName: string
  servicePrice: number
  startTime: string   // ISO 8601
  endTime: string     // ISO 8601
  status: AppointmentStatus
  comment: string | null
}

export interface AppointmentsPagedResponse {
  items: AppointmentItem[]
  totalCount: number
  page: number
  pageSize: number
  totalPages: number
}

export async function getClientAppointments(
  clientId: string,
  page = 1,
  pageSize = 20,
): Promise<AppointmentsPagedResponse> {
  const { data } = await apiClient.get<AppointmentsPagedResponse>(
    `/api/appointments/get/client/${clientId}`,
    { params: { page, pageSize } },
  )
  return data
}

export async function cancelAppointmentByClient(id: string): Promise<void> {
  await apiClient.patch(`/api/appointments/cancel-by-client/${id}`)
}

export interface CreateAppointmentPayload {
  clientId: string
  masterProfileId: string
  serviceId: string
  startTime: string   // ISO 8601
  comment?: string | null
}

export async function createAppointment(payload: CreateAppointmentPayload): Promise<string> {
  const { data } = await apiClient.post<{ id: string }>('/api/appointments/create', payload)
  return data.id
}
