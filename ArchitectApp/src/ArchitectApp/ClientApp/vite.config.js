import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';

export default defineConfig({
    plugins: [react()],

    build: {
        outDir: '../wwwroot/dist',
        emptyOutDir: true,
        manifest: true,

        rollupOptions: {
            input: 'src/main.js',
            output: {
                entryFileNames: `[name].js`,
                chunkFileNames: `[name].js`,
                assetFileNames: `[name].[ext]`
            }
        }
    }
});