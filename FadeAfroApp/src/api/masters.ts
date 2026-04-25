import { apiClient } from './client'

export interface MasterProfile {
  id: string
  masterId: string
  firstName: string
  lastName: string | null
  photoUrl: string | null
  description: string | null
}

export interface GetAllMastersResponse {
  masters: MasterProfile[]
}

export interface CreateMasterProfileRequest {
  masterId: string
  photoUrl?: string
  description?: string
}

export async function getMasters(): Promise<GetAllMastersResponse> {
  const { data } = await apiClient.get<GetAllMastersResponse>('/api/master-profiles/all')
  return data
}

export async function createMasterProfile(request: CreateMasterProfileRequest): Promise<void> {
  await apiClient.post('/api/master-profiles/create', request)
}

export async function dismissMaster(userId: string): Promise<void> {
  await apiClient.delete(`/api/master-profiles/dismiss/${userId}`)
}
