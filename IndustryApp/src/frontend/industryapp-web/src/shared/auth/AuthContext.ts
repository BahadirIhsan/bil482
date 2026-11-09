import { createContext, useContext } from 'react'
import type { AuthUser } from './types'

export type AuthStatus = 'loading' | 'authenticated' | 'anonymous'

export interface AuthContextValue {
  status: AuthStatus
  user: AuthUser | null
  login: (email: string, password: string) => Promise<AuthUser>
  logout: () => Promise<void>
}

export const AuthContext = createContext<AuthContextValue | null>(null)

/** Oturumdaki kullanıcı ve giriş/çıkış işlemleri. AuthProvider dışında kullanılamaz. */
export function useAuth(): AuthContextValue {
  const value = useContext(AuthContext)
  if (!value) throw new Error('useAuth, AuthProvider içinde kullanılmalıdır.')
  return value
}
