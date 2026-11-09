import type { AuthResponse } from './types'

type AuthEventMap = {
  /** Refresh başarısız oldu veya kullanıcı çıkış yaptı. */
  logout: undefined
  /** Interceptor sessizce yeni token aldı; AuthProvider kullanıcıyı günceller. */
  refreshed: AuthResponse
}

type Listener<K extends keyof AuthEventMap> = (payload: AuthEventMap[K]) => void

const listeners: { [K in keyof AuthEventMap]: Set<Listener<K>> } = {
  logout: new Set(),
  refreshed: new Set(),
}

/** axios katmanı ile React (AuthProvider) arasındaki küçük olay köprüsü. */
export const authEvents = {
  on<K extends keyof AuthEventMap>(event: K, listener: Listener<K>): () => void {
    listeners[event].add(listener)
    return () => {
      listeners[event].delete(listener)
    }
  },
  emit<K extends keyof AuthEventMap>(event: K, ...payload: AuthEventMap[K] extends undefined ? [] : [AuthEventMap[K]]) {
    listeners[event].forEach((listener) => listener(payload[0] as AuthEventMap[K]))
  },
}
