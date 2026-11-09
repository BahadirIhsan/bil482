import { useEffect, useState, type ReactNode } from 'react'
import { HubConnectionBuilder, LogLevel, type HubConnection } from '@microsoft/signalr'
import { useQueryClient } from '@tanstack/react-query'
import { useAuth } from '../auth/AuthContext'
import { tokenStorage } from '../auth/tokenStorage'
import { refreshAccessToken } from '../http/axiosClient'
import { RealtimeContext } from './RealtimeContext'

const hubUrl = import.meta.env.VITE_HUB_URL ?? '/hubs/notifications'

/**
 * T-12 · Giriş yapılınca /hubs/notifications bağlantısını kurar, çıkışta kapatır. Hub yalnızca sunucudan
 * yayın yapar; her işlem REST'ten geçer. Yeniden bağlanınca kaçırılmış olaylar için tüm sorgular yenilenir.
 */
export function RealtimeProvider({ children }: { children: ReactNode }) {
  const { status, user } = useAuth()
  const queryClient = useQueryClient()
  const [connection, setConnection] = useState<HubConnection | null>(null)
  const userId = user?.id

  useEffect(() => {
    if (status !== 'authenticated' || !userId) return

    const hub = new HubConnectionBuilder()
      .withUrl(hubUrl, {
        // WebSocket başlık gönderemez; token access_token sorgu parametresiyle gider (sunucu /hubs'ta okur).
        accessTokenFactory: async () => tokenStorage.getAccessToken() ?? (await refreshAccessToken()) ?? '',
      })
      .withAutomaticReconnect()
      .configureLogging(LogLevel.Warning)
      .build()

    hub.onreconnected(() => void queryClient.invalidateQueries())

    let cancelled = false
    hub
      .start()
      .then(() => {
        if (!cancelled) setConnection(hub)
      })
      .catch(() => {
        // Bağlantı kurulamazsa ekranlar REST ile çalışmaya devam eder; yalnızca canlı güncelleme olmaz.
      })

    return () => {
      cancelled = true
      setConnection(null)
      void hub.stop()
    }
  }, [status, userId, queryClient])

  return <RealtimeContext.Provider value={connection}>{children}</RealtimeContext.Provider>
}
