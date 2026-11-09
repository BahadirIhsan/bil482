import { Box, CircularProgress } from '@mui/material'

export function PageLoader() {
  return (
    <Box sx={{ display: 'flex', justifyContent: 'center', py: 8 }}>
      <CircularProgress aria-label="Yükleniyor" />
    </Box>
  )
}
