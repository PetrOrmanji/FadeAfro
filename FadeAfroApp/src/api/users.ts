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

export interface CurrentUserResponse {
  id: string
  firstName: string
  lastName: string | null
}

export async function getCurrentUser(): Promise<CurrentUserResponse> {
  const { data } = await apiClient.get<CurrentUserResponse>('/api/users/me')
  return data
}

export async function getUserByTelegramId(telegramId: number): Promise<UserResponse> {
  const { data } = await apiClient.get<UserResponse>(`/api/users/get/${telegramId}`)
  return data
}

export async function getAllUsers(page: number, pageSize: number, search?: string): Promise<GetAllUsersResponse> {
  const { data } = await apiClient.get<GetAllUsersResponse>('/api/users/all', {
    params: { page, pageSize, search: search || undefined },
  })
  return data
}

export async function updateUserName(firstName: string, lastName: string | null): Promise<void> {
  await apiClient.put('/api/users/update-name', { firstName, lastName })
}
