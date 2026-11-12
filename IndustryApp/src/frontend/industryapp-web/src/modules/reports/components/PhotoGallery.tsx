import { useState } from 'react'
import { Box, CircularProgress, Dialog, IconButton, Skeleton, Typography } from '@mui/material'
import BrokenImageIcon from '@mui/icons-material/BrokenImage'
import CloseIcon from '@mui/icons-material/Close'
import { usePhotoBlob } from '../controller/usePhotoBlob'

function PhotoThumb({ photoId, index, onOpen }: { photoId: string; index: number; onOpen: (url: string) => void }) {
  const { url, isPending, isError } = usePhotoBlob(photoId)

  if (isPending) return <Skeleton variant="rectangular" sx={{ width: '100%', aspectRatio: '4 / 3', borderRadius: 1 }} />
  if (isError || !url) {
    return (
      <Box sx={{ aspectRatio: '4 / 3', display: 'grid', placeItems: 'center', bgcolor: 'action.hover', borderRadius: 1 }}>
        <BrokenImageIcon color="disabled" aria-label="Fotoğraf yüklenemedi" />
      </Box>
    )
  }

  return (
    <Box
      component="img"
      src={url}
      alt={`Hasar fotoğrafı ${index + 1}`}
      onClick={() => onOpen(url)}
      sx={{ width: '100%', aspectRatio: '4 / 3', objectFit: 'cover', borderRadius: 1, cursor: 'zoom-in', display: 'block' }}
    />
  )
}

/**
 * Raporun fotoğrafları (endpoint 15). Her fotoğraf token'lı istekle blob olarak çekilir; tıklanınca büyük gösterilir.
 * Müşteri rapor detayında ve usta iş detayında aynı bileşen kullanılır.
 */
export function PhotoGallery({ photoIds }: { photoIds: string[] }) {
  const [openUrl, setOpenUrl] = useState<string | null>(null)

  if (photoIds.length === 0) return <Typography color="text.secondary">Fotoğraf yok.</Typography>

  return (
    <>
      <Box sx={{ display: 'grid', gap: 1, gridTemplateColumns: { xs: 'repeat(2, 1fr)', sm: 'repeat(4, 1fr)' } }}>
        {photoIds.map((id, index) => (
          <PhotoThumb key={id} photoId={id} index={index} onOpen={setOpenUrl} />
        ))}
      </Box>
      <Dialog open={openUrl !== null} onClose={() => setOpenUrl(null)} maxWidth="lg">
        <IconButton aria-label="Kapat" onClick={() => setOpenUrl(null)} sx={{ position: 'absolute', right: 8, top: 8, bgcolor: 'background.paper' }}>
          <CloseIcon />
        </IconButton>
        {openUrl ? (
          <Box component="img" src={openUrl} alt="Hasar fotoğrafı" sx={{ maxWidth: '100%', maxHeight: '85vh', display: 'block' }} />
        ) : (
          <CircularProgress />
        )}
      </Dialog>
    </>
  )
}
