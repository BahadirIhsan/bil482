import { useQuery } from '@tanstack/react-query'
import { Alert, Chip, Paper, Stack, Typography } from '@mui/material'
import { mechanicStatusLabels, roleLabels } from '../../../shared/auth'
import { authApi } from '../../../shared/auth/authApi'
import { labelOf } from '../../../shared/format'
import { PageLoader } from '../../../shared/ui/PageLoader'
import { errorText } from '../../selection/model/errorText'

/**
 * Profil (route /profile, view #5). v0.2'de GET /api/account/me ile salt okunur; profil düzenleme formları
 * (endpoint 8, 9) v0.4'te eklenir.
 */
export function ProfileView() {
  const me = useQuery({ queryKey: ['account', 'me'], queryFn: authApi.me })

  if (me.isPending) return <PageLoader />
  if (me.isError) return <Alert severity="error">{errorText(me.error, 'Profil yüklenemedi.')}</Alert>

  const rows: [string, string][] = [
    ['Ad soyad', me.data.fullName || '-'],
    ['E-posta', me.data.email || '-'],
    ['Telefon', me.data.phone || '-'],
  ]

  return (
    <Stack spacing={2} sx={{ maxWidth: 560 }}>
      <Typography variant="h5" component="h1">
        Profil
      </Typography>
      <Paper variant="outlined" sx={{ p: 2 }}>
        <Stack spacing={1}>
          {rows.map(([label, value]) => (
            <Typography key={label}>
              <strong>{label}:</strong> {value}
            </Typography>
          ))}
          <Stack direction="row" spacing={1} alignItems="center">
            <Typography component="span">
              <strong>Rol:</strong>
            </Typography>
            {me.data.roles.map((role) => (
              <Chip key={role} size="small" label={labelOf(roleLabels, role)} />
            ))}
            {me.data.mechanicStatus && (
              <Chip size="small" variant="outlined" label={labelOf(mechanicStatusLabels, me.data.mechanicStatus)} />
            )}
          </Stack>
        </Stack>
      </Paper>
      <Alert severity="info">Profil bilgilerini düzenleme v0.4 sürümünde eklenecek.</Alert>
    </Stack>
  )
}
