import { Alert, AlertTitle, Button, Stack } from '@mui/material'
import { formatRemaining, useCountdown } from '../controller/useCountdown'

interface SelectionBannerProps {
  undoDeadline: string | null
  /** Sunucunun söylediği: kullanıcı rapor sahibi ve seçim hâlâ geri alınabilir. */
  canUndo: boolean
  undoing: boolean
  onUndo: () => void
  onGoToMatch: () => void
}

/**
 * FR-04.3 · Seçim sonrası bant: geri alma süresine geri sayım, "Vazgeç" ve "Eşleşmeye git".
 * Sayaç sıfıra inince "Vazgeç" kaybolur; süre kontrolünün asıl yeri yine sunucudaki State'tir.
 */
export function SelectionBanner({ undoDeadline, canUndo, undoing, onUndo, onGoToMatch }: SelectionBannerProps) {
  const remaining = useCountdown(undoDeadline)
  const undoOpen = canUndo && remaining > 0

  return (
    <Alert
      severity="success"
      action={
        <Stack direction={{ xs: 'column', sm: 'row' }} spacing={1}>
          {undoOpen && (
            <Button color="inherit" size="small" variant="outlined" disabled={undoing} onClick={onUndo}>
              Vazgeç
            </Button>
          )}
          <Button color="inherit" size="small" variant="contained" onClick={onGoToMatch}>
            Eşleşmeye git
          </Button>
        </Stack>
      }
    >
      <AlertTitle>Seçiminiz kaydedildi</AlertTitle>
      {undoOpen
        ? `Seçimi geri almak için kalan süre: ${formatRemaining(remaining)}`
        : 'Geri alma süresi doldu. Ustayla eşleşme ekranından iletişime geçebilirsiniz.'}
    </Alert>
  )
}
