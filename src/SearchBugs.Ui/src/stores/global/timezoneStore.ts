import { create } from "zustand";
import { persist } from "zustand/middleware";

export interface TimezoneState {
  // User's selected timezone
  timezone: string;

  // Auto-detected timezone from browser
  detectedTimezone: string;

  // Whether to use auto-detection or manual selection
  useAutoDetection: boolean;

  // Actions
  setTimezone: (timezone: string) => void;
  setUseAutoDetection: (useAuto: boolean) => void;
  initializeTimezone: () => void;
  getCurrentTimezone: () => string;
}

// Get browser's detected timezone
const getBrowserTimezone = (): string => {
  try {
    return Intl.DateTimeFormat().resolvedOptions().timeZone || "UTC";
  } catch {
    return "UTC";
  }
};

// Popular timezones for user selection
export const POPULAR_TIMEZONES = [
  { value: "UTC", label: "UTC (Coordinated Universal Time)" },
  { value: "America/New_York", label: "Eastern Time (US & Canada)" },
  { value: "America/Chicago", label: "Central Time (US & Canada)" },
  { value: "America/Denver", label: "Mountain Time (US & Canada)" },
  { value: "America/Los_Angeles", label: "Pacific Time (US & Canada)" },
  { value: "Europe/London", label: "London (GMT/BST)" },
  { value: "Europe/Paris", label: "Central European Time" },
  { value: "Asia/Tokyo", label: "Japan Standard Time" },
  { value: "Asia/Shanghai", label: "China Standard Time" },
  { value: "Asia/Kolkata", label: "India Standard Time" },
  { value: "Australia/Sydney", label: "Australian Eastern Time" },
  { value: "Asia/Phnom_Penh", label: "Cambodia Time" },
  { value: "Asia/Bangkok", label: "Thailand Time" },
  { value: "Asia/Ho_Chi_Minh", label: "Vietnam Time" },
];

export const useTimezoneStore = create<TimezoneState>()(
  persist(
    (set, get) => ({
      timezone: getBrowserTimezone(),
      detectedTimezone: getBrowserTimezone(),
      useAutoDetection: true,

      setTimezone: (timezone: string) => {
        set({
          timezone,
          useAutoDetection: false,
        });
      },

      setUseAutoDetection: (useAuto: boolean) => {
        const detectedTimezone = getBrowserTimezone();
        set({
          useAutoDetection: useAuto,
          timezone: useAuto ? detectedTimezone : get().timezone,
          detectedTimezone,
        });
      },

      initializeTimezone: () => {
        const detectedTimezone = getBrowserTimezone();
        const currentState = get();

        set({
          detectedTimezone,
          // If using auto-detection, update timezone to current detected one
          timezone: currentState.useAutoDetection
            ? detectedTimezone
            : currentState.timezone,
        });
      },

      getCurrentTimezone: () => {
        const state = get();
        return state.useAutoDetection ? state.detectedTimezone : state.timezone;
      },
    }),
    {
      name: "timezone-settings", // localStorage key
      // Only persist user preferences, not detected timezone
      partialize: (state) => ({
        timezone: state.timezone,
        useAutoDetection: state.useAutoDetection,
      }),
    }
  )
);
