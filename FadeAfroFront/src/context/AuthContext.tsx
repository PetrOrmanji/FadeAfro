import { createContext, useContext, useEffect, useState, ReactNode } from 'react'
import { useInitData } from '@tma.js/sdk-react'
import { login } from '../api/auth'
import { getMockUser } from '../mock/telegramMock'

type Role = 'Client' | 'Master' | 'Owner'

interface AuthState {
  isAuthenticated: boolean
  role: Role | null
  isLoading: boolean
}

const AuthContext = createContext<AuthState>({
  isAuthenticated: false,
  role: null,
  isLoading: true,
})

export const useAuth = () => useContext(AuthContext)

// ─────────────────────────────────────────────────────────────────────────────

const DEV = import.meta.env.DEV

export const AuthProvider = ({ children }: { children: ReactNode }) => {
  const [state, setState] = useState<AuthState>({
    isAuthenticated: false,
    role: null,
    isLoading: true,
  })

  const initData = useInitData()

  useEffect(() => {
    // DEV: пропускаем backend, берём роль из мока
    if (DEV) {
      const mockUser = getMockUser()
      setState({ isAuthenticated: true, role: mockUser.role, isLoading: false })
      return
    }

    // PROD: реальная авторизация через Telegram initData
    const raw = initData?.toString()
    if (!raw) return

    login(raw)
      .then((token) => {
        localStorage.setItem('token', token)
        // Роль достаём из JWT payload
        const payload = JSON.parse(atob(token.split('.')[1]))
        const roles: string[] = payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] ?? []
        const role = resolveRole(roles)
        setState({ isAuthenticated: true, role, isLoading: false })
      })
      .catch(() => {
        setState({ isAuthenticated: false, role: null, isLoading: false })
      })
  }, [initData])

  return <AuthContext.Provider value={state}>{children}</AuthContext.Provider>
}

// Если у пользователя несколько ролей — берём наивысшую
const resolveRole = (roles: string[]): Role => {
  if (roles.includes('Owner'))  return 'Owner'
  if (roles.includes('Master')) return 'Master'
  return 'Client'
}
