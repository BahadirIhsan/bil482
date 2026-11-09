import { Link as RouterLink, Outlet } from 'react-router-dom'
import { AppBar, Box, Button, Container, Toolbar, Typography } from '@mui/material'

/** Giriş ve kayıt ekranlarının layout'u (anonim). */
export function PublicLayout() {
  return (
    <Box sx={{ minHeight: '100vh', bgcolor: 'background.default' }}>
      <AppBar position="static" elevation={0}>
        <Toolbar>
          <Typography variant="h6" component={RouterLink} to="/" sx={{ flexGrow: 1, color: 'inherit', textDecoration: 'none' }}>
            IndustryApp
          </Typography>
          <Button color="inherit" component={RouterLink} to="/login">
            Giriş
          </Button>
          <Button color="inherit" component={RouterLink} to="/register">
            Kayıt ol
          </Button>
        </Toolbar>
      </AppBar>
      <Container maxWidth="md" sx={{ py: { xs: 2, sm: 6 } }}>
        <Outlet />
      </Container>
    </Box>
  )
}
