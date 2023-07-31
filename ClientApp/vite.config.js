import path from 'path';
import { defineConfig } from 'vite';

export default defineConfig({
  build: {
    outDir: path.resolve(__dirname, '..', 'wwwroot', 'dist'),
    sourcemap: true,
    emptyOutDir: true,
    rollupOptions: {
      output: {
        entryFileNames: 'main.js',
        chunkFileNames: 'main.js',
        assetFileNames: 'style[extname]',
      },
      input: {
        main: path.resolve(__dirname, 'src', 'js', 'site.js'),
      },
    },
    assetsDir: '',
  },
  mode: 'development',
  resolve: {
    alias: {
      '@': path.resolve(__dirname, 'src'),
    },
  },
  optimizeDeps: {
    include: ['@fortawesome/fontawesome-free'],
  },
  css: {
    postcss: {
      plugins: [require('autoprefixer')],
    },
  },
  server: {
    port: 3000,
  },
});