import { useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { Alert, Box, Button, Chip, CircularProgress, Divider, Paper, Stack, Typography } from '@mui/material'
import { formatDate, labelOf } from '../../../shared/format'
import { useReportOffers } from '../../offers/controller/useReportOffers'
import { OfferList } from '../../offers/components/OfferList'
import { SortSelect } from '../../offers/components/SortSelect'
import type { OfferListItem, OfferSortKey } from '../../offers/model/types'
import { ConfirmSelectDialog } from '../../selection/components/ConfirmSelectDialog'
import { SelectionBanner } from '../../selection/components/SelectionBanner'
import { useSelectionStatus, useSelectOffer, useUndoSelection } from '../../selection/controller/useSelection'
import { errorText } from '../../selection/model/errorText'
import { ReportActionDialog, type ReportAction } from '../components/ReportActionDialog'
import { useCancelReport, useCompleteReport, useCustomerReport } from '../controller/useCustomerReport'
import { reportStatusLabels, severityLabels, type ReportStatus } from '../model/types'

const statusColor: Record<ReportStatus, 'default' | 'primary' | 'success' | 'error' | 'warning'> = {
  Open: 'primary',
  OfferSelected: 'success',
  Completed: 'success',
  Cancelled: 'error',
  Expired: 'warning',
}

/**
 * UC-04 · Müşterinin rapor detay ekranı (route /customer/reports/:reportId, view #12).
 * Teklifleri sıralar, birini seçer (FR-04.1, FR-04.2), seçimi süre içinde geri alır (FR-04.3),
 * işi tamamlar veya raporu iptal eder (FR-04.5). Butonlar duruma göre görünür; asıl karar sunucuda.
 */
export function CustomerReportDetailView() {
  const { reportId = '' } = useParams()
  const navigate = useNavigate()
  const [sort, setSort] = useState<OfferSortKey>('price')
  const [offerToConfirm, setOfferToConfirm] = useState<OfferListItem | null>(null)
  const [action, setAction] = useState<ReportAction | null>(null)

  const report = useCustomerReport(reportId)
  const offers = useReportOffers(reportId, sort)
  const selection = useSelectionStatus(reportId)
  const selectOffer = useSelectOffer(reportId)
  const undoSelection = useUndoSelection(reportId)
  const complete = useCompleteReport(reportId)
  const cancel = useCancelReport(reportId)

  if (report.isPending) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', py: 8 }}>
        <CircularProgress aria-label="Yükleniyor" />
      </Box>
    )
  }

  if (report.isError || !report.data) {
    return <Alert severity="error">{errorText(report.error, 'Rapor yüklenemedi.')}</Alert>
  }

  const data = report.data
  const busy = selectOffer.isPending || undoSelection.isPending || complete.isPending || cancel.isPending
  const actionError = selectOffer.error ?? undoSelection.error ?? complete.error ?? cancel.error

  const confirmSelection = (offer: OfferListItem) =>
    selectOffer.mutate(offer.id, { onSettled: () => setOfferToConfirm(null) })

  const confirmAction = (value: ReportAction) =>
    (value === 'complete' ? complete : cancel).mutate(undefined, { onSettled: () => setAction(null) })

  return (
    <Stack spacing={3}>
      <Stack direction="row" justifyContent="space-between" alignItems="center" spacing={2}>
        <Typography variant="h5" component="h1">
          Rapor detayı
        </Typography>
        <Chip label={labelOf(reportStatusLabels, data.status)} color={statusColor[data.status]} />
      </Stack>

      {actionError && <Alert severity="error">{errorText(actionError)}</Alert>}

      {data.status === 'OfferSelected' && (
        <SelectionBanner
          undoDeadline={selection.data?.undoDeadline ?? data.undoDeadline}
          canUndo={selection.data?.canUndo ?? false}
          undoing={undoSelection.isPending}
          onUndo={() => undoSelection.mutate()}
          onGoToMatch={() => navigate(`/match/${reportId}`)}
        />
      )}

      <Paper variant="outlined" sx={{ p: 2 }}>
        <Stack spacing={0.5}>
          <Typography>
            <strong>Konum:</strong> {data.city}
            {data.district ? ` / ${data.district}` : ''}
          </Typography>
          <Typography>
            <strong>Araç türü:</strong> {data.vehicleTypeName || '-'}
          </Typography>
          <Typography>
            <strong>Oluşturulma:</strong> {formatDate(data.createdAt, true)}
          </Typography>
          {data.status === 'Open' && (
            <Typography>
              <strong>Teklif süresi sonu:</strong> {formatDate(data.expiresAt, true)}
            </Typography>
          )}
          {data.completedAt && (
            <Typography>
              <strong>Tamamlanma:</strong> {formatDate(data.completedAt, true)}
            </Typography>
          )}
          {data.cancelledAt && (
            <Typography>
              <strong>İptal:</strong> {formatDate(data.cancelledAt, true)}
            </Typography>
          )}
          <Typography>
            <strong>Açıklama:</strong> {data.description || '-'}
          </Typography>
        </Stack>
        <Divider sx={{ my: 1.5 }} />
        <Typography variant="subtitle2" gutterBottom>
          Hasar bölgeleri
        </Typography>
        {data.damageItems.length === 0 ? (
          <Typography color="text.secondary">-</Typography>
        ) : (
          <Stack direction="row" spacing={1} useFlexGap flexWrap="wrap">
            {data.damageItems.map((item) => (
              <Chip
                key={item.damageCategoryId}
                variant="outlined"
                label={`${item.categoryName || '-'} · ${labelOf(severityLabels, item.severity)}`}
              />
            ))}
          </Stack>
        )}
      </Paper>

      <Box>
        <Stack direction="row" justifyContent="space-between" alignItems="center" sx={{ mb: 1.5 }}>
          <Typography variant="h6" component="h2">
            Teklifler{offers.data ? ` (${offers.data.length})` : ''}
          </Typography>
          <SortSelect value={sort} onChange={setSort} />
        </Stack>
        {offers.isPending ? (
          <CircularProgress size={24} aria-label="Teklifler yükleniyor" />
        ) : offers.isError ? (
          <Alert severity="error">{errorText(offers.error, 'Teklifler yüklenemedi.')}</Alert>
        ) : (
          <OfferList
            offers={offers.data ?? []}
            canSelect={data.status === 'Open'}
            onSelect={setOfferToConfirm}
            busy={busy}
          />
        )}
      </Box>

      {(data.status === 'Open' || data.status === 'OfferSelected') && (
        <Stack direction="row" spacing={1.5}>
          {data.status === 'OfferSelected' && (
            <Button variant="contained" disabled={busy} onClick={() => setAction('complete')}>
              Tamamlandı
            </Button>
          )}
          <Button variant="outlined" color="error" disabled={busy} onClick={() => setAction('cancel')}>
            İptal et
          </Button>
        </Stack>
      )}

      <ConfirmSelectDialog
        offer={offerToConfirm}
        busy={selectOffer.isPending}
        onConfirm={confirmSelection}
        onClose={() => setOfferToConfirm(null)}
      />
      <ReportActionDialog action={action} busy={busy} onConfirm={confirmAction} onClose={() => setAction(null)} />
    </Stack>
  )
}
