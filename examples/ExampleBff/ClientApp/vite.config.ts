import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import { fileURLToPath, URL } from 'node:url'

export default defineConfig({
  plugins: [vue()],
  resolve: {
    alias: {
      '@': fileURLToPath(new URL('./src', import.meta.url))
    }
  },
  build: {
    outDir: '../wwwroot',
    emptyOutDir: true,
    rollupOptions: {
      output: {
        entryFileNames: 'assets/[name]-[hash].js',
        chunkFileNames: 'assets/[name]-[hash].js',
        assetFileNames: 'assets/[name]-[hash].[ext]'
      }
    }
  },
  server: {
    port: 5173,
    https: true,
    proxy: {
      '/api': {
        target: 'https://localhost:5004',
        changeOrigin: true,
        secure: false
      },
      '/bff': {
        target: 'https://localhost:5004',
        changeOrigin: true,
        secure: false
      },
      '/signin-oidc': {
        target: 'https://localhost:5004',
        changeOrigin: true,
        secure: false
      },
      '/signout-callback-oidc': {
        target: 'https://localhost:5004',
        changeOrigin: true,
        secure: false
      },
      '/health': {
        target: 'https://localhost:5004',
        changeOrigin: true,
        secure: false
      }
    }
  }
})
