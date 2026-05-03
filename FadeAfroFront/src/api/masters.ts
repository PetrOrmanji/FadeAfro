import apiClient from './client'

export interface MasterProfile {
  id: string
  firstName: string
  lastName: string | null
  photoUrl: string | null
  description: string | null
}

export interface GetAllMastersResponse {
  masters: MasterProfile[]
}

export const getAllMasters = async (): Promise<MasterProfile[]> => {
  const { data } = await apiClient.get<GetAllMastersResponse>('/master-profiles/get/all')
  return data.masters
}

export const getMyMasterProfile = async (): Promise<MasterProfile> => {
  const { data } = await apiClient.get<MasterProfile>('/master-profiles/get/me')
  return data
}

export const uploadMasterPhoto = async (file: File): Promise<void> => {
  const formData = new FormData()
  formData.append('file', file)
  await apiClient.post('/master-profiles/upload/me/photo', formData, {
    headers: { 'Content-Type': 'multipart/form-data' },
  })
}

export const getMasterPhotoUrl = (masterProfileId: string, cacheBust?: number) => {
  const base = `/api/master-profiles/get/${masterProfileId}/photo`
  return cacheBust ? `${base}?t=${cacheBust}` : base
}
