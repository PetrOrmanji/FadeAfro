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

export async function getMyMasterProfile(): Promise<MasterProfile> {
  const { data } = await apiClient.get<MasterProfile>('/api/master-profiles/my')
  return data
}

export async function updateMasterProfile(id: string, description: string | null): Promise<void> {
  await apiClient.put(`/api/master-profiles/update-description/${id}`, { description })
}

export async function uploadMasterPhoto(id: string, file: File): Promise<void> {
  const formData = new FormData()
  formData.append('file', file)
  await apiClient.post(`/api/master-profiles/upload-photo/${id}`, formData, {
    headers: { 'Content-Type': 'multipart/form-data' },
  })
}
