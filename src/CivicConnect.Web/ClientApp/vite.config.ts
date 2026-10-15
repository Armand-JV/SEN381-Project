import react from "@vitejs/plugin-react";
import { defineConfig } from "vite";

export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    proxy: {
      "/api": { target: "http://localhost:5287", changeOrigin: true },
      "/health": { target: "http://localhost:5287", changeOrigin: true },
    },
  },
  build: {
    outDir: "../wwwroot",
    emptyOutDir: true,
  },
});
