import axios, { AxiosError, type AxiosRequestConfig, type InternalAxiosRequestConfig } from 'axios'
import { authEvents } from '../auth/authEvents'
import { tokenStorage } from '../auth/tokenStorage'
import type { AuthResponse } from '../auth/types'
import { ApiError } from '../errors/ApiError'
import { GENERIC_ERROR, NETWORK_ERROR, messageFor } from '../errors/errorMessages'

const baseURL = import.meta.env.VITE_API_BASE_URL ?? '/api'

/** Tüm REST çağrılarının tek istemcisi. Model katmanı (…Api.ts) yalnızca bunu kullanır. */
export const client = axios.create({ baseURL, headers: { 'Content-Type': 'application/json' } })

/** Interceptor'sız istemci: refresh çağrısı kendi 401'ini tekrar yakalamasın diye. */
const bare = axios.create({ baseURL, headers: { 'Content-Type': 'application/json' } })

interface ProblemDetails {
  title?: string
  status?: number
  code?: string | null
  errors?: Record<string, string[]> | null
}

/** ProblemDetails.code'u Türkçe mesaja çevirip ApiError üretir; ham sunucu metni yalnızca alan hatalarında kullanılır. */
export function toApiError(error: unknown): ApiError {
  if (error instanceof ApiError) return error
  if (!axios.isAxiosError(error)) return new ApiError(GENERIC_ERROR, 0, null)

  const axiosError = error as AxiosError<ProblemDetails>
  if (!axiosError.response) return new ApiError(NETWORK_ERROR, 0, null)

  const { status, data } = axiosError.response
  const code = data?.code ?? null
  const fallback = status === 401 ? messageFor('TOKEN_EXPIRED') : status === 403 ? 'Bu işlem için yetkiniz yok.' : GENERIC_ERROR
  return new ApiError(messageFor(code, fallback), status, code, data?.errors ?? {})
}

// Aynı anda yalnızca bir refresh isteği uçar (eşzamanlı 401'ler, StrictMode'un çift effect'i). Refresh token
// her kullanımda yenilendiği için ikinci bir paralel istek eski token'la gider ve sunucu bunu çalıntı sayar.
let refreshing: Promise<string | null> | null = null

/** Refresh token ile yeni çift alır; başarısızsa oturumu temizler ve null döner. Eşzamanlı çağrılar tek isteği paylaşır. */
export function refreshAccessToken(): Promise<string | null> {
  refreshing ??= doRefresh().finally(() => {
    refreshing = null
  })
  return refreshing
}

async function doRefresh(): Promise<string | null> {
  const refreshToken = tokenStorage.getRefreshToken()
  if (!refreshToken) return null
  try {
    const { data } = await bare.post<AuthResponse>('/auth/refresh', { refreshToken })
    tokenStorage.setAccessToken(data.accessToken)
    tokenStorage.setRefreshToken(data.refreshToken)
    authEvents.emit('refreshed', data)
    return data.accessToken
  } catch {
    tokenStorage.clear()
    return null
  }
}

client.interceptors.request.use((config) => {
  const token = tokenStorage.getAccessToken()
  if (token) config.headers.Authorization = `Bearer ${token}`
  return config
})

type RetriableConfig = InternalAxiosRequestConfig & { _retried?: boolean }

// 401'de tek bir refresh isteği atılır; aynı anda gelen diğer 401'ler aynı sözü bekler ve yeni token'la tekrarlanır (T-14.6).
client.interceptors.response.use(undefined, async (error: unknown) => {
  if (!axios.isAxiosError(error)) throw toApiError(error)

  const original = error.config as RetriableConfig | undefined
  const isAuthCall = original?.url?.startsWith('/auth/') ?? false
  if (error.response?.status !== 401 || !original || original._retried || isAuthCall) throw toApiError(error)

  original._retried = true
  const token = await refreshAccessToken()
  if (!token) {
    authEvents.emit('logout')
    throw toApiError(error)
  }

  original.headers.Authorization = `Bearer ${token}`
  return client(original as AxiosRequestConfig)
})
