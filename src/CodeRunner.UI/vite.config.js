import { fileURLToPath, URL } from 'node:url'
import { defineConfig } from 'vite'
import copy from 'rollup-plugin-copy';
import vue from '@vitejs/plugin-vue'


// https://vitejs.dev/config/
export default defineConfig({
  plugins: [
    vue(), 
    copy({
      targets: [
        {
          src: './node_modules/ace-builds/src-min-noconflict/*',
          dest: './dist/node_modules/ace-builds/src-noconflict'
        }
      ],
      verbose: true,
      hook: 'writeBundle'
    })
  ],
  resolve: {
    alias: {
      '@': fileURLToPath(new URL('./src', import.meta.url))
    }
  }
})