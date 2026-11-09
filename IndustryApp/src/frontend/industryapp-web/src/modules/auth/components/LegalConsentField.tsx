import { useState } from 'react'
import {
  Alert,
  Button,
  Checkbox,
  CircularProgress,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  FormControl,
  FormControlLabel,
  FormHelperText,
  Link,
  Stack,
  Typography,
} from '@mui/material'
import type { LegalDocument } from '../../catalog/model/types'

interface LegalConsentFieldProps {
  documents: LegalDocument[] | undefined
  loading: boolean
  value: number[]
  onChange: (ids: number[]) => void
  error?: string
}

/** Kayıt formundaki zorunlu sözleşme onayları (FR-01.2, KVKK). Metin dialog'da okunur. */
export function LegalConsentField({ documents, loading, value, onChange, error }: LegalConsentFieldProps) {
  const [reading, setReading] = useState<LegalDocument | null>(null)

  if (loading) return <CircularProgress size={20} aria-label="Sözleşmeler yükleniyor" />
  if (!documents || documents.length === 0) return <Alert severity="warning">Sözleşmeler yüklenemedi.</Alert>

  const toggle = (id: number, checked: boolean) =>
    onChange(checked ? [...value.filter((v) => v !== id), id] : value.filter((v) => v !== id))

  return (
    <FormControl error={Boolean(error)} component="fieldset">
      <Stack spacing={0.5}>
        {documents.map((doc) => (
          <Stack key={doc.id} direction="row" alignItems="center" spacing={1}>
            <FormControlLabel
              control={<Checkbox checked={value.includes(doc.id)} onChange={(e) => toggle(doc.id, e.target.checked)} />}
              label={`${doc.title} metnini okudum, onaylıyorum.`}
            />
            <Link component="button" type="button" variant="body2" onClick={() => setReading(doc)}>
              Metni oku
            </Link>
          </Stack>
        ))}
      </Stack>
      {error && <FormHelperText>{error}</FormHelperText>}

      <Dialog open={reading !== null} onClose={() => setReading(null)} maxWidth="sm" fullWidth>
        <DialogTitle>{reading?.title}</DialogTitle>
        <DialogContent dividers>
          <Typography variant="caption" color="text.secondary">
            Sürüm {reading?.version || '-'}
          </Typography>
          <Typography sx={{ whiteSpace: 'pre-wrap', mt: 1 }}>{reading?.body || '-'}</Typography>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setReading(null)}>Kapat</Button>
        </DialogActions>
      </Dialog>
    </FormControl>
  )
}

