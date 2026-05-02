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

export const getMasterServices = async (masterProfileId: string): Promise<MasterService[]> => {
  const { data } = await apiClient.get<GetMasterServicesResponse>(`/master-services/get/${masterProfileId}`)
  return data.services
}
