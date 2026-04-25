import { createContext, ReactNode, useState } from 'react'

interface AuthContextValue {
  isLoading: boolean
}

export const AuthContext = createContext<AuthContextValue>({ isLoading: false })

export function AuthProvider({ children }: { children: ReactNode }) {
  const [isLoading] = useState(false)

  return (
    <AuthContext.Provider value={{ isLoading }}>
      {children}
    </AuthContext.Provider>
  )
}
