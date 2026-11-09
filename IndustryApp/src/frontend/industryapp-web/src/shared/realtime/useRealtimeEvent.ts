import { useEffect, useRef } from 'react'
import { useRealtimeConnection } from './RealtimeContext'

/**
 * Bir SignalR olayını dinler (notification.created, offer.*, report.statusChanged). Handler her render'da
 * değişebilir; abonelik yalnızca bağlantı veya olay adı değişince yenilenir.
 */
export function useRealtimeEvent<T>(eventName: string, handler: (payload: T) => void): void {
  const connection = useRealtimeConnection()
  const ref = useRef(handler)
  ref.current = handler

  useEffect(() => {
    if (!connection) return
    const listener = (payload: T) => ref.current(payload)
    connection.on(eventName, listener)
    return () => connection.off(eventName, listener)
  }, [connection, eventName])
}
