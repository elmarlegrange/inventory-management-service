import { defineConfig } from 'vite';
import vue from '@vitejs/plugin-vue';

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [vue()],
  server: {
    port: 3000,
    proxy: {
      '/products': {
        target: 'http://localhost:8080',
        changeOrigin: true
      },
      '/warehouses': {
        target: 'http://localhost:8080',
        changeOrigin: true
      },
      '/orders': {
        target: 'http://localhost:8080',
        changeOrigin: true
      }
    }
  }
});
