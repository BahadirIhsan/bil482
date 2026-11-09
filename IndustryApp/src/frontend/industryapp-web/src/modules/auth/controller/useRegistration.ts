import { useMutation } from '@tanstack/react-query'
import { useAuth } from '../../../shared/auth'
import { registrationApi } from '../model/registrationApi'
import type { RegisterCustomerBody, RegisterMechanicBody } from '../model/types'

/** UC-01 · Müşteri kaydı; başarıda aynı bilgilerle oturum açılır ve kullanıcı döner. */
export function useRegisterCustomer() {
  const { login } = useAuth()
  return useMutation({
    mutationFn: async (body: RegisterCustomerBody) => {
      await registrationApi.registerCustomer(body)
      return login(body.email, body.password)
    },
  })
}

/** UC-01 · Usta kaydı; hesap onay bekler, giriş sonrası /mechanic/pending'e yönlenir. */
export function useRegisterMechanic() {
  const { login } = useAuth()
  return useMutation({
    mutationFn: async (body: RegisterMechanicBody) => {
      await registrationApi.registerMechanic(body)
      return login(body.email, body.password)
    },
  })
}
