import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { Button, Paper, Stack, TextField, Typography } from '@mui/material'
import { useAuth } from '../../../shared/auth'

const GUID = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i

/**
 * Müşteri paneli (route /customer, view #9). Yeni rapor ve "Raporlarım" kısayolları (UC-02) ile rapor numarasıyla
 * detay açma. Panel özeti rol bazlı Abstract Factory ile v1.0'da gelir.
 */
export function CustomerDashboardView() {
  const { user } = useAuth()
  const navigate = useNavigate()
  const [reportId, setReportId] = useState('')
  const valid = GUID.test(reportId.trim())

  return (
    <Stack spacing={3} sx={{ maxWidth: 640 }}>
      <Typography variant="h5" component="h1">
        Hoş geldiniz{user?.fullName ? `, ${user.fullName}` : ''}
      </Typography>
      <Stack direction={{ xs: 'column', sm: 'row' }} spacing={1.5}>
        <Button variant="contained" onClick={() => navigate('/customer/reports/new')}>
          Yeni kaza raporu
        </Button>
        <Button variant="outlined" onClick={() => navigate('/customer/reports')}>
          Raporlarım
        </Button>
      </Stack>
      <Paper
        variant="outlined"
        component="form"
        sx={{ p: 2 }}
        onSubmit={(e) => {
          e.preventDefault()
          if (valid) navigate(`/customer/reports/${reportId.trim()}`)
        }}
      >
        <Stack direction={{ xs: 'column', sm: 'row' }} spacing={1.5}>
          <TextField
            fullWidth
            size="small"
            label="Rapor numarası"
            placeholder="ör. 3f2a6c1e-…"
            value={reportId}
            onChange={(e) => setReportId(e.target.value)}
            error={reportId.length > 0 && !valid}
            helperText={reportId.length > 0 && !valid ? 'Geçerli bir rapor numarası girin.' : ' '}
          />
          <Button type="submit" variant="contained" disabled={!valid} sx={{ alignSelf: { sm: 'flex-start' }, height: 40 }}>
            Raporu aç
          </Button>
        </Stack>
      </Paper>
    </Stack>
  )
}
