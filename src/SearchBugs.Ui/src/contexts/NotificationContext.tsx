import React, { createContext, useEffect, useState, ReactNode } from "react";
import {
  notificationService,
  NotificationData,
} from "../services/notificationService";
import { useAuthStore } from "../stores/global/authStore";

interface NotificationContextType {
  notifications: NotificationData[];
  unreadCount: number;
  isConnected: boolean;
  addNotification: (notification: NotificationData) => void;
  markAsRead: (notificationId: string) => void;
  markAllAsRead: () => void;
  clearNotifications: () => void;
}

const NotificationContext = createContext<NotificationContextType | undefined>(
  undefined
);

export { NotificationContext };

interface NotificationProviderProps {
  children: ReactNode;
}

export const NotificationProvider: React.FC<NotificationProviderProps> = ({
  children,
}) => {
  const [notifications, setNotifications] = useState<NotificationData[]>([]);
  const [isConnected, setIsConnected] = useState(false);
  const { user, isAuthenticated } = useAuthStore();

  useEffect(() => {
    const initializeSignalR = async () => {
      if (isAuthenticated && user) {
        try {
          await notificationService.startConnection();
          await notificationService.joinUserGroup(user.id);
          setIsConnected(true);

          // Set up notification listeners
          const handleNotification = (data: NotificationData) => {
            addNotification(data);
          };

          const handleBugNotification = (data: NotificationData) => {
            addNotification(data);
          };

          notificationService.onNotification(
            "notification",
            handleNotification
          );
          notificationService.onNotification(
            "bugNotification",
            handleBugNotification
          );

          return () => {
            notificationService.offNotification(
              "notification",
              handleNotification
            );
            notificationService.offNotification(
              "bugNotification",
              handleBugNotification
            );
          };
        } catch (error) {
          console.error("Failed to initialize SignalR:", error);
          setIsConnected(false);
        }
      }
    };

    const cleanup = initializeSignalR();

    return () => {
      if (cleanup instanceof Promise) {
        cleanup.then((cleanupFn) => cleanupFn?.());
      }
    };
  }, [isAuthenticated, user]);

  useEffect(() => {
    return () => {
      notificationService.stopConnection();
    };
  }, []);

  const addNotification = (notification: NotificationData) => {
    setNotifications((prev) => [
      {
        ...notification,
        id: notification.id || Date.now().toString(),
      },
      ...prev,
    ]);
  };

  const markAsRead = (notificationId: string) => {
    setNotifications((prev) =>
      prev.map((notification) =>
        notification.id === notificationId
          ? { ...notification, isRead: true }
          : notification
      )
    );
  };

  const markAllAsRead = () => {
    setNotifications((prev) =>
      prev.map((notification) => ({ ...notification, isRead: true }))
    );
  };

  const clearNotifications = () => {
    setNotifications([]);
  };

  const unreadCount = notifications.filter((n) => !n.isRead).length;

  const value: NotificationContextType = {
    notifications,
    unreadCount,
    isConnected,
    addNotification,
    markAsRead,
    markAllAsRead,
    clearNotifications,
  };

  return (
    <NotificationContext.Provider value={value}>
      {children}
    </NotificationContext.Provider>
  );
};
