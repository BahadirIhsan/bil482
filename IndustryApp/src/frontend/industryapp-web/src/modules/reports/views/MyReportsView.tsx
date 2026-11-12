import { useState } from 'react'
import { Link as RouterLink } from 'react-router-dom'
import { Alert, Box, Button, Card, CardActionArea, CardContent, Chip, CircularProgress, Pagination, Stack, Typography } from '@mui/material'
import AddIcon from '@mui/icons-material/Add'
import { formatDate, formatMoney, labelOf } from '../../../shared/format'
import { errorText } from '../../selection/model/errorText'
import { useMyReports } from '../controller/useMyReports'
import { reportStatusLabels, type ReportStatus } from '../model/types'

const statusColor: Record<ReportStatus, 'default' | 'primary' | 'success' | 'error' | 'warning'> = {
  Open: 'primary',
  OfferSelected: 'success',
  Completed: 'success',
  Cancelled: 'error',
  Expired: 'warning',
}

/** UC-02 · "Raporlarım" (route /customer/reports, view #10): en yeni önce, teklif sayısı ve en düşük teklifle. */
export function MyReportsView() {
  const [page, setPage] = useState(1)
  const reports = useMyReports(page)

  return (
    <Stack spacing={3}>
      <Stack direction="row" justifyContent="space-between" alignItems="center" spacing={2}>
        <Typography variant="h5" component="h1">
          Raporlarım
        </Typography>
        <Button component={RouterLink} to="/customer/reports/new" variant="contained" startIcon={<AddIcon />}>
          Yeni rapor
        </Button>
      </Stack>

      {reports.isPending ? (
        <Box sx={{ display: 'flex', justifyContent: 'center', py: 6 }}>
          <CircularProgress aria-label="Yükleniyor" />
        </Box>
      ) : reports.isError ? (
        <Alert severity="error">{errorText(reports.error, 'Raporlar yüklenemedi.')}</Alert>
      ) : reports.data.items.length === 0 ? (
        <Alert severity="info">Henüz raporunuz yok. Hasarı fotoğraflarla bildirmek için yeni bir rapor oluşturun.</Alert>
      ) : (
        <Stack spacing={1.5}>
          {reports.data.items.map((r) => (
            <Card key={r.id} variant="outlined">
              <CardActionArea component={RouterLink} to={`/customer/reports/${r.id}`}>
                <CardContent>
                  <Stack direction={{ xs: 'column', sm: 'row' }} justifyContent="space-between" spacing={1}>
                    <Box>
                      <Typography variant="subtitle1">
                        {r.city}
                        {r.district ? ` / ${r.district}` : ''} · {r.vehicleTypeName || '-'}
                      </Typography>
                      <Typography variant="body2" color="text.secondary">
                        Oluşturulma: {formatDate(r.createdAt, true)}
                        {r.status === 'Open' ? ` · Son teklif: ${formatDate(r.expiresAt, true)}` : ''}
                      </Typography>
                    </Box>
                    <Stack direction="row" spacing={1} alignItems="center" useFlexGap flexWrap="wrap">
                      <Chip size="small" label={`${r.offerCount} teklif`} variant="outlined" />
                      {r.lowestOfferPrice != null && (
                        <Chip size="small" label={`En düşük ${formatMoney(r.lowestOfferPrice)}`} variant="outlined" />
                      )}
                      <Chip size="small" label={labelOf(reportStatusLabels, r.status)} color={statusColor[r.status]} />
                    </Stack>
                  </Stack>
                </CardContent>
              </CardActionArea>
            </Card>
          ))}
          {reports.data.totalPages > 1 && (
            <Pagination page={page} count={reports.data.totalPages} onChange={(_, value) => setPage(value)} sx={{ alignSelf: 'center' }} />
          )}
        </Stack>
      )}
    </Stack>
  )
}
