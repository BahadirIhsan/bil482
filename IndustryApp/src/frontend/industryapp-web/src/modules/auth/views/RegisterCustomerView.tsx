import { zodResolver } from '@hookform/resolvers/zod'
import { Controller, useForm } from 'react-hook-form'
import { Link as RouterLink, useNavigate } from 'react-router-dom'
import { Alert, Button, Grid, Link, Paper, Stack, TextField, Typography } from '@mui/material'
import { homePathFor } from '../../../shared/auth'
import { useLegalDocuments } from '../../catalog/controller/useCatalog'
import { errorText } from '../../selection/model/errorText'
import { LegalConsentField } from '../components/LegalConsentField'
import { useRegisterCustomer } from '../controller/useRegistration'
import { applyServerErrors } from '../model/applyServerErrors'
import { allAccepted } from '../model/consents'
import { registerCustomerSchema, type RegisterCustomerForm } from '../model/schemas'

const fields = ['fullName', 'email', 'phone', 'password', 'city', 'district', 'acceptedLegalDocumentIds'] as const

/** UC-01 · Müşteri kaydı (route /register/customer, view #3): ortak alanlar + il/ilçe + sözleşme onayları. */
export function RegisterCustomerView() {
  const navigate = useNavigate()
  const documents = useLegalDocuments('Customer')
  const register = useRegisterCustomer()

  const form = useForm<RegisterCustomerForm>({
    resolver: zodResolver(registerCustomerSchema),
    defaultValues: { fullName: '', email: '', phone: '', password: '', city: '', district: '', acceptedLegalDocumentIds: [] },
  })
  const { errors } = form.formState

  const onSubmit = form.handleSubmit((values) => {
    if (!allAccepted(documents.data, values.acceptedLegalDocumentIds)) {
      form.setError('acceptedLegalDocumentIds', { message: 'Tüm zorunlu sözleşmeler onaylanmalıdır.' })
      return
    }
    register.mutate(
      { ...values, district: values.district || null },
      {
        onSuccess: (user) => navigate(homePathFor(user), { replace: true }),
        onError: (e) => applyServerErrors(e, fields, form.setError),
      },
    )
  })

  return (
    <Paper variant="outlined" sx={{ p: { xs: 2, sm: 4 }, maxWidth: 640, mx: 'auto' }}>
      <Stack component="form" spacing={2.5} onSubmit={onSubmit} noValidate>
        <Typography variant="h5" component="h1">
          Müşteri kaydı
        </Typography>
        {register.isError && <Alert severity="error">{errorText(register.error)}</Alert>}
        <Grid container spacing={2}>
          <Grid item xs={12}>
            <TextField fullWidth label="Ad soyad" autoComplete="name" {...form.register('fullName')}
              error={Boolean(errors.fullName)} helperText={errors.fullName?.message} />
          </Grid>
          <Grid item xs={12} sm={6}>
            <TextField fullWidth label="E-posta" type="email" autoComplete="email" {...form.register('email')}
              error={Boolean(errors.email)} helperText={errors.email?.message} />
          </Grid>
          <Grid item xs={12} sm={6}>
            <TextField fullWidth label="Telefon" placeholder="05XXXXXXXXX" autoComplete="tel" inputMode="numeric" {...form.register('phone')}
              error={Boolean(errors.phone)} helperText={errors.phone?.message} />
          </Grid>
          <Grid item xs={12}>
            <TextField fullWidth label="Şifre" type="password" autoComplete="new-password" {...form.register('password')}
              error={Boolean(errors.password)} helperText={errors.password?.message ?? 'En az 8 karakter, harf ve rakam.'} />
          </Grid>
          <Grid item xs={12} sm={6}>
            <TextField fullWidth label="İl" {...form.register('city')} error={Boolean(errors.city)} helperText={errors.city?.message} />
          </Grid>
          <Grid item xs={12} sm={6}>
            <TextField fullWidth label="İlçe (isteğe bağlı)" {...form.register('district')}
              error={Boolean(errors.district)} helperText={errors.district?.message} />
          </Grid>
        </Grid>
        <Controller
          control={form.control}
          name="acceptedLegalDocumentIds"
          render={({ field, fieldState }) => (
            <LegalConsentField
              documents={documents.data}
              loading={documents.isPending}
              value={field.value}
              onChange={field.onChange}
              error={fieldState.error?.message}
            />
          )}
        />
        <Button type="submit" variant="contained" size="large" disabled={register.isPending}>
          Kaydol
        </Button>
        <Typography variant="body2" color="text.secondary">
          Zaten hesabınız var mı?{' '}
          <Link component={RouterLink} to="/login">
            Giriş yapın
          </Link>
        </Typography>
      </Stack>
    </Paper>
  )
}
