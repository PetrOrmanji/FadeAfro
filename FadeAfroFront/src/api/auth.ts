import apiClient from './client'

export const login = async (initData: string): Promise<string> => {
  const { data } = await apiClient.post<{ token: string }>('/auth/login', { initData })
  return data.token
}
