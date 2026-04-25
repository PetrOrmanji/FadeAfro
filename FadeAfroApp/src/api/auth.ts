import { apiClient } from './client'

export interface LoginResponse {
  token: string
}

export async function login(initData: string): Promise<LoginResponse> {
  const { data } = await apiClient.post<LoginResponse>('/api/auth/login', { initData })
  return data
}
