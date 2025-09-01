import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { apiClient } from "../lib/api";
import { useNotifications } from "./useNotifications";

// Hook for fetching unread count
export const useUnreadCountQuery = () => {
  return useQuery({
    queryKey: ["notifications", "unread-count"],
    queryFn: async () => {
      try {
        const response = await apiClient.notifications.getUnreadCount();
        if (response.data?.isSuccess) {
          return response.data.value;
        }
        // Return default structure if API fails
        return { count: 0 };
      } catch (error) {
        console.error("Failed to fetch unread count:", error);
        // Return default structure to prevent undefined errors
        return { count: 0 };
      }
    },
    staleTime: 30 * 1000, // 30 seconds
    refetchInterval: 60 * 1000, // Refetch every minute
    refetchOnWindowFocus: true,
  });
};

// Hook for fetching notifications list
export const useNotificationsQuery = (
  pageNumber = 1,
  pageSize = 10,
  isRead?: boolean
) => {
  return useQuery({
    queryKey: ["notifications", "list", { pageNumber, pageSize, isRead }],
    queryFn: async () => {
      try {
        const response = await apiClient.notifications.getAll({
          pageNumber,
          pageSize,
          isRead,
        });
        if (response.data?.isSuccess) {
          return response.data.value;
        }
        // Return default structure if API fails
        return {
          notifications: [],
          totalCount: 0,
          totalPages: 0,
          pageNumber: 1,
          pageSize: pageSize,
        };
      } catch (error) {
        console.error("Failed to fetch notifications:", error);
        // Return default structure to prevent undefined errors
        return {
          notifications: [],
          totalCount: 0,
          totalPages: 0,
          pageNumber: 1,
          pageSize: pageSize,
        };
      }
    },
    staleTime: 30 * 1000, // 30 seconds
  });
};

// Hook for notification mutations
export const useNotificationMutations = () => {
  const queryClient = useQueryClient();
  const { markAsRead, markAllAsRead } = useNotifications();

  const markAsReadMutation = useMutation({
    mutationFn: async (id: string) => {
      const response = await apiClient.notifications.markAsRead(id);
      return response.data;
    },
    onSuccess: (_, id) => {
      // Update local context
      markAsRead(id);
      // Invalidate React Query cache
      queryClient.invalidateQueries({ queryKey: ["notifications"] });
    },
    onError: (error) => {
      console.error("Failed to mark notification as read:", error);
    },
  });

  const markAllAsReadMutation = useMutation({
    mutationFn: async () => {
      const response = await apiClient.notifications.markAllAsRead();
      return response.data;
    },
    onSuccess: () => {
      // Update local context
      markAllAsRead();
      // Invalidate React Query cache
      queryClient.invalidateQueries({ queryKey: ["notifications"] });
    },
    onError: (error) => {
      console.error("Failed to mark all notifications as read:", error);
    },
  });

  const deleteNotificationMutation = useMutation({
    mutationFn: async (id: string) => {
      const response = await apiClient.notifications.delete(id);
      return response.data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["notifications"] });
    },
    onError: (error) => {
      console.error("Failed to delete notification:", error);
    },
  });

  const clearAllMutation = useMutation({
    mutationFn: async () => {
      const response = await apiClient.notifications.clearAll();
      return response.data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["notifications"] });
    },
    onError: (error) => {
      console.error("Failed to clear all notifications:", error);
    },
  });

  return {
    markAsReadMutation,
    markAllAsReadMutation,
    deleteNotificationMutation,
    clearAllMutation,
  };
};
