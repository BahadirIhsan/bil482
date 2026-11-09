import { Button, Dialog, DialogActions, DialogContent, DialogContentText, DialogTitle } from '@mui/material'

export type ReportAction = 'complete' | 'cancel'

interface ReportActionDialogProps {
  action: ReportAction | null
  busy: boolean
  onConfirm: (action: ReportAction) => void
  onClose: () => void
}

const texts: Record<ReportAction, { title: string; body: string; confirm: string }> = {
  complete: {
    title: 'İşi tamamla',
    body: 'Onarımın tamamlandığını onaylıyor musunuz? Bu işlem geri alınamaz ve usta bilgilendirilir.',
    confirm: 'Tamamlandı',
  },
  cancel: {
    title: 'Raporu iptal et',
    body: 'Raporu iptal etmek istediğinize emin misiniz? Bekleyen teklifler kapanır ve ustalar bilgilendirilir.',
    confirm: 'İptal et',
  },
}

/** FR-04.5 · Tamamlama ve iptal onayı. */
export function ReportActionDialog({ action, busy, onConfirm, onClose }: ReportActionDialogProps) {
  const text = action ? texts[action] : null
  return (
    <Dialog open={action !== null} onClose={busy ? undefined : onClose} maxWidth="xs" fullWidth>
      <DialogTitle>{text?.title}</DialogTitle>
      <DialogContent>
        <DialogContentText>{text?.body}</DialogContentText>
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose} disabled={busy}>
          Vazgeç
        </Button>
        <Button
          variant="contained"
          color={action === 'cancel' ? 'error' : 'primary'}
          disabled={busy || action === null}
          onClick={() => action && onConfirm(action)}
        >
          {text?.confirm}
        </Button>
      </DialogActions>
    </Dialog>
  )
}
