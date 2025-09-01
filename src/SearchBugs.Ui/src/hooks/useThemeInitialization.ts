import { useEffect } from "react";
import { useUIStore } from "../stores/global/uiStore";
import {
  applyTheme,
  setupSystemThemeListener,
  getSystemTheme,
} from "../lib/theme";

/**
 * Vite-friendly theme initialization hook
 * Uses utility functions for cleaner theme management
 */
export const useThemeInitialization = () => {
  const { theme } = useUIStore();

  // Initialize theme on mount
  useEffect(() => {
    applyTheme(theme);
  }, [theme]);

  // Handle system theme changes
  useEffect(() => {
    if (theme !== "system") return;

    const cleanup = setupSystemThemeListener((isDark) => {
      const root = document.documentElement;
      root.setAttribute("data-theme", isDark ? "dark" : "light");
      root.classList.toggle("dark", isDark);
    });

    // Apply initial system theme
    const systemTheme = getSystemTheme();
    const root = document.documentElement;
    root.setAttribute("data-theme", systemTheme);
    root.classList.toggle("dark", systemTheme === "dark");

    return cleanup;
  }, [theme]);
};
