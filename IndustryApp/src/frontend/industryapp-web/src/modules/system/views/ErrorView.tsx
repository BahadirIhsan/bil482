import { Link as RouterLink } from 'react-router-dom'
import { Button, Stack, Typography } from '@mui/material'

const content = {
  403: { title: 'Erişim izniniz yok', text: 'Bu sayfayı görüntülemek için yetkiniz bulunmuyor.' },
  404: { title: 'Sayfa bulunamadı', text: 'Aradığınız sayfa taşınmış veya hiç var olmamış olabilir.' },
} as const

/** 403 ve 404 sayfası (route /403 ve *, view #8). */
export function ErrorView({ code }: { code: 403 | 404 }) {
  return (
    <Stack spacing={2} alignItems="flex-start" sx={{ py: 6 }}>
      <Typography variant="h3" component="p" color="text.secondary">
        {code}
      </Typography>
      <Typography variant="h5" component="h1">
        {content[code].title}
      </Typography>
      <Typography color="text.secondary">{content[code].text}</Typography>
      <Button component={RouterLink} to="/" variant="contained">
        Ana sayfaya dön
      </Button>
    </Stack>
  )
}
