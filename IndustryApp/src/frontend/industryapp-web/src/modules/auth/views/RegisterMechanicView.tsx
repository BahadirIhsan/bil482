import { zodResolver } from '@hookform/resolvers/zod'
import { useState } from 'react'
import { Controller, useForm } from 'react-hook-form'
import { Link as RouterLink, useNavigate } from 'react-router-dom'
import {
  Alert,
  Button,
  Checkbox,
  CircularProgress,
  FormControl,
  FormControlLabel,
  FormGroup,
  FormHelperText,
  FormLabel,
  Grid,
  Link,
  Paper,
  Stack,
  TextField,
  Typography,
} from '@mui/material'
import MyLocationIcon from '@mui/icons-material/MyLocation'
import { homePathFor } from '../../../shared/auth'
import { useDamageCategories, useLegalDocuments } from '../../catalog/controller/useCatalog'
import { errorText } from '../../selection/model/errorText'
import { LegalConsentField } from '../components/LegalConsentField'
import { useRegisterMechanic } from '../controller/useRegistration'
import { applyServerErrors } from '../model/applyServerErrors'
import { allAccepted } from '../model/consents'
import { registerMechanicSchema, type RegisterMechanicForm } from '../model/schemas'

const fields = [
  'fullName', 'email', 'phone', 'password', 'workshopName', 'taxNumber', 'city', 'district', 'address',
  'latitude', 'longitude', 'specialtyCategoryIds', 'acceptedLegalDocumentIds',
] as const

/**
 * UC-01 · Usta kaydı (route /register/mechanic, view #4): ortak alanlar + atölye, vergi no, adres, konum
 * ("Konumumu kullan"), uzmanlıklar (hasar ağacının 1. seviyesi) ve sözleşmeler. Hesap admin onayı bekler.
 */
export function RegisterMechanicView() {
  const navigate = useNavigate()
  const categories = useDamageCategories()
  const documents = useLegalDocuments('Mechanic')
  const register = useRegisterMechanic()
  const [locating, setLocating] = useState(false)
  const [locationError, setLocationError] = useState<string | null>(null)

  const form = useForm<RegisterMechanicForm>({
    resolver: zodResolver(registerMechanicSchema),
    defaultValues: {
      fullName: '', email: '', phone: '', password: '', workshopName: '', taxNumber: '', city: '', district: '',
      address: '', specialtyCategoryIds: [], acceptedLegalDocumentIds: [],
    },
  })
  const { errors } = form.formState

  const fillMyLocation = () => {
    if (!('geolocation' in navigator)) {
      setLocationError('Tarayıcınız konum paylaşımını desteklemiyor; enlem ve boylamı elle girin.')
      return
    }
    setLocating(true)
    setLocationError(null)
    navigator.geolocation.getCurrentPosition(
      (position) => {
        form.setValue('latitude', Number(position.coords.latitude.toFixed(6)), { shouldValidate: true })
        form.setValue('longitude', Number(position.coords.longitude.toFixed(6)), { shouldValidate: true })
        setLocating(false)
      },
      () => {
        setLocationError('Konum alınamadı; enlem ve boylamı elle girin.')
        setLocating(false)
      },
      { enableHighAccuracy: true, timeout: 10_000 },
    )
  }

  const onSubmit = form.handleSubmit((values) => {
    if (!allAccepted(documents.data, values.acceptedLegalDocumentIds)) {
      form.setError('acceptedLegalDocumentIds', { message: 'Tüm zorunlu sözleşmeler onaylanmalıdır.' })
      return
    }
    register.mutate(values, {
      onSuccess: (user) => navigate(homePathFor(user), { replace: true }),
      onError: (e) => applyServerErrors(e, fields, form.setError),
    })
  })

  const text = (name: Exclude<(typeof fields)[number], 'latitude' | 'longitude' | 'specialtyCategoryIds' | 'acceptedLegalDocumentIds'>) => ({
    ...form.register(name),
    error: Boolean(errors[name]),
    helperText: errors[name]?.message,
  })

  return (
    <Paper variant="outlined" sx={{ p: { xs: 2, sm: 4 }, maxWidth: 720, mx: 'auto' }}>
      <Stack component="form" spacing={2.5} onSubmit={onSubmit} noValidate>
        <Typography variant="h5" component="h1">
          Usta kaydı
        </Typography>
        <Alert severity="info">Hesabınız yönetici onayından sonra iş bildirimi almaya başlar.</Alert>
        {register.isError && <Alert severity="error">{errorText(register.error)}</Alert>}

        <Grid container spacing={2}>
          <Grid item xs={12}>
            <TextField fullWidth label="Ad soyad" autoComplete="name" {...text('fullName')} />
          </Grid>
          <Grid item xs={12} sm={6}>
            <TextField fullWidth label="E-posta" type="email" autoComplete="email" {...text('email')} />
          </Grid>
          <Grid item xs={12} sm={6}>
            <TextField fullWidth label="Telefon" placeholder="05XXXXXXXXX" autoComplete="tel" inputMode="numeric" {...text('phone')} />
          </Grid>
          <Grid item xs={12}>
            <TextField fullWidth label="Şifre" type="password" autoComplete="new-password" {...text('password')}
              helperText={errors.password?.message ?? 'En az 8 karakter, harf ve rakam.'} />
          </Grid>
          <Grid item xs={12} sm={7}>
            <TextField fullWidth label="Atölye adı" {...text('workshopName')} />
          </Grid>
          <Grid item xs={12} sm={5}>
            <TextField fullWidth label="Vergi numarası" inputMode="numeric" {...text('taxNumber')} />
          </Grid>
          <Grid item xs={12} sm={6}>
            <TextField fullWidth label="İl" {...text('city')} />
          </Grid>
          <Grid item xs={12} sm={6}>
            <TextField fullWidth label="İlçe" {...text('district')} />
          </Grid>
          <Grid item xs={12}>
            <TextField fullWidth multiline minRows={2} label="Atölye adresi" {...text('address')} />
          </Grid>
          <Grid item xs={6} sm={4}>
            <TextField fullWidth label="Enlem" type="number" inputProps={{ step: 'any' }} InputLabelProps={{ shrink: true }}
              {...form.register('latitude', { valueAsNumber: true })}
              error={Boolean(errors.latitude)} helperText={errors.latitude?.message} />
          </Grid>
          <Grid item xs={6} sm={4}>
            <TextField fullWidth label="Boylam" type="number" inputProps={{ step: 'any' }} InputLabelProps={{ shrink: true }}
              {...form.register('longitude', { valueAsNumber: true })}
              error={Boolean(errors.longitude)} helperText={errors.longitude?.message} />
          </Grid>
          <Grid item xs={12} sm={4} sx={{ display: 'flex', alignItems: 'flex-start' }}>
            <Button fullWidth variant="outlined" startIcon={locating ? <CircularProgress size={16} /> : <MyLocationIcon />}
              onClick={fillMyLocation} disabled={locating} sx={{ height: 56 }}>
              Konumumu kullan
            </Button>
          </Grid>
          {locationError && (
            <Grid item xs={12}>
              <Alert severity="warning">{locationError}</Alert>
            </Grid>
          )}
        </Grid>

        <Controller
          control={form.control}
          name="specialtyCategoryIds"
          render={({ field, fieldState }) => (
            <FormControl error={Boolean(fieldState.error)} component="fieldset">
              <FormLabel component="legend">Uzmanlık alanları</FormLabel>
              {categories.isPending ? (
                <CircularProgress size={20} aria-label="Kategoriler yükleniyor" />
              ) : categories.isError ? (
                <Alert severity="warning">Hasar kategorileri yüklenemedi.</Alert>
              ) : (
                <FormGroup row>
                  {(categories.data ?? []).map((category) => (
                    <FormControlLabel
                      key={category.id}
                      label={category.name}
                      control={
                        <Checkbox
                          checked={field.value.includes(category.id)}
                          onChange={(e) =>
                            field.onChange(
                              e.target.checked
                                ? [...field.value, category.id]
                                : field.value.filter((id: number) => id !== category.id),
                            )
                          }
                        />
                      }
                    />
                  ))}
                </FormGroup>
              )}
              {fieldState.error && <FormHelperText>{fieldState.error.message}</FormHelperText>}
            </FormControl>
          )}
        />

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
