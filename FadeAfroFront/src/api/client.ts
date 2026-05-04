import axios from 'axios'
import { RateLimitError } from './errors'

const apiClient = axios.create({
  baseURL: import.meta.env.VITE_API_URL,
})

// Attach JWT token to every request
apiClient.interceptors.request.use((config) => {
  const token = localStorage.getItem('token')
  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }
  return config
})

// Throw RateLimitError on 429 so every catch block can handle it uniformly
apiClient.interceptors.response.use(
  response => response,
  error => {
    if (error?.response?.status === 429) throw new RateLimitError()
    throw error
  }
)

export default apiClient
