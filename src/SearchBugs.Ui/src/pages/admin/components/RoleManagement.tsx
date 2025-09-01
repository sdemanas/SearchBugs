import React, { useState } from "react";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { Checkbox } from "@/components/ui/checkbox";
import { Badge } from "@/components/ui/badge";
import { Skeleton } from "@/components/ui/skeleton";
import { AlertCircle, Check, X, Shield, Settings, Users } from "lucide-react";
import { Alert, AlertDescription } from "@/components/ui/alert";
import {
  useRbacApi,
  type RoleWithPermissions,
  type Permission,
} from "@/hooks/useRbacApi";

const getRoleBadgeColor = (roleName: string) => {
  switch (roleName.toLowerCase()) {
    case "admin":
      return "bg-red-100 text-red-800 hover:bg-red-200";
    case "project manager":
      return "bg-blue-100 text-blue-800 hover:bg-blue-200";
    case "developer":
      return "bg-green-100 text-green-800 hover:bg-green-200";
    case "reporter":
      return "bg-yellow-100 text-yellow-800 hover:bg-yellow-200";
    case "guest":
      return "bg-gray-100 text-gray-800 hover:bg-gray-200";
    default:
      return "bg-purple-100 text-purple-800 hover:bg-purple-200";
  }
};

const getRoleIcon = (roleName: string) => {
  switch (roleName.toLowerCase()) {
    case "admin":
      return <Shield className="h-4 w-4" />;
    case "project manager":
      return <Settings className="h-4 w-4" />;
    default:
      return <Users className="h-4 w-4" />;
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

export const RoleManagement: React.FC = () => {
  const rbacApi = useRbacApi();
  const [selectedRole, setSelectedRole] = useState<RoleWithPermissions | null>(
    null
  );

  // Fetch roles and permissions using the new hook
  const {
    data: roles,
    isLoading: rolesLoading,
    error: rolesError,
  } = rbacApi.useRoles();

  const { data: permissions, isLoading: permissionsLoading } =
    rbacApi.usePermissions();

  // Fetch role with permissions when a role is selected
  const { data: roleWithPermissions, isLoading: rolePermissionsLoading } =
    rbacApi.useRoleWithPermissions(selectedRole?.id);

  // Permission mutations
  const assignPermission = rbacApi.useAssignPermission();
  const removePermission = rbacApi.useRemovePermission();

  const handlePermissionChange = (
    permission: Permission,
    isChecked: boolean
  ) => {
    if (!selectedRole) return;

    if (isChecked) {
      assignPermission.mutate({
        roleId: selectedRole.id,
        permissionId: permission.id,
      });
    } else {
      removePermission.mutate({
        roleId: selectedRole.id,
        permissionId: permission.id,
      });
    }
  };

  if (rolesError) {
    return (
      <Alert variant="destructive">
        <AlertCircle className="h-4 w-4" />
        <AlertDescription>
          Failed to load roles. Please try again later.
        </AlertDescription>
      </Alert>
    );
  }

  return (
    <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
      {/* Roles List */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <Shield className="h-5 w-5" />
            System Roles
          </CardTitle>
          <CardDescription>
            Select a role to manage its permissions
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-3">
          {rolesLoading
            ? Array.from({ length: 5 }).map((_, i) => (
                <div key={i} className="flex items-center space-x-3">
                  <Skeleton className="h-4 w-4" />
                  <Skeleton className="h-4 flex-1" />
                </div>
              ))
            : roles?.map((role) => (
                <div
                  key={role.id}
                  className={`p-3 rounded-lg border cursor-pointer transition-colors ${
                    selectedRole?.id === role.id
                      ? "border-blue-500 bg-blue-50 dark:bg-blue-950"
                      : "border-gray-200 hover:border-gray-300 hover:bg-gray-50 dark:hover:bg-gray-800"
                  }`}
                  onClick={() => setSelectedRole(role)}
                >
                  <div className="flex items-center justify-between">
                    <div className="flex items-center gap-2">
                      {getRoleIcon(role.name)}
                      <span className="font-medium">{role.name}</span>
                    </div>
                    <Badge className={getRoleBadgeColor(role.name)}>
                      {role.permissions?.length || 0} permissions
                    </Badge>
                  </div>
                </div>
              ))}
        </CardContent>
      </Card>

      {/* Role Permissions */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <Settings className="h-5 w-5" />
            Role Permissions
            {selectedRole && (
              <Badge className={getRoleBadgeColor(selectedRole.name)}>
                {selectedRole.name}
              </Badge>
            )}
          </CardTitle>
          <CardDescription>
            {selectedRole
              ? `Manage permissions for ${selectedRole.name} role`
              : "Select a role to view and manage its permissions"}
          </CardDescription>
        </CardHeader>
        <CardContent>
          {!selectedRole ? (
            <div className="text-center text-muted-foreground py-8">
              <Shield className="h-12 w-12 mx-auto mb-4 opacity-50" />
              <p>Select a role from the list to manage its permissions</p>
            </div>
          ) : rolePermissionsLoading || permissionsLoading ? (
            <div className="space-y-6">
              {Array.from({ length: 3 }).map((_, i) => (
                <div key={i} className="space-y-3">
                  <Skeleton className="h-5 w-32" />
                  {Array.from({ length: 4 }).map((_, j) => (
                    <div key={j} className="flex items-center space-x-3">
                      <Skeleton className="h-4 w-4" />
                      <Skeleton className="h-4 flex-1" />
                    </div>
                  ))}
                </div>
              ))}
            </div>
          ) : (
            <div className="space-y-6">
              {permissions &&
                Object.entries(groupPermissionsByCategory(permissions)).map(
                  ([category, categoryPermissions]) => (
                    <div key={category} className="space-y-3">
                      <h4 className="font-semibold text-sm text-gray-900 dark:text-gray-100 border-b pb-1">
                        {category}
                      </h4>
                      <div className="grid gap-3">
                        {categoryPermissions.map((permission) => {
                          const isAssigned =
                            roleWithPermissions?.permissions?.some(
                              (p) => p.id === permission.id
                            ) || false;
                          const isLoading =
                            assignPermission.isPending ||
                            removePermission.isPending;

                          return (
                            <div
                              key={permission.id}
                              className="flex items-start space-x-3 p-2 rounded-md hover:bg-gray-50 dark:hover:bg-gray-800"
                            >
                              <Checkbox
                                checked={isAssigned}
                                disabled={isLoading}
                                onCheckedChange={(checked) =>
                                  handlePermissionChange(permission, !!checked)
                                }
                                className="mt-0.5"
                              />
                              <div className="flex-1 min-w-0">
                                <div className="flex items-center gap-2">
                                  <span className="font-medium text-sm">
                                    {permission.name}
                                  </span>
                                  {isAssigned ? (
                                    <Check className="h-3 w-3 text-green-500" />
                                  ) : (
                                    <X className="h-3 w-3 text-gray-400" />
                                  )}
                                </div>
                                <p className="text-xs text-muted-foreground mt-1">
                                  {permission.description}
                                </p>
                              </div>
                            </div>
                          );
                        })}
                      </div>
                    </div>
                  )
                )}
            </div>
          )}
        </CardContent>
      </Card>
    </div>
  );
};
