import { useEffect } from "react";
import { useTimezoneStore } from "../stores/global/timezoneStore";
import {
  safeFormatDistanceWithTimezone,
  formatDateInTimezone,
  convertToUtc,
} from "@/lib/date-utils";

export const useTimezone = () => {
  const {
    timezone,
    detectedTimezone,
    useAutoDetection,
    setTimezone,
    setUseAutoDetection,
    initializeTimezone,
    getCurrentTimezone,
  } = useTimezoneStore();

  // Initialize timezone on mount
  useEffect(() => {
    initializeTimezone();
  }, [initializeTimezone]);

  // Get the current active timezone
  const currentTimezone = getCurrentTimezone();

  // Timezone-aware formatting functions
  const formatDistanceToNow = (dateString: string | undefined | null) => {
    return safeFormatDistanceWithTimezone(dateString, currentTimezone);
  };

  const formatDate = (
    dateString: string | undefined | null,
    formatString: string = "PPpp"
  ) => {
    return formatDateInTimezone(dateString, currentTimezone, formatString);
  };

  const toUtc = (date: Date) => {
    return convertToUtc(date, currentTimezone);
  };

  // Get timezone display name
  const getTimezoneDisplay = (tz?: string) => {
    const targetTimezone = tz || currentTimezone;
    try {
      const formatter = new Intl.DateTimeFormat("en", {
        timeZone: targetTimezone,
        timeZoneName: "long",
      });
      const parts = formatter.formatToParts(new Date());
      const timeZoneName = parts.find(
        (part) => part.type === "timeZoneName"
      )?.value;
      return `${targetTimezone} (${timeZoneName})`;
    } catch {
      return targetTimezone;
    }
  };

  // Get current time in user's timezone
  const getCurrentTime = () => {
    return new Date().toLocaleString("en-US", {
      timeZone: currentTimezone,
      year: "numeric",
      month: "short",
      day: "numeric",
      hour: "2-digit",
      minute: "2-digit",
      second: "2-digit",
      timeZoneName: "short",
    });
  };

  return {
    // State
    timezone,
    detectedTimezone,
    useAutoDetection,
    currentTimezone,

    // Actions
    setTimezone,
    setUseAutoDetection,

    // Utility functions
    formatDistanceToNow,
    formatDate,
    toUtc,
    getTimezoneDisplay,
    getCurrentTime,
  };
};
