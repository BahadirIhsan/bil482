import { useRef } from 'react'
import { Alert, Box, Button, IconButton, LinearProgress, Stack, Typography } from '@mui/material'
import AddAPhotoIcon from '@mui/icons-material/AddAPhoto'
import CheckCircleIcon from '@mui/icons-material/CheckCircle'
import DeleteIcon from '@mui/icons-material/Delete'
import ErrorIcon from '@mui/icons-material/Error'
import ReplayIcon from '@mui/icons-material/Replay'
import { PHOTO_INPUT_ACCEPT } from '../model/photoRules'
import type { usePhotoUpload } from '../controller/usePhotoUpload'

type PhotoUploadState = ReturnType<typeof usePhotoUpload>

/** Sihirbazın 1. adımı: fotoğraf seçme/çekme, her fotoğraf için ilerleme, hata ve tekrar deneme. */
export function PhotoUploader({ state }: { state: PhotoUploadState }) {
  const input = useRef<HTMLInputElement>(null)

  return (
    <Stack spacing={2}>
      <Typography color="text.secondary">
        Hasarı farklı açılardan gösteren 1–{state.max} fotoğraf ekleyin (JPEG, PNG veya WebP; en fazla 10 MB). Fotoğrafların
        konum bilgisi sunucuda silinir.
      </Typography>

      <input
        ref={input}
        type="file"
        accept={PHOTO_INPUT_ACCEPT}
        multiple
        hidden
        onChange={(e) => {
          if (e.target.files) state.add(e.target.files)
          e.target.value = ''
        }}
      />
      <Button
        variant="outlined"
        startIcon={<AddAPhotoIcon />}
        disabled={!state.canAddMore}
        onClick={() => input.current?.click()}
        sx={{ alignSelf: 'flex-start' }}
      >
        Fotoğraf ekle ({state.items.length}/{state.max})
      </Button>

      {state.items.length === 0 && <Alert severity="info">Devam etmek için en az bir fotoğraf ekleyin.</Alert>}

      <Box sx={{ display: 'grid', gap: 1.5, gridTemplateColumns: { xs: 'repeat(2, 1fr)', sm: 'repeat(4, 1fr)' } }}>
        {state.items.map((item) => (
          <Box key={item.id} sx={{ border: 1, borderColor: item.status === 'error' ? 'error.main' : 'divider', borderRadius: 1, overflow: 'hidden' }}>
            <Box
              component="img"
              src={item.previewUrl}
              alt={item.file.name}
              sx={{ width: '100%', aspectRatio: '4 / 3', objectFit: 'cover', display: 'block', opacity: item.status === 'done' ? 1 : 0.6 }}
            />
            <Box sx={{ p: 1 }}>
              {item.status === 'uploading' && <LinearProgress variant="determinate" value={item.progress} aria-label="Yükleniyor" />}
              <Stack direction="row" alignItems="center" spacing={0.5} sx={{ mt: item.status === 'uploading' ? 0.5 : 0 }}>
                {item.status === 'done' && <CheckCircleIcon color="success" fontSize="small" />}
                {item.status === 'error' && <ErrorIcon color="error" fontSize="small" />}
                <Typography variant="caption" noWrap sx={{ flexGrow: 1 }} title={item.file.name}>
                  {item.status === 'uploading' ? `%${item.progress}` : item.file.name}
                </Typography>
                {item.status === 'error' && (
                  <IconButton size="small" aria-label="Tekrar dene" onClick={() => state.retry(item.id)}>
                    <ReplayIcon fontSize="small" />
                  </IconButton>
                )}
                <IconButton size="small" aria-label="Fotoğrafı kaldır" disabled={item.status === 'uploading'} onClick={() => state.remove(item.id)}>
                  <DeleteIcon fontSize="small" />
                </IconButton>
              </Stack>
              {item.error && (
                <Typography variant="caption" color="error" sx={{ display: 'block' }}>
                  {item.error}
                </Typography>
              )}
            </Box>
          </Box>
        ))}
      </Box>
    </Stack>
  )
}
