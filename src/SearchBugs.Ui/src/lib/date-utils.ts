import { formatDistanceToNow, isValid, parseISO, format } from "date-fns";
import { fromZonedTime, toZonedTime, formatInTimeZone } from "date-fns-tz";

/**
 * Safely formats a date string to "time ago" format (e.g., "2 hours ago")
 * Handles invalid dates gracefully by returning a fallback message
 * @param dateString - The date string to format (ISO string or any valid date format)
 * @returns Formatted time distance string or "Recently" if invalid
 */
export const safeFormatDistance = (
  dateString: string | undefined | null
): string => {
  try {
    // Handle null, undefined, or empty strings
    if (!dateString || typeof dateString !== "string") {
      return "Recently";
    }

    // Try parsing the date string in different formats
    let date: Date;

    // First try parsing as ISO string
    date = parseISO(dateString);

    // If that fails, try with Date constructor
    if (!isValid(date)) {
      date = new Date(dateString);
    }

    // If still invalid, return fallback
    if (!isValid(date)) {
      return "Recently";
    }

    return formatDistanceToNow(date, { addSuffix: true });
  } catch (error) {
    console.warn("Error formatting date:", dateString, error);
    return "Recently";
  }
};

/**
 * Timezone-aware version of safeFormatDistance
 * @param dateString - The date string to format (should be in UTC)
 * @param timezone - The target timezone (e.g., 'America/New_York')
 * @returns Formatted time distance string considering timezone
 */
export const safeFormatDistanceWithTimezone = (
  dateString: string | undefined | null,
  timezone: string = "UTC"
): string => {
  try {
    if (!dateString || typeof dateString !== "string") {
      return "Recently";
    }

    let utcDate: Date;
    utcDate = parseISO(dateString);

    if (!isValid(utcDate)) {
      utcDate = new Date(dateString);
    }

    if (!isValid(utcDate)) {
      return "Recently";
    }

    // Convert UTC date to user's timezone for accurate relative time
    const zonedDate = toZonedTime(utcDate, timezone);
    return formatDistanceToNow(zonedDate, { addSuffix: true });
  } catch (error) {
    console.warn(
      "Error formatting date with timezone:",
      dateString,
      timezone,
      error
    );
    return "Recently";
  }
};

/**
 * Formats a date string to a specific format in a given timezone
 * @param dateString - The date string to format (should be in UTC)
 * @param timezone - The target timezone
 * @param formatString - The format string (e.g., 'PPpp', 'MMM dd, yyyy HH:mm')
 * @returns Formatted date string
 */
export const formatDateInTimezone = (
  dateString: string | undefined | null,
  timezone: string = "UTC",
  formatString: string = "PPpp" // Default: "Apr 29, 2021, 1:45:00 PM"
): string => {
  try {
    if (!dateString || typeof dateString !== "string") {
      return "Invalid date";
    }

    let utcDate: Date;
    utcDate = parseISO(dateString);

    if (!isValid(utcDate)) {
      utcDate = new Date(dateString);
    }

    if (!isValid(utcDate)) {
      return "Invalid date";
    }

    // Format the date in the specified timezone
    return formatInTimeZone(utcDate, timezone, formatString);
  } catch (error) {
    console.warn(
      "Error formatting date in timezone:",
      dateString,
      timezone,
      error
    );
    return "Invalid date";
  }
};

/**
 * Converts a local date/time to UTC for sending to server
 * @param date - The local date
 * @param timezone - The user's timezone
 * @returns UTC date
 */
export const convertToUtc = (date: Date, timezone: string): Date => {
  try {
    return fromZonedTime(date, timezone);
  } catch (error) {
    console.warn("Error converting to UTC:", date, timezone, error);
    return date; // Fallback to original date
  }
};

/**
 * Safely parses a date string for sorting purposes
 * Returns timestamp in milliseconds or 0 for invalid dates
 * @param dateString - The date string to parse
 * @returns Timestamp in milliseconds
 */
export const safeParseDateForSort = (
  dateString: string | undefined | null
): number => {
  try {
    if (!dateString || typeof dateString !== "string") {
      return 0;
    }

    let date = parseISO(dateString);
    if (!isValid(date)) {
      date = new Date(dateString);
    }
    return isValid(date) ? date.getTime() : 0;
  } catch {
    return 0;
  }
};
