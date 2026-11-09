// UC-01 · Oturum tipleri (POST /api/auth/login, /refresh — endpoint 3, 4; GET /api/account/me — endpoint 6).

export type Role = 'Customer' | 'Mechanic' | 'Admin'

export type MechanicStatus = 'PendingApproval' | 'Approved' | 'Rejected' | 'Suspended'

export interface AuthUser {
  id: string
  fullName: string
  email: string
  roles: Role[]
  mechanicStatus: MechanicStatus | null
}

export interface AuthResponse {
  accessToken: string
  accessTokenExpiresAt: string
  refreshToken: string
  refreshTokenExpiresAt: string
  user: AuthUser
}

export interface Me extends AuthUser {
  phone: string
}

export const roleLabels: Record<Role, string> = {
  Customer: 'Müşteri',
  Mechanic: 'Usta',
  Admin: 'Yönetici',
}

export const mechanicStatusLabels: Record<MechanicStatus, string> = {
  PendingApproval: 'Onay bekliyor',
  Approved: 'Onaylı',
  Rejected: 'Reddedildi',
  Suspended: 'Askıya alındı',
}
