/**
 * Vite-friendly theme utilities
 * Uses CSS custom properties and data attributes for optimal performance
 */

export type Theme = "light" | "dark" | "system";

export const getSystemTheme = (): "light" | "dark" => {
  if (typeof window === "undefined") return "light";
  return window.matchMedia("(prefers-color-scheme: dark)").matches
    ? "dark"
    : "light";
};

export const applyTheme = (theme: Theme): void => {
  if (typeof window === "undefined") return;

  const root = document.documentElement;

  // Remove existing theme attributes
  root.removeAttribute("data-theme");
  root.classList.remove("dark");

  if (theme === "dark") {
    root.setAttribute("data-theme", "dark");
    root.classList.add("dark");
  } else if (theme === "light") {
    root.setAttribute("data-theme", "light");
  } else {
    // System theme
    const systemTheme = getSystemTheme();
    root.setAttribute("data-theme", systemTheme);
    if (systemTheme === "dark") {
      root.classList.add("dark");
    }
  }
};

export const getStoredTheme = (): Theme => {
  if (typeof window === "undefined") return "system";

  const stored = localStorage.getItem("theme") as Theme;
  return stored && ["light", "dark", "system"].includes(stored)
    ? stored
    : "system";
};

export const setStoredTheme = (theme: Theme): void => {
  if (typeof window === "undefined") return;
  localStorage.setItem("theme", theme);
};

export const setupSystemThemeListener = (
  callback: (isDark: boolean) => void
): (() => void) => {
  if (typeof window === "undefined") return () => {};

  const mediaQuery = window.matchMedia("(prefers-color-scheme: dark)");

  const handleChange = (e: MediaQueryListEvent) => {
    callback(e.matches);
  };

  mediaQuery.addEventListener("change", handleChange);

  // Return cleanup function
  return () => {
    mediaQuery.removeEventListener("change", handleChange);
  };
};
