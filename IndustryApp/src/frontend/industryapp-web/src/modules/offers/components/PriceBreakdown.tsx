import { Table, TableBody, TableCell, TableRow } from '@mui/material'
import { formatMoney, labelOf } from '../../../shared/format'
import { priceItemLabels, type OfferPriceLine } from '../model/types'

interface PriceBreakdownProps {
  lines: OfferPriceLine[]
  total: number
  currency: string
}

/** Teklifin fiyat kırılımı; toplam her zaman kalemlerin toplamıdır (PAT-03.4). */
export function PriceBreakdown({ lines, total, currency }: PriceBreakdownProps) {
  return (
    <Table size="small" aria-label="Fiyat kırılımı">
      <TableBody>
        {lines.map((line, index) => (
          <TableRow key={`${line.itemType}-${index}`}>
            <TableCell>{labelOf(priceItemLabels, line.itemType)}</TableCell>
            <TableCell sx={{ color: 'text.secondary' }}>{line.description || '-'}</TableCell>
            <TableCell align="right">{formatMoney(line.amount, currency)}</TableCell>
          </TableRow>
        ))}
        <TableRow>
          <TableCell colSpan={2} sx={{ fontWeight: 600 }}>
            Toplam
          </TableCell>
          <TableCell align="right" sx={{ fontWeight: 600 }}>
            {formatMoney(total, currency)}
          </TableCell>
        </TableRow>
      </TableBody>
    </Table>
  )
}
