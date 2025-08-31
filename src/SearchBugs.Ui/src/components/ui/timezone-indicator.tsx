import React from "react";
import { Badge } from "@/components/ui/badge";
import { Clock } from "lucide-react";
import { useTimezone } from "@/hooks/useTimezone";

export const TimezoneIndicator: React.FC = () => {
  const { currentTimezone, getCurrentTime } = useTimezone();
  const [currentTime, setCurrentTime] = React.useState<string>("");

  React.useEffect(() => {
    const updateTime = () => {
      setCurrentTime(getCurrentTime());
    };

    updateTime();
    const interval = setInterval(updateTime, 30000); // Update every 30 seconds

    return () => clearInterval(interval);
  }, [getCurrentTime]);

  // Get short timezone name
  const getShortTimezoneName = () => {
    try {
      const formatter = new Intl.DateTimeFormat("en", {
        timeZone: currentTimezone,
        timeZoneName: "short",
      });
      const parts = formatter.formatToParts(new Date());
      const timeZoneName = parts.find(
        (part) => part.type === "timeZoneName"
      )?.value;
      return timeZoneName || currentTimezone.split("/").pop();
    } catch {
      return currentTimezone.split("/").pop();
    }
  };

  return (
    <Badge
      variant="outline"
      className="flex items-center gap-1 text-xs"
      title={`Current timezone: ${currentTimezone}\n${currentTime}`}
    >
      <Clock className="h-3 w-3" />
      {getShortTimezoneName()}
    </Badge>
  );
};
