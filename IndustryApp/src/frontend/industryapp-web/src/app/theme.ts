import { createTheme } from '@mui/material/styles'
import { trTR } from '@mui/material/locale'

/** Uygulama teması; MUI bileşen metinleri (sayfalama vb.) Türkçe. */
export const theme = createTheme(
  {
    palette: {
      primary: { main: '#1f5fa8' },
      background: { default: '#f6f7f9' },
    },
    shape: { borderRadius: 8 },
    components: {
      MuiButton: { defaultProps: { disableElevation: true }, styleOverrides: { root: { textTransform: 'none' } } },
    },
  },
  trTR,
)
