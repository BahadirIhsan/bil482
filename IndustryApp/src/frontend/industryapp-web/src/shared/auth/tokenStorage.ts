/**
 * T-14.6 · Access token yalnızca bellekte, refresh token localStorage'da tutulur. Risk notu: localStorage XSS'e
 * karşı httpOnly cookie kadar güvenli değildir; ilk sürüm için bilinçli sadeleştirme (SDD §17 Future Work).
 */
const REFRESH_KEY = 'industryapp.refreshToken'

let accessToken: string | null = null

export const tokenStorage = {
  getAccessToken: (): string | null => accessToken,
  setAccessToken: (token: string | null) => {
    accessToken = token
  },
  getRefreshToken: (): string | null => {
    try {
      return window.localStorage.getItem(REFRESH_KEY)
    } catch {
      return null
    }
  },
  setRefreshToken: (token: string | null) => {
    try {
      if (token) window.localStorage.setItem(REFRESH_KEY, token)
      else window.localStorage.removeItem(REFRESH_KEY)
    } catch {
      // Gizli pencere vb.: oturum yalnızca bu sekmede yaşar.
    }
  },
  clear: () => {
    accessToken = null
    tokenStorage.setRefreshToken(null)
  },
}
