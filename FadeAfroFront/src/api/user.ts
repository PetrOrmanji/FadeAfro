import apiClient from './client'

export interface UserResponse {
  id: string
  firstName: string
  lastName: string | null
}

export const getMe = async (): Promise<UserResponse> => {
  const { data } = await apiClient.get<UserResponse>('/users/get/me')
  return data
}
