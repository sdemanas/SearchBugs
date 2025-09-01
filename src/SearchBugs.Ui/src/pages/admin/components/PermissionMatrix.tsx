import React from "react";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Check, X, Shield, Lock, AlertCircle } from "lucide-react";
import { Alert, AlertDescription } from "@/components/ui/alert";
import {
  useRbacApi,
  type RoleWithPermissions,
  type Permission,
} from "@/hooks/useRbacApi";

const getRoleBadgeColor = (roleName: string) => {
  switch (roleName.toLowerCase()) {
    case "admin":
      return "bg-red-100 text-red-800";
    case "project manager":
      return "bg-blue-100 text-blue-800";
    case "developer":
      return "bg-green-100 text-green-800";
    case "reporter":
      return "bg-yellow-100 text-yellow-800";
    case "guest":
      return "bg-gray-100 text-gray-800";
    default:
      return "bg-purple-100 text-purple-800";
  }
};

const groupPermissionsByCategory = (permissions: Permission[]) => {
  const groups: { [key: string]: Permission[] } = {};

  permissions.forEach((permission) => {
    const category = permission.name.includes("User")
      ? "User Management"
      : permission.name.includes("Project")
      ? "Project Management"
      : permission.name.includes("Bug")
      ? "Bug Management"
      : permission.name.includes("Repository")
      ? "Repository Management"
      : permission.name.includes("Notification")
      ? "Notifications"
      : permission.name.includes("Role") ||
        permission.name.includes("Permission")
      ? "Role & Permission Management"
      : "Other";

    if (!groups[category]) {
      groups[category] = [];
    }
    groups[category].push(permission);
  });

  return groups;
};

export const PermissionMatrix: React.FC = () => {
  const rbacApi = useRbacApi();

  // Fetch all roles with permissions and all permissions using the new hook
  const {
    data: roles,
    isLoading: rolesLoading,
    error: rolesError,
  } = rbacApi.useRolesWithPermissions();

  const { data: permissions, isLoading: permissionsLoading } =
    rbacApi.usePermissions();

  const hasPermission = (
    role: RoleWithPermissions,
    permissionId: number
  ): boolean => {
    return role.permissions?.some((p) => p.id === permissionId) || false;
  };

  if (rolesError) {
    return (
      <Alert variant="destructive">
        <AlertCircle className="h-4 w-4" />
        <AlertDescription>
          Failed to load permission matrix. Please try again later.
        </AlertDescription>
      </Alert>
    );
  }

  if (rolesLoading || permissionsLoading) {
    return (
      <div className="space-y-4">
        <div className="flex items-center gap-2">
          <Shield className="h-5 w-5 animate-spin" />
          <span>Loading permission matrix...</span>
        </div>
        <div className="bg-gray-100 dark:bg-gray-800 animate-pulse h-96 rounded-lg" />
      </div>
    );
  }

  if (!roles || !permissions) {
    return (
      <Alert>
        <AlertCircle className="h-4 w-4" />
        <AlertDescription>
          No data available to display the permission matrix.
        </AlertDescription>
      </Alert>
    );
  }

  const permissionGroups = groupPermissionsByCategory(permissions);

  return (
    <div className="space-y-6">
      {/* Summary Cards */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
        <Card>
          <CardContent className="flex items-center p-6">
            <div className="flex items-center gap-3">
              <div className="p-2 bg-blue-100 dark:bg-blue-900 rounded-lg">
                <Shield className="h-5 w-5 text-blue-600 dark:text-blue-400" />
              </div>
              <div>
                <p className="text-2xl font-bold">{roles.length}</p>
                <p className="text-sm text-muted-foreground">Total Roles</p>
              </div>
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardContent className="flex items-center p-6">
            <div className="flex items-center gap-3">
              <div className="p-2 bg-green-100 dark:bg-green-900 rounded-lg">
                <Lock className="h-5 w-5 text-green-600 dark:text-green-400" />
              </div>
              <div>
                <p className="text-2xl font-bold">{permissions.length}</p>
                <p className="text-sm text-muted-foreground">
                  Total Permissions
                </p>
              </div>
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardContent className="flex items-center p-6">
            <div className="flex items-center gap-3">
              <div className="p-2 bg-purple-100 dark:bg-purple-900 rounded-lg">
                <Shield className="h-5 w-5 text-purple-600 dark:text-purple-400" />
              </div>
              <div>
                <p className="text-2xl font-bold">
                  {Object.keys(permissionGroups).length}
                </p>
                <p className="text-sm text-muted-foreground">
                  Permission Categories
                </p>
              </div>
            </div>
          </CardContent>
        </Card>
      </div>

      {/* Permission Matrix */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <Shield className="h-5 w-5" />
            Permission Matrix
          </CardTitle>
          <CardDescription>
            View all role-permission assignments in a comprehensive matrix
            format.
          </CardDescription>
        </CardHeader>
        <CardContent>
          <div className="space-y-8">
            {Object.entries(permissionGroups).map(
              ([category, categoryPermissions]) => (
                <div key={category} className="space-y-4">
                  <h3 className="text-lg font-semibold text-gray-900 dark:text-gray-100 border-b pb-2">
                    {category}
                  </h3>

                  <div className="overflow-x-auto">
                    <table className="w-full min-w-full">
                      <thead>
                        <tr>
                          <th className="text-left p-3 font-medium text-gray-900 dark:text-gray-100 border-b">
                            Permission
                          </th>
                          {roles.map((role) => (
                            <th
                              key={role.id}
                              className="text-center p-3 border-b min-w-[120px]"
                            >
                              <Badge className={getRoleBadgeColor(role.name)}>
                                {role.name}
                              </Badge>
                            </th>
                          ))}
                        </tr>
                      </thead>
                      <tbody>
                        {categoryPermissions.map((permission) => (
                          <tr
                            key={permission.id}
                            className="border-b hover:bg-gray-50 dark:hover:bg-gray-800"
                          >
                            <td className="p-3">
                              <div>
                                <p className="font-medium text-sm">
                                  {permission.name}
                                </p>
                                <p className="text-xs text-muted-foreground mt-1">
                                  {permission.description}
                                </p>
                              </div>
                            </td>
                            {roles.map((role) => (
                              <td
                                key={`${role.id}-${permission.id}`}
                                className="p-3 text-center"
                              >
                                <div className="flex justify-center">
                                  {hasPermission(role, permission.id) ? (
                                    <div className="flex items-center justify-center w-6 h-6 bg-green-100 dark:bg-green-900 rounded-full">
                                      <Check className="h-4 w-4 text-green-600 dark:text-green-400" />
                                    </div>
                                  ) : (
                                    <div className="flex items-center justify-center w-6 h-6 bg-gray-100 dark:bg-gray-800 rounded-full">
                                      <X className="h-4 w-4 text-gray-400" />
                                    </div>
                                  )}
                                </div>
                              </td>
                            ))}
                          </tr>
                        ))}
                      </tbody>
                    </table>
                  </div>
                </div>
              )
            )}
          </div>

          {/* Legend */}
          <div className="mt-6 p-4 bg-gray-50 dark:bg-gray-900 rounded-lg">
            <h4 className="font-semibold text-sm mb-3">Legend</h4>
            <div className="flex items-center gap-6 text-sm">
              <div className="flex items-center gap-2">
                <div className="flex items-center justify-center w-6 h-6 bg-green-100 dark:bg-green-900 rounded-full">
                  <Check className="h-4 w-4 text-green-600 dark:text-green-400" />
                </div>
                <span>Permission granted</span>
              </div>
              <div className="flex items-center gap-2">
                <div className="flex items-center justify-center w-6 h-6 bg-gray-100 dark:bg-gray-800 rounded-full">
                  <X className="h-4 w-4 text-gray-400" />
                </div>
                <span>Permission not granted</span>
              </div>
            </div>
          </div>
        </CardContent>
      </Card>
    </div>
  );
};
