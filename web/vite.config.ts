import react from '@vitejs/plugin-react'
import { defineConfig } from 'vitest/config'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    proxy: {
      // The app calls /api on its own origin and Vite forwards to the .NET API:
      // no CORS in development, and the same URLs as behind a reverse proxy in production.
      '/api': 'http://localhost:5143',
    },
  },
  test: {
    environment: 'jsdom',
    setupFiles: ['./src/test/setup.ts'],
    restoreMocks: true,
  },
})
