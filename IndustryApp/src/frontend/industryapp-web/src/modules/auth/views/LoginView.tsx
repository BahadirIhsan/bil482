import { zodResolver } from '@hookform/resolvers/zod'
import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { Link as RouterLink, Navigate, useLocation, useNavigate, type Location } from 'react-router-dom'
import { Alert, Button, Link, Paper, Stack, TextField, Typography } from '@mui/material'
import { homePathFor, useAuth } from '../../../shared/auth'
import { errorText } from '../../selection/model/errorText'
import { loginSchema, type LoginForm } from '../model/schemas'

/** UC-01 · Giriş (route /login, view #1). Başarıda role göre yönlendirilir (T-14.3). */
export function LoginView() {
  const { status, user, login } = useAuth()
  const navigate = useNavigate()
  const location = useLocation()
  const from = (location.state as { from?: Location } | null)?.from
  const [error, setError] = useState<unknown>(null)

  const form = useForm<LoginForm>({ resolver: zodResolver(loginSchema), defaultValues: { email: '', password: '' } })
  const { errors, isSubmitting } = form.formState

  if (status === 'authenticated' && user) return <Navigate to={homePathFor(user)} replace />

  const onSubmit = form.handleSubmit(async (values) => {
    setError(null)
    try {
      const signedIn = await login(values.email, values.password)
      navigate(from ? `${from.pathname}${from.search}` : homePathFor(signedIn), { replace: true })
    } catch (e) {
      setError(e)
    }
  })

  return (
    <Paper variant="outlined" sx={{ p: { xs: 2, sm: 4 }, maxWidth: 420, mx: 'auto' }}>
      <Stack component="form" spacing={2.5} onSubmit={onSubmit} noValidate>
        <Typography variant="h5" component="h1">
          Giriş yap
        </Typography>
        {error !== null && <Alert severity="error">{errorText(error)}</Alert>}
        <TextField
          label="E-posta"
          type="email"
          autoComplete="email"
          autoFocus
          {...form.register('email')}
          error={Boolean(errors.email)}
          helperText={errors.email?.message}
        />
        <TextField
          label="Şifre"
          type="password"
          autoComplete="current-password"
          {...form.register('password')}
          error={Boolean(errors.password)}
          helperText={errors.password?.message}
        />
        <Button type="submit" variant="contained" size="large" disabled={isSubmitting}>
          Giriş yap
        </Button>
        <Typography variant="body2" color="text.secondary">
          Hesabınız yok mu?{' '}
          <Link component={RouterLink} to="/register">
            Kaydolun
          </Link>
        </Typography>
      </Stack>
    </Paper>
  )
}
