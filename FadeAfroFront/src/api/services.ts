import apiClient from './client'

export interface MasterService {
  id: string
  name: string
  description: string | null
  price: number
  duration: string // "HH:MM:SS"
}

export interface GetMasterServicesResponse {
  services: MasterService[]
}

export interface ServicePayload {
  name: string
  description?: string
  price: number
  duration: string // "HH:MM:SS"
}

export const getMasterServices = async (masterProfileId: string): Promise<MasterService[]> => {
  const { data } = await apiClient.get<GetMasterServicesResponse>(`/master-services/get/${masterProfileId}`)
  return data.services
}

export const addMyService = async (payload: ServicePayload): Promise<void> => {
  await apiClient.post('/master-services/add/me', payload)
}

export const updateMyService = async (serviceId: string, payload: ServicePayload): Promise<void> => {
  await apiClient.put(`/master-services/update/me/${serviceId}`, payload)
}

export const deleteMyService = async (serviceId: string): Promise<void> => {
  await apiClient.delete(`/master-services/delete/me/${serviceId}`)
}
