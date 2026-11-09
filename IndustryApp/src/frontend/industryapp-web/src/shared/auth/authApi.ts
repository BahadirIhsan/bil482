import { client } from '../http/axiosClient'
import type { AuthResponse, Me } from './types'

// UC-01 · Oturum uç noktaları (endpoint 3, 5, 6). Refresh (4) axiosClient içindedir.
export const authApi = {
  login: async (email: string, password: string): Promise<AuthResponse> =>
    (await client.post<AuthResponse>('/auth/login', { email, password })).data,

  logout: async (refreshToken: string): Promise<void> => {
    await client.post('/auth/logout', { refreshToken })
  },

  me: async (): Promise<Me> => (await client.get<Me>('/account/me')).data,
}
