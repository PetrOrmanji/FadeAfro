import { createContext, ReactNode, useEffect, useState } from 'react'
import { login } from '@/api/auth'
import { getRoleFromToken, getUserIdFromToken } from '@/utils/jwt'

interface AuthContextValue {
  isLoading: boolean
  token: string | null
  role: string | null
  userId: string | null
}

export const AuthContext = createContext<AuthContextValue>({
  isLoading: true,
  token: null,
  role: null,
  userId: null,
})

const DEV_AUTH = import.meta.env.VITE_DEV_AUTH === 'true'

export function AuthProvider({ children }: { children: ReactNode }) {
  const [isLoading, setIsLoading] = useState(true)
  const [token, setToken] = useState<string | null>(null)
  const [role, setRole] = useState<string | null>(null)
  const [userId, setUserId] = useState<string | null>(null)

  useEffect(() => {
    let cancelled = false

    async function authenticate() {
      // Проверяем есть ли уже токен в сессии
      const stored = sessionStorage.getItem('token')
      if (stored) {
        if (!cancelled) {
          setToken(stored)
          setRole(getRoleFromToken(stored))
          setUserId(getUserIdFromToken(stored))
          setIsLoading(false)
        }
        return
      }

      try {
        const initData = DEV_AUTH
          ? window.Telegram?.WebApp?.initData ?? ''
          : window.Telegram.WebApp.initData

        const { token: jwt } = await login(initData)

        if (!cancelled) {
          sessionStorage.setItem('token', jwt)
          setToken(jwt)
          setRole(getRoleFromToken(jwt))
          setUserId(getUserIdFromToken(jwt))
        }
      } catch (err) {
        console.error('Auth failed:', err)
      } finally {
        if (!cancelled) setIsLoading(false)
      }
    }

    authenticate()

    return () => { cancelled = true }
  }, [])

  return (
    <AuthContext.Provider value={{ isLoading, token, role, userId }}>
      {children}
    </AuthContext.Provider>
  )
}
