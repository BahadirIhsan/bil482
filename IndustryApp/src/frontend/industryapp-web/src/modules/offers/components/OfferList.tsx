import { useState } from 'react'
import { Box, Button, Card, CardActions, CardContent, Chip, Collapse, Stack, Typography } from '@mui/material'
import { formatDate, formatMoney, labelOf } from '../../../shared/format'
import { offerStatusLabels, type OfferListItem, type OfferStatus } from '../model/types'
import { PriceBreakdown } from './PriceBreakdown'

interface OfferListProps {
  offers: OfferListItem[]
  /** Rapor Open iken true; seçim butonları yalnızca o zaman görünür (karar yine sunucuda, State'te). */
  canSelect: boolean
  onSelect: (offer: OfferListItem) => void
  busy: boolean
}

const statusColor: Record<OfferStatus, 'default' | 'success' | 'warning'> = {
  Pending: 'warning',
  Accepted: 'success',
  NotSelected: 'default',
  Withdrawn: 'default',
  Closed: 'default',
}

/** FR-04.1 · Rapordaki teklifler, sunucudan gelen sırayla. */
export function OfferList({ offers, canSelect, onSelect, busy }: OfferListProps) {
  if (offers.length === 0) {
    return <Typography color="text.secondary">Henüz teklif yok. Yeni teklifler bu listeye kendiliğinden düşer.</Typography>
  }

  return (
    <Stack spacing={1.5}>
      {offers.map((offer) => (
        <OfferCard key={offer.id} offer={offer} canSelect={canSelect} onSelect={onSelect} busy={busy} />
      ))}
    </Stack>
  )
}

interface OfferCardProps {
  offer: OfferListItem
  canSelect: boolean
  onSelect: (offer: OfferListItem) => void
  busy: boolean
}

function OfferCard({ offer, canSelect, onSelect, busy }: OfferCardProps) {
  const [open, setOpen] = useState(false)
  const selectable = canSelect && offer.status === 'Pending'

  return (
    <Card variant="outlined" sx={offer.status === 'Accepted' ? { borderColor: 'success.main', borderWidth: 2 } : undefined}>
      <CardContent sx={{ pb: 1 }}>
        <Stack direction="row" justifyContent="space-between" alignItems="flex-start" spacing={2}>
          <Box>
            <Typography variant="subtitle1" fontWeight={600}>
              {offer.workshopName || '-'}
            </Typography>
            <Typography color="text.secondary">
              {formatMoney(offer.totalPrice, offer.currency)} · {offer.estimatedDays} gün
            </Typography>
            <Typography variant="caption" color="text.secondary">
              Teklif tarihi: {formatDate(offer.updatedAt ?? offer.createdAt, true)}
            </Typography>
          </Box>
          <Chip size="small" label={labelOf(offerStatusLabels, offer.status)} color={statusColor[offer.status]} />
        </Stack>
        {offer.note && (
          <Typography variant="body2" sx={{ mt: 1 }}>
            {offer.note}
          </Typography>
        )}
        <Collapse in={open} unmountOnExit>
          <Box sx={{ mt: 1 }}>
            <PriceBreakdown lines={offer.priceItems} total={offer.totalPrice} currency={offer.currency} />
          </Box>
        </Collapse>
      </CardContent>
      <CardActions sx={{ justifyContent: 'space-between', px: 2, pb: 2 }}>
        <Button size="small" onClick={() => setOpen((value) => !value)}>
          {open ? 'Kırılımı gizle' : 'Fiyat kırılımı'}
        </Button>
        {selectable && (
          <Button variant="contained" size="small" disabled={busy} onClick={() => onSelect(offer)}>
            Bu teklifi seç
          </Button>
        )}
      </CardActions>
    </Card>
  )
}
