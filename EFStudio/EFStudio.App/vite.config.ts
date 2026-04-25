import path from "path";
import tailwindcss from "@tailwindcss/vite";
import react from "@vitejs/plugin-react";
import { defineConfig } from "vite";

export default defineConfig({
  base: "/efstudio/",
  plugins: [react(), tailwindcss()],
  resolve: {
    alias: {
      "@": path.resolve(__dirname, "./src"),
    },
  },
  server: {
    open: "/efstudio/",
    proxy: {
      "/efstudio/api": {
        target: "http://localhost:5123",
        changeOrigin: true,
      },
    },
  },
  build: {
    outDir: "../EFStudio.Core/wwwroot",
    emptyOutDir: true,
  },
});
