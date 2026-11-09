import { Button, Dialog, DialogActions, DialogContent, DialogContentText, DialogTitle } from '@mui/material'
import { formatMoney } from '../../../shared/format'
import type { OfferListItem } from '../../offers/model/types'

interface ConfirmSelectDialogProps {
  offer: OfferListItem | null
  busy: boolean
  onConfirm: (offer: OfferListItem) => void
  onClose: () => void
}

/** FR-04.2 · "Bu teklifi seç" onayı. Seçim sonrası iletişim bilgileri karşılıklı açılır. */
export function ConfirmSelectDialog({ offer, busy, onConfirm, onClose }: ConfirmSelectDialogProps) {
  return (
    <Dialog open={offer !== null} onClose={busy ? undefined : onClose} maxWidth="xs" fullWidth>
      <DialogTitle>Teklifi seç</DialogTitle>
      <DialogContent>
        {offer && (
          <DialogContentText>
            <strong>{offer.workshopName || '-'}</strong> ustasının {formatMoney(offer.totalPrice, offer.currency)} tutarındaki ve{' '}
            {offer.estimatedDays} günlük teklifini seçmek üzeresiniz. Diğer teklifler "Seçilmedi" olarak işaretlenir ve iletişim
            bilgileriniz seçtiğiniz ustayla paylaşılır. Seçiminizi kısa bir süre içinde geri alabilirsiniz.
          </DialogContentText>
        )}
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose} disabled={busy}>
          Vazgeç
        </Button>
        <Button variant="contained" disabled={busy || offer === null} onClick={() => offer && onConfirm(offer)}>
          Seçimi onayla
        </Button>
      </DialogActions>
    </Dialog>
  )
}
