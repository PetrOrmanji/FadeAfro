import { apiClient } from './client'

export interface ServiceItem {
  id: string
  name: string
  description: string | null
  price: number
  duration: string  // "HH:mm:ss" from backend
}

export interface GetServicesResponse {
  services: ServiceItem[]
}

export interface ServicePayload {
  name: string
  description: string | null
  price: number
  duration: string  // "HH:mm:ss"
}

export async function getServices(masterProfileId: string): Promise<GetServicesResponse> {
  const { data } = await apiClient.get<GetServicesResponse>(`/api/services/get/${masterProfileId}`)
  return data
}

export async function addService(masterProfileId: string, payload: ServicePayload): Promise<string> {
  const { data } = await apiClient.post<{ id: string }>('/api/services/add', {
    masterProfileId,
    ...payload,
  })
  return data.id
}

export async function updateService(serviceId: string, payload: ServicePayload): Promise<void> {
  await apiClient.put(`/api/services/update/${serviceId}`, payload)
}

export async function deleteService(serviceId: string): Promise<void> {
  await apiClient.delete(`/api/services/delete/${serviceId}`)
}
