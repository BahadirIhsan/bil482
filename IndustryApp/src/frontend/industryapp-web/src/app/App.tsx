import { CssBaseline, ThemeProvider } from '@mui/material'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { RouterProvider } from 'react-router-dom'
import { AuthProvider } from '../shared/auth'
import { isApiError } from '../shared/errors'
import { RealtimeProvider } from '../shared/realtime'
import { router } from './router'
import { theme } from './theme'

// 4xx yanıtlar tekrar denenmez (403/404/409 tekrar denemeyle düzelmez); ağ ve 5xx hataları bir kez denenir.
const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: 30_000,
      refetchOnWindowFocus: false,
      retry: (failureCount, error) => !(isApiError(error) && error.status >= 400 && error.status < 500) && failureCount < 1,
    },
  },
})

/** Sağlayıcı sırası: Query → Theme → Auth (Query'yi temizler) → Realtime (Auth'a bağlı) → Router. */
export default function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <ThemeProvider theme={theme}>
        <CssBaseline />
        <AuthProvider>
          <RealtimeProvider>
            <RouterProvider router={router} />
          </RealtimeProvider>
        </AuthProvider>
      </ThemeProvider>
    </QueryClientProvider>
  )
}
