import { Outlet } from 'react-router-dom'
import { Spinner } from '@telegram-apps/telegram-ui'
import { AuthProvider } from '@/context/AuthContext'
import { useAuth } from '@/hooks/useAuth'

function AuthGate() {
  const { isLoading } = useAuth()

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
