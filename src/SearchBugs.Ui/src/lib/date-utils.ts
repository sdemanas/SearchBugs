import { formatDistanceToNow, isValid, parseISO } from "date-fns";

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
