import { z } from 'zod'

/**
 * Form şemaları backend'deki FluentValidation kurallarıyla aynıdır (RegisterValidators.cs). İstemci
 * doğrulaması yalnızca kolaylıktır; asıl kontrol sunucudadır.
 */
const fullName = z.string().trim().min(1, 'Ad soyad zorunludur.').max(150, 'Ad soyad en fazla 150 karakter olabilir.')
const email = z.string().trim().min(1, 'E-posta zorunludur.').pipe(z.email('Geçerli bir e-posta adresi girin.'))
const phone = z.string().trim().regex(/^05\d{9}$/, 'Telefon 05XXXXXXXXX biçiminde olmalıdır.')
const password = z
  .string()
  .min(8, 'Şifre en az 8 karakter olmalıdır.')
  .regex(/\d/, 'Şifre en az bir rakam içermelidir.')
  .regex(/\p{L}/u, 'Şifre en az bir harf içermelidir.')
const consents = z.array(z.number()).min(1, 'Zorunlu sözleşmeler onaylanmalıdır.')

export const loginSchema = z.object({
  email: z.string().trim().min(1, 'E-posta zorunludur.'),
  password: z.string().min(1, 'Şifre zorunludur.'),
})
export type LoginForm = z.infer<typeof loginSchema>

export const registerCustomerSchema = z.object({
  fullName,
  email,
  phone,
  password,
  city: z.string().trim().min(1, 'İl zorunludur.').max(50),
  district: z.string().trim().max(80),
  acceptedLegalDocumentIds: consents,
})
export type RegisterCustomerForm = z.infer<typeof registerCustomerSchema>

export const registerMechanicSchema = z.object({
  fullName,
  email,
  phone,
  password,
  workshopName: z.string().trim().min(1, 'Atölye adı zorunludur.').max(150),
  taxNumber: z.string().trim().regex(/^\d{10,11}$/, 'Vergi numarası 10 veya 11 haneli olmalıdır.'),
  city: z.string().trim().min(1, 'İl zorunludur.').max(50),
  district: z.string().trim().min(1, 'İlçe zorunludur.').max(80),
  address: z.string().trim().min(1, 'Adres zorunludur.').max(300),
  latitude: z.number({ error: 'Enlem girin.' }).min(-90, 'Enlem -90 ile 90 arasında olmalıdır.').max(90, 'Enlem -90 ile 90 arasında olmalıdır.'),
  longitude: z.number({ error: 'Boylam girin.' }).min(-180, 'Boylam -180 ile 180 arasında olmalıdır.').max(180, 'Boylam -180 ile 180 arasında olmalıdır.'),
  specialtyCategoryIds: z.array(z.number()).min(1, 'En az bir uzmanlık seçilmelidir.'),
  acceptedLegalDocumentIds: consents,
})
export type RegisterMechanicForm = z.infer<typeof registerMechanicSchema>
