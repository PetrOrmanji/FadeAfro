import { apiClient } from './client'

export interface UserResponse {
  id: string
  telegramId: number
  firstName: string
  lastName: string | null
  username: string | null
  roles: string[]
}

export interface GetAllUsersResponse {
  items: UserResponse[]
  totalCount: number
  page: number
  pageSize: number
  totalPages: number
}

export async function getUserByTelegramId(telegramId: number): Promise<UserResponse> {
  const { data } = await apiClient.get<UserResponse>(`/api/users/get/${telegramId}`)
  return data
}

export async function getAllUsers(page: number, pageSize: number): Promise<GetAllUsersResponse> {
  const { data } = await apiClient.get<GetAllUsersResponse>('/api/users/all', {
    params: { page, pageSize },
  })
  return data
}
