import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// Geliştirmede /api ve /hubs istekleri yerel API'ye (launchSettings: 5080) yönlendirilir; CORS ayarı gerekmez.
export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    proxy: {
      '/api': { target: 'http://localhost:5080', changeOrigin: true },
      '/hubs': { target: 'http://localhost:5080', changeOrigin: true, ws: true },
    },
  },
})
