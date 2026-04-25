import { ReactNode } from 'react'
import { Navigate } from 'react-router-dom'
import { useAuth } from '@/hooks/useAuth'

interface ProtectedRouteProps {
  roles: string[]
  children: ReactNode
}

export function ProtectedRoute({ roles, children }: ProtectedRouteProps) {
  const { isLoading, role } = useAuth()

  if (isLoading) return null

  if (!role || !roles.includes(role)) {
    return <Navigate to="/" replace />
  }

  return <>{children}</>
}
