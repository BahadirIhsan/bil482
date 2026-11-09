// UC-01 · Kayıt istekleri (POST /api/auth/register/customer, /register/mechanic — endpoint 1, 2).

export interface RegisterCustomerBody {
  fullName: string
  email: string
  phone: string
  password: string
  city: string
  district: string | null
  acceptedLegalDocumentIds: number[]
}

export interface RegisterMechanicBody {
  fullName: string
  email: string
  phone: string
  password: string
  workshopName: string
  taxNumber: string
  city: string
  district: string
  address: string
  latitude: number
  longitude: number
  specialtyCategoryIds: number[]
  acceptedLegalDocumentIds: number[]
}

export interface RegistrationResponse {
  userId: string
  role: 'Customer' | 'Mechanic'
  requiresApproval: boolean
}
