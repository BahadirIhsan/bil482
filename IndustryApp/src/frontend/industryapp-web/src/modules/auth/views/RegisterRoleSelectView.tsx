import { Link as RouterLink } from 'react-router-dom'
import { Card, CardActionArea, CardContent, Grid, Link, Stack, Typography } from '@mui/material'
import BuildIcon from '@mui/icons-material/Build'
import DirectionsCarIcon from '@mui/icons-material/DirectionsCar'

const options = [
  {
    to: '/register/customer',
    title: 'Müşteriyim',
    text: 'Aracımdaki hasarı bildirip ustalardan teklif almak istiyorum.',
    icon: <DirectionsCarIcon fontSize="large" color="primary" />,
  },
  {
    to: '/register/mechanic',
    title: 'Ustayım',
    text: 'Atölyem için iş bildirimi alıp teklif vermek istiyorum. Hesabım onaydan sonra açılır.',
    icon: <BuildIcon fontSize="large" color="primary" />,
  },
]

/** UC-01 · Kayıt türü seçimi (route /register, view #2). */
export function RegisterRoleSelectView() {
  return (
    <Stack spacing={3} sx={{ maxWidth: 720, mx: 'auto' }}>
      <Typography variant="h5" component="h1">
        Kayıt ol
      </Typography>
      <Grid container spacing={2}>
        {options.map((option) => (
          <Grid item xs={12} sm={6} key={option.to}>
            <Card variant="outlined" sx={{ height: '100%' }}>
              <CardActionArea component={RouterLink} to={option.to} sx={{ height: '100%' }}>
                <CardContent>
                  <Stack spacing={1.5}>
                    {option.icon}
                    <Typography variant="h6">{option.title}</Typography>
                    <Typography color="text.secondary">{option.text}</Typography>
                  </Stack>
                </CardContent>
              </CardActionArea>
            </Card>
          </Grid>
        ))}
      </Grid>
      <Typography variant="body2" color="text.secondary">
        Zaten hesabınız var mı?{' '}
        <Link component={RouterLink} to="/login">
          Giriş yapın
        </Link>
      </Typography>
    </Stack>
  )
}
