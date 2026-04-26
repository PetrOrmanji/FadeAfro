import { useEffect } from 'react'
import { Outlet, useNavigate, useLocation } from 'react-router-dom'
import { Spinner } from '@telegram-apps/telegram-ui'
import { AuthProvider } from '@/context/AuthContext'
import { useAuth } from '@/hooks/useAuth'

function AuthGate() {
  const { isLoading, role } = useAuth()
  const navigate = useNavigate()
  const location = useLocation()

  useEffect(() => {
    if (isLoading) return

    if (role === 'Owner' && !location.pathname.startsWith('/owner')) {
      navigate('/owner', { replace: true })
    }

    if (role === 'Master' && !location.pathname.startsWith('/master')) {
      navigate('/master', { replace: true })
    }
  }, [isLoading, role, location.pathname, navigate])

  if (isLoading) {
    return (
      <div style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: '100dvh' }}>
        <Spinner size="l" />
      </div>
    )
  }

  return <Outlet />
}

export function AppLayout() {
  return (
    <AuthProvider>
      <AuthGate />
    </AuthProvider>
  )
}
