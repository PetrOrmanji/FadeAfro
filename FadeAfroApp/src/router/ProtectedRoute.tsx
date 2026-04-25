import { ReactNode } from 'react'

interface ProtectedRouteProps {
  roles: string[]
  children: ReactNode
}

export function ProtectedRoute({ children }: ProtectedRouteProps) {
  return <>{children}</>
}
