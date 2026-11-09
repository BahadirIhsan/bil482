import { createContext, useContext } from 'react'
import type { HubConnection } from '@microsoft/signalr'

export const RealtimeContext = createContext<HubConnection | null>(null)

/** Açık SignalR bağlantısı; giriş yapılmamışsa veya bağlantı kurulmadıysa null. */
export const useRealtimeConnection = (): HubConnection | null => useContext(RealtimeContext)
