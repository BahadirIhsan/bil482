import type { AuthUser, Role } from './types'

const priority: Role[] = ['Admin', 'Mechanic', 'Customer']

/** Birden çok rol varsa en yetkili olanı. */
export const primaryRole = (user: AuthUser | null): Role | null =>
  user ? (priority.find((role) => user.roles.includes(role)) ?? null) : null

/** Giriş sonrası yönlendirme (T-14.3): müşteri, onaylı/onaysız usta, admin. */
export function homePathFor(user: AuthUser | null): string {
  switch (primaryRole(user)) {
    case 'Customer':
      return '/customer'
    case 'Mechanic':
      return user?.mechanicStatus === 'Approved' ? '/mechanic' : '/mechanic/pending'
    case 'Admin':
      return '/admin'
    default:
      return '/login'
  }
}
