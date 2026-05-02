import { createContext, useContext, useEffect, useState, type ReactNode } from 'react'
import { useRawInitData } from '@tma.js/sdk-react'
import { login } from '../api/auth'
import { getMockUser } from '../mock/telegramMock'

export type Role = 'Client' | 'Master' | 'Owner'

interface AuthState {
  isAuthenticated: boolean
  roles: Role[]
  isLoading: boolean
}

const AuthContext = createContext<AuthState>({
  isAuthenticated: false,
  roles: [],
  isLoading: true,
})

export const useAuth = () => useContext(AuthContext)

export const hasRole = (roles: Role[], role: Role) => roles.includes(role)

// ─────────────────────────────────────────────────────────────────────────────

const DEV = import.meta.env.DEV

// Роли мок-пользователей (Owner + Master — частный случай)
const MOCK_ROLES: Record<string, Role[]> = {
  Client: ['Client'],
  Master: ['Master'],
  Owner: ['Owner'],
  OwnerMaster: ['Owner', 'Master'],
}

export const AuthProvider = ({ children }: { children: ReactNode }) => {
  const [state, setState] = useState<AuthState>({
    isAuthenticated: false,
    roles: [],
    isLoading: true,
  })

  const initData = useRawInitData()

  useEffect(() => {
    // DEV: пропускаем backend, берём роли из мока
    if (DEV) {
      const mockUser = getMockUser()
      const roles = MOCK_ROLES[mockUser.role] ?? ['Client']
      setState({ isAuthenticated: true, roles, isLoading: false })
      return
    }

    // PROD: реальная авторизация через Telegram initData
    const raw = initData
    if (!raw) return

    login(raw)
      .then((token) => {
        localStorage.setItem('token', token)
        const payload = JSON.parse(atob(token.split('.')[1]))
        const rawRoles = payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] ?? []
        const roles: Role[] = (Array.isArray(rawRoles) ? rawRoles : [rawRoles]) as Role[]
        setState({ isAuthenticated: true, roles, isLoading: false })
      })
      .catch(() => {
        setState({ isAuthenticated: false, roles: [], isLoading: false })
      })
  }, [initData])

  return <AuthContext.Provider value={state}>{children}</AuthContext.Provider>
}
