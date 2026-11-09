import { Navigate } from 'react-router-dom'
import { Alert, Button, Stack, Typography } from '@mui/material'
import { mechanicStatusLabels, useAuth } from '../../../shared/auth'
import { labelOf } from '../../../shared/format'

/**
 * Onaysız usta ekranı (route /mechanic/pending, view #14). Onay token'a giriş sırasında yazıldığı için
 * yönetici onayından sonra çıkış yapıp tekrar girmek gerekir. Ret gerekçesi v0.3'te gösterilir.
 */
export function MechanicPendingApprovalView() {
  const { user, logout } = useAuth()
  if (user?.mechanicStatus === 'Approved') return <Navigate to="/mechanic" replace />

  return (
    <Stack spacing={2} sx={{ maxWidth: 560 }}>
      <Typography variant="h5" component="h1">
        Hesap durumu: {labelOf(mechanicStatusLabels, user?.mechanicStatus)}
      </Typography>
      {user?.mechanicStatus === 'Rejected' || user?.mechanicStatus === 'Suspended' ? (
        <Alert severity="error">Hesabınız şu an iş bildirimi alamıyor. Ayrıntı için yöneticiyle iletişime geçin.</Alert>
      ) : (
        <Alert severity="info">
          Hesabınız yönetici onayı bekliyor. Onaylandıktan sonra çıkış yapıp tekrar giriş yaptığınızda iş bildirimleri
          açılır.
        </Alert>
      )}
      <Button variant="outlined" onClick={() => void logout()} sx={{ alignSelf: 'flex-start' }}>
        Çıkış yap
      </Button>
    </Stack>
  )
}
