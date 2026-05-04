import { createContext, useContext, useEffect, useState, type ReactNode } from 'react'
import { useRawInitData } from '@tma.js/sdk-react'
import { login } from '../api/auth'
import { getMockInitData } from '../mock/telegramMock'
import { isRateLimitError, showRateLimitAlert } from '../api/errors'

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

export const AuthProvider = ({ children }: { children: ReactNode }) => {
  const [state, setState] = useState<AuthState>({
    isAuthenticated: false,
    roles: [],
    isLoading: true,
  })

  const rawInitData = useRawInitData()

  useEffect(() => {
    // DEV: используем мок initData (бэкенд пропускает валидацию через SkipValidation)
    // PROD: используем реальные данные от Telegram SDK
    const initData = DEV ? getMockInitData() : rawInitData
    if (!initData) return

    login(initData)
      .then((token) => {
        localStorage.setItem('token', token)
        const payload = JSON.parse(atob(token.split('.')[1]))
        const rawRoles = payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] ?? []
        const roles: Role[] = (Array.isArray(rawRoles) ? rawRoles : [rawRoles]) as Role[]
        setState({ isAuthenticated: true, roles, isLoading: false })
      })
      .catch((e: unknown) => {
        if (isRateLimitError(e)) showRateLimitAlert()
        setState({ isAuthenticated: false, roles: [], isLoading: false })
      })
  }, [rawInitData])

  return <AuthContext.Provider value={state}>{children}</AuthContext.Provider>
}
