import apiClient from './client'

export interface UserResponse {
  id: string
  telegramId: number
  firstName: string
  lastName: string | null
  username: string | null
  roles: UserRole[]
}

export type UserRole = 'Client' | 'Master' | 'Owner'

export interface UserItem {
  id: string
  telegramId: number
  firstName: string
  lastName: string | null
  username: string | null
  roles: UserRole[]
}

export interface PagedResponse<T> {
  items: T[]
  totalCount: number
  page: number
  pageSize: number
  totalPages: number
}

export const getMe = async (): Promise<UserResponse> => {
  const { data } = await apiClient.get<{ userDto: UserResponse }>('/users/get/me')
  return data.userDto
}

export const getAllUsers = async (
  page: number,
  pageSize: number,
  search?: string,
): Promise<PagedResponse<UserItem>> => {
  const params: Record<string, unknown> = { page, pageSize }
  if (search) params.search = search
  const { data } = await apiClient.get<PagedResponse<UserItem>>('/users/get/all', { params })
  return data
}

export const assignMaster = async (userId: string): Promise<void> => {
  await apiClient.post(`/master-profiles/assign/${userId}`)
}

export const dismissMaster = async (userId: string): Promise<void> => {
  await apiClient.delete(`/master-profiles/dismiss/${userId}`)
}
