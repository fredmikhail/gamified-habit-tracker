import tailwindcss from '@tailwindcss/vite'
import react from '@vitejs/plugin-react'
import { defineConfig } from 'vitest/config'

export default defineConfig({
  plugins: [react(), tailwindcss()],

  server: {
    proxy: {
      '/api': {
        target: 'https://localhost:7287',
        changeOrigin: true,
        secure: false,
      },
    },
  },

  test: {
    environment: 'jsdom',
    setupFiles: './src/test/setup.ts',
  },
})
