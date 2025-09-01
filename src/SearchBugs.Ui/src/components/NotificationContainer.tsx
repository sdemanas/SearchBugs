import React, { useState } from "react";
import { useNotifications } from "../hooks/useNotifications";
import { NotificationData } from "../services/notificationService";

interface NotificationPopupProps {
  notification: NotificationData;
  onDismiss: () => void;
}

const NotificationPopup: React.FC<NotificationPopupProps> = ({
  notification,
  onDismiss,
}) => {
  const [isVisible, setIsVisible] = useState(true);

  const handleDismiss = () => {
    setIsVisible(false);
    setTimeout(() => {
      onDismiss();
    }, 300);
  };

  const getNotificationIcon = (type: string) => {
    switch (type) {
      case "bug_created":
        return "🐛";
      case "bug_updated":
        return "📝";
      case "bug_assigned":
        return "👤";
      default:
        return "📢";
    }
  };

  const getNotificationColor = (type: string) => {
    switch (type) {
      case "bug_created":
        return "bg-green-500";
      case "bug_updated":
        return "bg-blue-500";
      case "bug_assigned":
        return "bg-yellow-500";
      default:
        return "bg-gray-500";
    }
  };

  if (!isVisible) return null;

  return (
    <div
      className={`
      fixed top-4 right-4 z-50 
      transform transition-all duration-300 ease-in-out
      ${isVisible ? "translate-x-0 opacity-100" : "translate-x-full opacity-0"}
    `}
    >
      <div className="bg-card rounded-lg shadow-lg border border-border max-w-sm w-full">
        <div
          className={`${getNotificationColor(
            notification.type
          )} h-1 rounded-t-lg`}
        />

        <div className="p-4">
          <div className="flex items-start justify-between">
            <div className="flex items-start space-x-3">
              <div className="text-2xl">
                {getNotificationIcon(notification.type)}
              </div>

              <div className="flex-1">
                <p className="text-sm font-medium text-card-foreground">
                  {notification.message}
                </p>

                {notification.data && (
                  <p className="text-xs text-muted-foreground mt-1">
                    {notification.data}
                  </p>
                )}

                <p className="text-xs text-muted-foreground mt-2">
                  {new Date(notification.timestamp).toLocaleTimeString()}
                </p>
              </div>
            </div>

            <button
              onClick={handleDismiss}
              className="text-muted-foreground hover:text-foreground transition-colors"
            >
              <svg className="w-4 h-4" fill="currentColor" viewBox="0 0 20 20">
                <path
                  fillRule="evenodd"
                  d="M4.293 4.293a1 1 0 011.414 0L10 8.586l4.293-4.293a1 1 0 111.414 1.414L11.414 10l4.293 4.293a1 1 0 01-1.414 1.414L10 11.414l-4.293 4.293a1 1 0 01-1.414-1.414L8.586 10 4.293 5.707a1 1 0 010-1.414z"
                  clipRule="evenodd"
                />
              </svg>
            </button>
          </div>
        </div>
      </div>
    </div>
  );
};

interface NotificationContainerProps {}

export const NotificationContainer: React.FC<
  NotificationContainerProps
> = () => {
  const { notifications } = useNotifications();
  const [visibleNotifications, setVisibleNotifications] = useState<
    NotificationData[]
  >([]);

  React.useEffect(() => {
    // Show only the latest 3 notifications as popups
    const latestNotifications = notifications
      .filter((n) => !n.isRead)
      .slice(0, 3)
      .reverse(); // Show newest first

    setVisibleNotifications(latestNotifications);
  }, [notifications]);

  const handleDismissNotification = (notificationId: string | undefined) => {
    if (notificationId) {
      setVisibleNotifications((prev) =>
        prev.filter((n) => n.id !== notificationId)
      );
    }
  };

  return (
    <div className="fixed top-0 right-0 z-50 pointer-events-none">
      {visibleNotifications.map((notification, index) => (
        <div
          key={notification.id}
          className="pointer-events-auto"
          style={{
            transform: `translateY(${index * 120}px)`,
          }}
        >
          <NotificationPopup
            notification={notification}
            onDismiss={() => handleDismissNotification(notification.id)}
          />
        </div>
      ))}
    </div>
  );
};

export default NotificationContainer;
