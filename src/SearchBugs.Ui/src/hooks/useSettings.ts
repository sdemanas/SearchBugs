// Settings functionality hooks and services
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import { apiClient } from "@/lib/api";

// Types for different settings
export interface SecuritySettings {
  twoFactorEnabled: boolean;
  lastPasswordChange: string;
  activeTokens: number;
  loginHistory: Array<{
    id: string;
    location: string;
    device: string;
    timestamp: string;
    ipAddress: string;
  }>;
}

export interface NotificationSettings {
  emailNotifications: {
    bugAssignments: boolean;
    bugUpdates: boolean;
    projectUpdates: boolean;
    weeklyDigest: boolean;
    marketing: boolean;
  };
  pushNotifications: {
    bugAssignments: boolean;
    bugUpdates: boolean;
    mentions: boolean;
  };
  frequency: "immediately" | "daily" | "weekly";
}

export interface IntegrationSettings {
  github: {
    connected: boolean;
    username?: string;
    repositories: string[];
  };
  slack: {
    connected: boolean;
    workspaceName?: string;
    channels: string[];
  };
  discord: {
    connected: boolean;
    serverId?: string;
    username?: string;
  };
  jira: {
    connected: boolean;
    instance?: string;
    projects: string[];
  };
}

// Security Settings Hook
export const useSecuritySettings = () => {
  const queryClient = useQueryClient();

  const { data: securitySettings, isLoading } = useQuery({
    queryKey: ["security-settings"],
    queryFn: async (): Promise<SecuritySettings> => {
      // Mock API call - replace with actual API endpoint
      return {
        twoFactorEnabled: false,
        lastPasswordChange: "2024-05-15",
        activeTokens: 3,
        loginHistory: [
          {
            id: "1",
            location: "San Francisco, CA",
            device: "Chrome on macOS",
            timestamp: "2024-08-30T10:30:00Z",
            ipAddress: "192.168.1.1",
          },
          {
            id: "2",
            location: "New York, NY",
            device: "Safari on iOS",
            timestamp: "2024-08-29T15:20:00Z",
            ipAddress: "192.168.1.2",
          },
        ],
      };
    },
  });

  const updatePasswordMutation = useMutation({
    mutationFn: async (data: {
      currentPassword: string;
      newPassword: string;
    }) => {
      // Replace with actual API call
      await new Promise((resolve) => setTimeout(resolve, 1000));
      return { success: true };
    },
    onSuccess: () => {
      toast.success("Password updated successfully!");
      queryClient.invalidateQueries({ queryKey: ["security-settings"] });
    },
    onError: () => {
      toast.error("Failed to update password");
    },
  });

  const toggle2FAMutation = useMutation({
    mutationFn: async (enable: boolean) => {
      // Replace with actual API call
      await new Promise((resolve) => setTimeout(resolve, 1000));
      return { success: true, enabled: enable };
    },
    onSuccess: (data) => {
      toast.success(
        `Two-factor authentication ${data.enabled ? "enabled" : "disabled"}!`
      );
      queryClient.invalidateQueries({ queryKey: ["security-settings"] });
    },
    onError: () => {
      toast.error("Failed to update two-factor authentication");
    },
  });

  const revokeTokenMutation = useMutation({
    mutationFn: async (tokenId: string) => {
      // Replace with actual API call
      await new Promise((resolve) => setTimeout(resolve, 500));
      return { success: true };
    },
    onSuccess: () => {
      toast.success("Token revoked successfully!");
      queryClient.invalidateQueries({ queryKey: ["security-settings"] });
    },
    onError: () => {
      toast.error("Failed to revoke token");
    },
  });

  return {
    securitySettings,
    isLoading,
    updatePassword: updatePasswordMutation.mutate,
    isUpdatingPassword: updatePasswordMutation.isPending,
    toggle2FA: toggle2FAMutation.mutate,
    isToggling2FA: toggle2FAMutation.isPending,
    revokeToken: revokeTokenMutation.mutate,
    isRevokingToken: revokeTokenMutation.isPending,
  };
};

// Notification Settings Hook
export const useNotificationSettings = () => {
  const queryClient = useQueryClient();

  const { data: notificationSettings, isLoading } = useQuery({
    queryKey: ["notification-settings"],
    queryFn: async (): Promise<NotificationSettings> => {
      // Mock API call - replace with actual API endpoint
      return {
        emailNotifications: {
          bugAssignments: true,
          bugUpdates: true,
          projectUpdates: false,
          weeklyDigest: true,
          marketing: false,
        },
        pushNotifications: {
          bugAssignments: true,
          bugUpdates: false,
          mentions: true,
        },
        frequency: "immediately",
      };
    },
  });

  const updateNotificationsMutation = useMutation({
    mutationFn: async (settings: Partial<NotificationSettings>) => {
      // Replace with actual API call
      await new Promise((resolve) => setTimeout(resolve, 1000));
      return { success: true };
    },
    onSuccess: () => {
      toast.success("Notification preferences updated!");
      queryClient.invalidateQueries({ queryKey: ["notification-settings"] });
    },
    onError: () => {
      toast.error("Failed to update notification preferences");
    },
  });

  return {
    notificationSettings,
    isLoading,
    updateNotifications: updateNotificationsMutation.mutate,
    isUpdating: updateNotificationsMutation.isPending,
  };
};

// Integration Settings Hook
export const useIntegrationSettings = () => {
  const queryClient = useQueryClient();

  const { data: integrationSettings, isLoading } = useQuery({
    queryKey: ["integration-settings"],
    queryFn: async (): Promise<IntegrationSettings> => {
      // Mock API call - replace with actual API endpoint
      return {
        github: {
          connected: true,
          username: "john-doe",
          repositories: ["searchbugs-app", "bug-tracker"],
        },
        slack: {
          connected: false,
          workspaceName: undefined,
          channels: [],
        },
        discord: {
          connected: false,
          serverId: undefined,
          username: undefined,
        },
        jira: {
          connected: true,
          instance: "mycompany.atlassian.net",
          projects: ["PROJ-1", "PROJ-2"],
        },
      };
    },
  });

  const connectIntegrationMutation = useMutation({
    mutationFn: async ({
      service,
      config,
    }: {
      service: string;
      config: any;
    }) => {
      // Replace with actual API call
      await new Promise((resolve) => setTimeout(resolve, 1500));
      return { success: true };
    },
    onSuccess: (_, { service }) => {
      toast.success(`${service} connected successfully!`);
      queryClient.invalidateQueries({ queryKey: ["integration-settings"] });
    },
    onError: (_, { service }) => {
      toast.error(`Failed to connect ${service}`);
    },
  });

  const disconnectIntegrationMutation = useMutation({
    mutationFn: async (service: string) => {
      // Replace with actual API call
      await new Promise((resolve) => setTimeout(resolve, 1000));
      return { success: true };
    },
    onSuccess: (_, service) => {
      toast.success(`${service} disconnected successfully!`);
      queryClient.invalidateQueries({ queryKey: ["integration-settings"] });
    },
    onError: (_, service) => {
      toast.error(`Failed to disconnect ${service}`);
    },
  });

  return {
    integrationSettings,
    isLoading,
    connectIntegration: connectIntegrationMutation.mutate,
    isConnecting: connectIntegrationMutation.isPending,
    disconnectIntegration: disconnectIntegrationMutation.mutate,
    isDisconnecting: disconnectIntegrationMutation.isPending,
  };
};

// Export Data Hook
export const useDataExport = () => {
  const exportDataMutation = useMutation({
    mutationFn: async (format: "json" | "csv") => {
      // Replace with actual API call
      await new Promise((resolve) => setTimeout(resolve, 2000));

      // Mock data export
      const mockData = {
        profile: { name: "John Doe", email: "john@example.com" },
        bugs: [],
        projects: [],
        exportedAt: new Date().toISOString(),
      };

      // Create and download file
      const blob = new Blob([JSON.stringify(mockData, null, 2)], {
        type: "application/json",
      });
      const url = URL.createObjectURL(blob);
      const a = document.createElement("a");
      a.href = url;
      a.download = `searchbugs-data-export-${Date.now()}.${format}`;
      document.body.appendChild(a);
      a.click();
      document.body.removeChild(a);
      URL.revokeObjectURL(url);

      return { success: true };
    },
    onSuccess: () => {
      toast.success("Data export started! Your download should begin shortly.");
    },
    onError: () => {
      toast.error("Failed to export data");
    },
  });

  return {
    exportData: exportDataMutation.mutate,
    isExporting: exportDataMutation.isPending,
  };
};

// Account Deletion Hook
export const useAccountDeletion = () => {
  const deleteAccountMutation = useMutation({
    mutationFn: async (confirmation: {
      password: string;
      confirmText: string;
    }) => {
      if (confirmation.confirmText !== "DELETE MY ACCOUNT") {
        throw new Error("Confirmation text does not match");
      }
      // Replace with actual API call
      await new Promise((resolve) => setTimeout(resolve, 2000));
      return { success: true };
    },
    onSuccess: () => {
      toast.success(
        "Account deletion initiated. You will receive a confirmation email."
      );
      // Redirect to home or logout
      window.location.href = "/";
    },
    onError: (error: any) => {
      toast.error(error.message || "Failed to delete account");
    },
  });

  return {
    deleteAccount: deleteAccountMutation.mutate,
    isDeletingAccount: deleteAccountMutation.isPending,
  };
};
