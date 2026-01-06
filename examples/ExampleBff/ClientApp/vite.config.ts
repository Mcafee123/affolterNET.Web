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
    // Configure HMR to work when accessed via BFF proxy at https://localhost:5004
    hmr: {
      host: 'localhost',
      clientPort: 5004,  // Browser connects to BFF on 5004
      protocol: 'wss'    // BFF uses HTTPS, so WebSocket is wss://
    }
  }
})
