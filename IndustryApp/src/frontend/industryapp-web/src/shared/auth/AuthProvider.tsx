import { useCallback, useEffect, useMemo, useState, type ReactNode } from 'react'
import { useQueryClient } from '@tanstack/react-query'
import { refreshAccessToken } from '../http/axiosClient'
import { authApi } from './authApi'
import { AuthContext, type AuthContextValue, type AuthStatus } from './AuthContext'
import { authEvents } from './authEvents'
import { tokenStorage } from './tokenStorage'
import type { AuthUser } from './types'

/**
 * UC-01 · Oturum durumu. Açılışta localStorage'daki refresh token ile sessizce oturum açılır; interceptor'ın
 * yenilediği token'lar ve başarısız refresh (logout) authEvents üzerinden buraya yansır.
 */
export function AuthProvider({ children }: { children: ReactNode }) {
  const queryClient = useQueryClient()
  const [status, setStatus] = useState<AuthStatus>(() => (tokenStorage.getRefreshToken() ? 'loading' : 'anonymous'))
  const [user, setUser] = useState<AuthUser | null>(null)

  useEffect(() => {
    const offRefreshed = authEvents.on('refreshed', (data) => {
      setUser(data.user)
      setStatus('authenticated')
    })
    const offLogout = authEvents.on('logout', () => {
      tokenStorage.clear()
      setUser(null)
      setStatus('anonymous')
      queryClient.clear()
    })
    return () => {
      offRefreshed()
      offLogout()
    }
  }, [queryClient])

  useEffect(() => {
    if (status !== 'loading') return
    void refreshAccessToken().then((token) => {
      if (!token) setStatus('anonymous') // başarılıysa 'refreshed' olayı kullanıcıyı ayarlar
    })
  }, [status])

  const login = useCallback(async (email: string, password: string) => {
    const data = await authApi.login(email, password)
    tokenStorage.setAccessToken(data.accessToken)
    tokenStorage.setRefreshToken(data.refreshToken)
    setUser(data.user)
    setStatus('authenticated')
    return data.user
  }, [])

  const logout = useCallback(async () => {
    const refreshToken = tokenStorage.getRefreshToken()
    if (refreshToken) await authApi.logout(refreshToken).catch(() => undefined)
    authEvents.emit('logout')
  }, [])

  const value = useMemo<AuthContextValue>(() => ({ status, user, login, logout }), [status, user, login, logout])
  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}
