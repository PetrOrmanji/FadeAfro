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

export const getMasterPhotoUrl = (masterProfileId: string) =>
  `/api/master-profiles/get/${masterProfileId}/photo`
