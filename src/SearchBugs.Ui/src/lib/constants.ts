export const __prod__ = process.env.NODE_ENV === "production";

// API Base URL configuration
const getApiBaseUrl = (): string => {
  // Check for environment variable first
  if (import.meta.env.VITE_API_BASE_URL) {
    return import.meta.env.VITE_API_BASE_URL;
  }

  // Development defaults
  if (!__prod__) {
    return "http://localhost:5026/api/";
  }

  // Production default
  return "https://api.searchbugs.com/api/";
};

const apiBaseUrl = getApiBaseUrl();

export { apiBaseUrl };
