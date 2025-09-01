import React, { useState } from "react";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Input } from "@/components/ui/input";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import {
  Users,
  Search,
  UserPlus,
  UserMinus,
  Shield,
  AlertCircle,
} from "lucide-react";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { useRbacApi, type User, type UserRole } from "@/hooks/useRbacApi";

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

export const UserRoleManagement: React.FC = () => {
  const rbacApi = useRbacApi();
  const [searchTerm, setSearchTerm] = useState("");
  const [selectedUser, setSelectedUser] = useState<User | null>(null);
  const [selectedRoleToAssign, setSelectedRoleToAssign] = useState<string>("");

  // Fetch users and roles using the new hook
  const {
    data: users,
    isLoading: usersLoading,
    error: usersError,
  } = rbacApi.useUsers(searchTerm);

  const { data: roles } = rbacApi.useRoles();

  // Fetch user roles when a user is selected
  const { data: userRoles, isLoading: userRolesLoading } = rbacApi.useUserRoles(
    selectedUser?.id
  );

  // User role mutations
  const assignRole = rbacApi.useAssignRole();
  const removeRole = rbacApi.useRemoveRole();

  const handleAssignRole = () => {
    if (!selectedUser || !selectedRoleToAssign) return;

    const roleId = parseInt(selectedRoleToAssign);
    assignRole.mutate(
      { userId: selectedUser.id, roleId },
      {
        onSuccess: () => {
          setSelectedRoleToAssign("");
        },
      }
    );
  };

  const handleRemoveRole = (roleId: number) => {
    if (!selectedUser) return;

    removeRole.mutate({ userId: selectedUser.id, roleId });
  };

  if (usersError) {
    return (
      <Alert variant="destructive">
        <AlertCircle className="h-4 w-4" />
        <AlertDescription>
          Failed to load users. Please try again later.
        </AlertDescription>
      </Alert>
    );
  }

  return (
    <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
      {/* Users List */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <Users className="h-5 w-5" />
            System Users
          </CardTitle>
          <CardDescription>
            Select a user to manage their role assignments
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          {/* Search */}
          <div className="relative">
            <Search className="absolute left-3 top-2.5 h-4 w-4 text-muted-foreground" />
            <Input
              placeholder="Search users by name or email..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              className="pl-10"
            />
          </div>

          {/* Users List */}
          <div className="space-y-2 max-h-96 overflow-y-auto">
            {usersLoading ? (
              <div className="text-center py-4 text-muted-foreground">
                Loading users...
              </div>
            ) : users?.length === 0 ? (
              <div className="text-center py-4 text-muted-foreground">
                No users found
              </div>
            ) : (
              users?.map((user) => (
                <div
                  key={user.id}
                  className={`p-3 rounded-lg border cursor-pointer transition-colors ${
                    selectedUser?.id === user.id
                      ? "border-blue-500 bg-blue-50 dark:bg-blue-950"
                      : "border-gray-200 hover:border-gray-300 hover:bg-gray-50 dark:hover:bg-gray-800"
                  }`}
                  onClick={() => setSelectedUser(user)}
                >
                  <div className="flex flex-col space-y-2">
                    <div className="flex items-center justify-between">
                      <div>
                        <p className="font-medium">
                          {user.fullName ||
                            `${user.firstName} ${user.lastName}`}
                        </p>
                        <p className="text-sm text-muted-foreground">
                          {user.email}
                        </p>
                      </div>
                      <div className="text-right">
                        <div className="text-xs text-muted-foreground">
                          {user.roles?.length || 0} roles
                        </div>
                      </div>
                    </div>
                    <div className="flex flex-wrap gap-1">
                      {user.roles?.map((role) => (
                        <Badge
                          key={role.id}
                          className={getRoleBadgeColor(role.name)}
                        >
                          {role.name}
                        </Badge>
                      ))}
                    </div>
                  </div>
                </div>
              ))
            )}
          </div>
        </CardContent>
      </Card>

      {/* User Role Management */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <Shield className="h-5 w-5" />
            User Role Management
            {selectedUser && (
              <Badge variant="outline">
                {selectedUser.fullName ||
                  `${selectedUser.firstName} ${selectedUser.lastName}`}
              </Badge>
            )}
          </CardTitle>
          <CardDescription>
            {selectedUser
              ? `Manage roles for ${selectedUser.email}`
              : "Select a user to manage their roles"}
          </CardDescription>
        </CardHeader>
        <CardContent>
          {!selectedUser ? (
            <div className="text-center text-muted-foreground py-8">
              <Users className="h-12 w-12 mx-auto mb-4 opacity-50" />
              <p>Select a user from the list to manage their roles</p>
            </div>
          ) : (
            <div className="space-y-6">
              {/* Assign New Role */}
              <div className="space-y-3">
                <h4 className="font-semibold text-sm">Assign New Role</h4>
                <div className="flex gap-2">
                  <Select
                    value={selectedRoleToAssign}
                    onValueChange={setSelectedRoleToAssign}
                  >
                    <SelectTrigger className="flex-1">
                      <SelectValue placeholder="Select a role to assign" />
                    </SelectTrigger>
                    <SelectContent>
                      {roles
                        ?.filter(
                          (role) =>
                            !userRoles?.roles?.some(
                              (ur: UserRole) => ur.id === role.id
                            )
                        )
                        .map((role) => (
                          <SelectItem key={role.id} value={role.id.toString()}>
                            <div className="flex items-center gap-2">
                              <Badge className={getRoleBadgeColor(role.name)}>
                                {role.name}
                              </Badge>
                            </div>
                          </SelectItem>
                        ))}
                    </SelectContent>
                  </Select>
                  <Button
                    onClick={handleAssignRole}
                    disabled={!selectedRoleToAssign || assignRole.isPending}
                    className="shrink-0"
                  >
                    <UserPlus className="h-4 w-4 mr-2" />
                    Assign
                  </Button>
                </div>
              </div>

              {/* Current Roles */}
              <div className="space-y-3">
                <h4 className="font-semibold text-sm">Current Roles</h4>
                {userRolesLoading ? (
                  <div className="text-center py-4 text-muted-foreground">
                    Loading user roles...
                  </div>
                ) : userRoles?.roles?.length === 0 ? (
                  <div className="text-center py-4 text-muted-foreground">
                    No roles assigned to this user
                  </div>
                ) : (
                  <div className="space-y-3">
                    {userRoles?.roles?.map((role: UserRole) => (
                      <div
                        key={role.id}
                        className="flex items-center justify-between p-3 border rounded-lg"
                      >
                        <div className="flex items-center gap-3">
                          <Badge className={getRoleBadgeColor(role.name)}>
                            {role.name}
                          </Badge>
                          <div>
                            <p className="text-sm text-muted-foreground">
                              {role.permissions?.length || 0} permissions
                            </p>
                          </div>
                        </div>
                        <Button
                          variant="outline"
                          size="sm"
                          onClick={() => handleRemoveRole(role.id)}
                          disabled={removeRole.isPending}
                          className="text-red-600 hover:text-red-700 hover:bg-red-50"
                        >
                          <UserMinus className="h-4 w-4 mr-2" />
                          Remove
                        </Button>
                      </div>
                    ))}
                  </div>
                )}
              </div>

              {/* User Permissions Summary */}
              {userRoles?.roles && userRoles.roles.length > 0 && (
                <div className="space-y-3">
                  <h4 className="font-semibold text-sm">
                    Effective Permissions
                  </h4>
                  <div className="bg-gray-50 dark:bg-gray-900 p-3 rounded-lg">
                    <div className="text-sm text-muted-foreground">
                      This user has{" "}
                      {userRoles.roles.reduce(
                        (total: number, role: UserRole) =>
                          total + (role.permissions?.length || 0),
                        0
                      )}{" "}
                      total permissions from their assigned roles.
                    </div>
                  </div>
                </div>
              )}
            </div>
          )}
        </CardContent>
      </Card>
    </div>
  );
};
