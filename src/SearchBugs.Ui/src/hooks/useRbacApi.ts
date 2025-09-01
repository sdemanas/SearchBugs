import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { useToast } from "@/components/ui/use-toast";

export interface Role {
  id: number;
  name: string;
  permissions: Permission[];
}

export interface Permission {
  id: number;
  name: string;
  description: string;
}

export interface RoleWithPermissions extends Role {
  permissions: Permission[];
}

export interface User {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  fullName: string;
  roles: UserRole[];
}

export interface UserRole {
  id: number;
  name: string;
  permissions: Permission[];
}

export interface UserRolesResponse {
  userId: string;
  email: string;
  fullName: string;
  roles: RoleDto[];
}

export interface RoleDto {
  id: number;
  name: string;
  permissions: PermissionDto[];
}

export interface PermissionDto {
  id: number;
  name: string;
  description: string;
}

// Helper function to get auth headers
const getAuthHeaders = () => ({
  Authorization: `Bearer ${localStorage.getItem("access")}`,
});

export const useRbacApi = () => {
  const queryClient = useQueryClient();
  const { toast } = useToast();

  // Roles queries
  const useRoles = () => {
    return useQuery<Role[]>({
      queryKey: ["roles"],
      queryFn: async () => {
        const response = await fetch("/api/roles", {
          headers: getAuthHeaders(),
        });
        if (!response.ok) throw new Error("Failed to fetch roles");
        const data = await response.json();
        return data.value || data;
      },
      staleTime: 5 * 60 * 1000, // 5 minutes
    });
  };

  const useRoleWithPermissions = (roleId: number | undefined) => {
    return useQuery<RoleWithPermissions>({
      queryKey: ["role", roleId],
      queryFn: async () => {
        if (!roleId) throw new Error("Role ID is required");
        const response = await fetch(`/api/roles/${roleId}`, {
          headers: getAuthHeaders(),
        });
        if (!response.ok) throw new Error("Failed to fetch role permissions");
        const data = await response.json();
        return data.value || data;
      },
      enabled: !!roleId,
      staleTime: 5 * 60 * 1000, // 5 minutes
    });
  };

  const usePermissions = () => {
    return useQuery<Permission[]>({
      queryKey: ["permissions"],
      queryFn: async () => {
        const response = await fetch("/api/roles/permissions", {
          headers: getAuthHeaders(),
        });
        if (!response.ok) throw new Error("Failed to fetch permissions");
        const data = await response.json();
        return data.value || data;
      },
      staleTime: 10 * 60 * 1000, // 10 minutes - permissions change rarely
    });
  };

  const useRolesWithPermissions = () => {
    return useQuery<RoleWithPermissions[]>({
      queryKey: ["roles-with-permissions"],
      queryFn: async () => {
        const response = await fetch("/api/roles", {
          headers: getAuthHeaders(),
        });
        if (!response.ok) throw new Error("Failed to fetch roles");
        const rolesData = await response.json();
        const rolesValue = rolesData.value || rolesData;

        // Fetch detailed permissions for each role
        const rolesWithPermissions = await Promise.all(
          rolesValue.map(async (role: Role) => {
            const roleResponse = await fetch(`/api/roles/${role.id}`, {
              headers: getAuthHeaders(),
            });
            if (roleResponse.ok) {
              const roleData = await roleResponse.json();
              return roleData.value || roleData;
            }
            return { ...role, permissions: [] };
          })
        );

        return rolesWithPermissions;
      },
      staleTime: 5 * 60 * 1000, // 5 minutes
    });
  };

  // Permission mutations
  const useAssignPermission = () => {
    return useMutation({
      mutationFn: async ({
        roleId,
        permissionId,
      }: {
        roleId: number;
        permissionId: number;
      }) => {
        const response = await fetch(
          `/api/roles/${roleId}/permissions/${permissionId}`,
          {
            method: "POST",
            headers: getAuthHeaders(),
          }
        );
        if (!response.ok) throw new Error("Failed to assign permission");
        return response.json();
      },
      onSuccess: (_, { roleId }) => {
        queryClient.invalidateQueries({ queryKey: ["role", roleId] });
        queryClient.invalidateQueries({ queryKey: ["roles-with-permissions"] });
        toast({
          title: "Permission Assigned",
          description: "Permission has been successfully assigned to the role.",
        });
      },
      onError: (error: Error) => {
        toast({
          title: "Error",
          description: `Failed to assign permission: ${error.message}`,
          variant: "destructive",
        });
      },
    });
  };

  const useRemovePermission = () => {
    return useMutation({
      mutationFn: async ({
        roleId,
        permissionId,
      }: {
        roleId: number;
        permissionId: number;
      }) => {
        const response = await fetch(
          `/api/roles/${roleId}/permissions/${permissionId}`,
          {
            method: "DELETE",
            headers: getAuthHeaders(),
          }
        );
        if (!response.ok) throw new Error("Failed to remove permission");
        return response.json();
      },
      onSuccess: (_, { roleId }) => {
        queryClient.invalidateQueries({ queryKey: ["role", roleId] });
        queryClient.invalidateQueries({ queryKey: ["roles-with-permissions"] });
        toast({
          title: "Permission Removed",
          description:
            "Permission has been successfully removed from the role.",
        });
      },
      onError: (error: Error) => {
        toast({
          title: "Error",
          description: `Failed to remove permission: ${error.message}`,
          variant: "destructive",
        });
      },
    });
  };

  // User queries
  const useUsers = (searchTerm?: string) => {
    return useQuery<User[]>({
      queryKey: ["users", searchTerm],
      queryFn: async () => {
        const params = new URLSearchParams();
        if (searchTerm) params.append("searchTerm", searchTerm);

        const response = await fetch(`/api/users?${params.toString()}`, {
          headers: getAuthHeaders(),
        });

        if (!response.ok) throw new Error("Failed to fetch users");
        const data = await response.json();
        return data.value?.users || data.users || data;
      },
      staleTime: 2 * 60 * 1000, // 2 minutes
    });
  };

  const useUserRoles = (userId: string | undefined) => {
    return useQuery<UserRolesResponse>({
      queryKey: ["user-roles", userId],
      queryFn: async () => {
        if (!userId) throw new Error("User ID is required");
        const response = await fetch(`/api/users/${userId}/roles`, {
          headers: getAuthHeaders(),
        });
        if (!response.ok) throw new Error("Failed to fetch user roles");
        const data = await response.json();
        return data.value || data;
      },
      enabled: !!userId,
      staleTime: 3 * 60 * 1000, // 3 minutes
    });
  };

  // User role mutations
  const useAssignRole = () => {
    return useMutation({
      mutationFn: async ({
        userId,
        roleId,
      }: {
        userId: string;
        roleId: number;
      }) => {
        const response = await fetch(`/api/users/${userId}/roles`, {
          method: "POST",
          headers: {
            "Content-Type": "application/json",
            ...getAuthHeaders(),
          },
          body: JSON.stringify({ userId, roleId }),
        });
        if (!response.ok) throw new Error("Failed to assign role");
        return response.json();
      },
      onSuccess: (_, { userId }) => {
        queryClient.invalidateQueries({
          queryKey: ["user-roles", userId],
        });
        queryClient.invalidateQueries({ queryKey: ["users"] });
        toast({
          title: "Role Assigned",
          description: "Role has been successfully assigned to the user.",
        });
      },
      onError: (error: Error) => {
        toast({
          title: "Error",
          description: `Failed to assign role: ${error.message}`,
          variant: "destructive",
        });
      },
    });
  };

  const useRemoveRole = () => {
    return useMutation({
      mutationFn: async ({
        userId,
        roleId,
      }: {
        userId: string;
        roleId: number;
      }) => {
        const response = await fetch(`/api/users/${userId}/roles/${roleId}`, {
          method: "DELETE",
          headers: getAuthHeaders(),
        });
        if (!response.ok) throw new Error("Failed to remove role");
        return response.json();
      },
      onSuccess: (_, { userId }) => {
        queryClient.invalidateQueries({
          queryKey: ["user-roles", userId],
        });
        queryClient.invalidateQueries({ queryKey: ["users"] });
        toast({
          title: "Role Removed",
          description: "Role has been successfully removed from the user.",
        });
      },
      onError: (error: Error) => {
        toast({
          title: "Error",
          description: `Failed to remove role: ${error.message}`,
          variant: "destructive",
        });
      },
    });
  };

  return {
    // Role queries
    useRoles,
    useRoleWithPermissions,
    usePermissions,
    useRolesWithPermissions,

    // Permission mutations
    useAssignPermission,
    useRemovePermission,

    // User queries
    useUsers,
    useUserRoles,

    // User role mutations
    useAssignRole,
    useRemoveRole,
  };
};
