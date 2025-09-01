import React, {
  createContext,
  useEffect,
  useState,
  ReactNode,
  useCallback,
} from "react";
import {
  notificationService,
  NotificationData,
} from "../services/notificationService";
import { useAuthStore } from "../stores/global/authStore";
import { apiClient } from "../lib/api";

interface NotificationContextType {
  notifications: NotificationData[];
  unreadCount: number;
  isConnected: boolean;
  addNotification: (notification: NotificationData) => void;
  markAsRead: (notificationId: string) => void;
  markAllAsRead: () => void;
  clearNotifications: () => void;
  loadNotifications: () => Promise<void>;
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
  const [token, setToken] = useState<string | null>(null);

  // Get token from auth store
  useEffect(() => {
    const authToken = localStorage.getItem("access"); // Use the correct key from api.ts
    setToken(authToken);
  }, [isAuthenticated]);

  useEffect(() => {
    const initializeSignalR = async () => {
      if (isAuthenticated && user && token) {
        try {
          await notificationService.startConnection(token);
          await notificationService.joinUserGroup(user.id);
          setIsConnected(true);

          // Set up notification listeners
          const handleNotification = (data: NotificationData) => {
            console.log("Received notification:", data);
            addNotification(data);
          };

          const handleBugNotification = (data: NotificationData) => {
            console.log("Received bug notification:", data);
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

          // Set up connection state handlers
          const connection = notificationService.getConnection();
          if (connection) {
            connection.onreconnecting(() => {
              console.log("SignalR reconnecting...");
              setIsConnected(false);
            });

            connection.onreconnected(() => {
              console.log("SignalR reconnected");
              setIsConnected(true);
              // Rejoin user group after reconnection
              notificationService.joinUserGroup(user.id);
            });

            connection.onclose(() => {
              console.log("SignalR connection closed");
              setIsConnected(false);
            });
          }

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
  }, [isAuthenticated, user, token]);

  useEffect(() => {
    return () => {
      notificationService.stopConnection();
    };
  }, []);

  const addNotification = (notification: NotificationData) => {
    const notificationWithId = {
      ...notification,
      id: notification.id || Date.now().toString(),
      timestamp:
        notification.timestamp ||
        notification.createdAt ||
        new Date().toISOString(),
    };

    setNotifications((prev) => {
      // Check if notification already exists to avoid duplicates
      const exists = prev.find((n) => n.id === notificationWithId.id);
      if (exists) {
        return prev;
      }

      // Add new notification to the beginning and limit to 50 notifications
      const updated = [notificationWithId, ...prev].slice(0, 50);
      return updated;
    });
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

  const loadNotifications = useCallback(async () => {
    if (!isAuthenticated) return;

    try {
      // Load recent notifications from the backend
      const response = await apiClient.notifications.getAll({ pageSize: 50 });
      if (response.data.isSuccess) {
        const backendNotifications = response.data.value.notifications.map(
          (n) => ({
            id: n.id,
            type: n.type,
            message: n.message,
            data: n.data || undefined,
            bugId: n.bugId || undefined,
            isRead: n.isRead,
            timestamp: n.createdAt,
            createdAt: n.createdAt,
          })
        );

        // Merge with existing SignalR notifications, avoiding duplicates
        setNotifications((prev) => {
          const existingIds = new Set(prev.map((n) => n.id));
          const newNotifications = backendNotifications.filter(
            (n) => !existingIds.has(n.id)
          );

          // Combine and sort by timestamp (newest first)
          const combined = [...prev, ...newNotifications].sort(
            (a, b) =>
              new Date(b.timestamp || b.createdAt || 0).getTime() -
              new Date(a.timestamp || a.createdAt || 0).getTime()
          );

          // Limit to 50 notifications
          return combined.slice(0, 50);
        });
      }
    } catch (error) {
      console.error("Failed to load notifications:", error);
    }
  }, [isAuthenticated]);

  // Load initial notifications when user is authenticated
  useEffect(() => {
    if (isAuthenticated && user) {
      loadNotifications();
    }
  }, [isAuthenticated, user, loadNotifications]);

  const unreadCount = notifications.filter((n) => !n.isRead).length;

  const value: NotificationContextType = {
    notifications,
    unreadCount,
    isConnected,
    addNotification,
    markAsRead,
    markAllAsRead,
    clearNotifications,
    loadNotifications,
  };

  return (
    <NotificationContext.Provider value={value}>
      {children}
    </NotificationContext.Provider>
  );
};
