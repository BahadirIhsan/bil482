import { client } from '../../../shared/http/axiosClient'
import type { RegisterCustomerBody, RegisterMechanicBody, RegistrationResponse } from './types'

export const registrationApi = {
  registerCustomer: async (body: RegisterCustomerBody): Promise<RegistrationResponse> =>
    (await client.post<RegistrationResponse>('/auth/register/customer', body)).data,

  registerMechanic: async (body: RegisterMechanicBody): Promise<RegistrationResponse> =>
    (await client.post<RegistrationResponse>('/auth/register/mechanic', body)).data,
}
