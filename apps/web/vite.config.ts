import { defineConfig } from 'vite';
import vue from '@vitejs/plugin-vue';

const apiTarget = process.env.VITE_API_URL || 'http://localhost:8080';

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [vue()],
  server: {
    port: 3000,
    proxy: {
      '/products': {
        target: apiTarget,
        changeOrigin: true
      },
      '/warehouses': {
        target: apiTarget,
        changeOrigin: true
      },
      '/orders': {
        target: apiTarget,
        changeOrigin: true
      },
      '/auth': {
        target: apiTarget,
        changeOrigin: true
      }
    }
  }
});
