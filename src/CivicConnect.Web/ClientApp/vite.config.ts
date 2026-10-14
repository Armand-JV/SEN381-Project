import react from "@vitejs/plugin-react";
import { defineConfig } from "vite";

export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    proxy: {
      "/api": { target: "https://localhost:7121", changeOrigin: true, secure: false },
      "/health": { target: "https://localhost:7121", changeOrigin: true, secure: false },
    },
  },
  build: {
    outDir: "../wwwroot",
    emptyOutDir: true,
  },
});
