import { Container, CssBaseline, Typography } from '@mui/material'

// Faz 1 iskeleti: provider'lar (QueryClient, Theme, Auth, Realtime) ve router Faz 2'de eklenir.
export default function App() {
  return (
    <>
      <CssBaseline />
      <Container maxWidth="sm" sx={{ py: 8 }}>
        <Typography variant="h4" component="h1" gutterBottom>
          IndustryApp
        </Typography>
        <Typography color="text.secondary">
          Kaza sonrası onarım teklif platformu · Faz 1 iskeleti
        </Typography>
      </Container>
    </>
  )
}
