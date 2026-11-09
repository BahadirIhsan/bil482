import { Alert, Stack, Typography } from '@mui/material'

/** Henüz yazılmamış ekranların yer tutucusu (rehber 2.4: bu fazda yalnızca 1–8 ve 12 aktif). */
export function ComingSoon({ title, release }: { title: string; release: string }) {
  return (
    <Stack spacing={2}>
      <Typography variant="h5" component="h1">
        {title}
      </Typography>
      <Alert severity="info">Bu ekran {release} sürümünde eklenecek.</Alert>
    </Stack>
  )
}
