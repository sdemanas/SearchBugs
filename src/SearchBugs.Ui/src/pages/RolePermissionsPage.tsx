import React, { useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Checkbox } from "@/components/ui/checkbox";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import {
  Shield,
  Search,
  Settings,
  User as UserIcon,
  UserCheck,
  UserCog,
  Users,
  Eye,
  X,
} from "lucide-react";
import { apiClient, Role, Permission, RoleWithPermissions } from "@/lib/api";
import { useToast } from "@/components/ui/use-toast";
import { CardLoadingSkeleton } from "@/components/ui/loading";

const roleIcons: Record<string, React.ReactNode> = {
  Admin: <Shield className="h-4 w-4" />,
  "Project Manager": <UserCog className="h-4 w-4" />,
  Developer: <UserCheck className="h-4 w-4" />,
  Reporter: <UserIcon className="h-4 w-4" />,
  Guest: <Eye className="h-4 w-4" />,
};

const roleColors: Record<string, string> = {
  Admin: "bg-red-100 text-red-800 hover:bg-red-200",
  "Project Manager": "bg-purple-100 text-purple-800 hover:bg-purple-200",
  Developer: "bg-blue-100 text-blue-800 hover:bg-blue-200",
  Reporter: "bg-green-100 text-green-800 hover:bg-green-200",
  Guest: "bg-gray-100 text-gray-800 hover:bg-gray-200",
};

interface ManagePermissionsDialogProps {
  role: RoleWithPermissions;
  allPermissions: Permission[];
  isOpen: boolean;
  onClose: () => void;
  onPermissionsChanged: () => void;
}

const ManagePermissionsDialog: React.FC<ManagePermissionsDialogProps> = ({
  role,
  allPermissions,
  isOpen,
  onClose,
  onPermissionsChanged,
}) => {
  const [searchTerm, setSearchTerm] = useState("");
  const queryClient = useQueryClient();
  const { toast } = useToast();

  const assignPermissionMutation = useMutation({
    mutationFn: async (permissionId: number) => {
      const response = await apiClient.roles.assignPermissionToRole(
        role.id,
        permissionId
      );
      return response.data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: ["roleWithPermissions", role.id],
      });
      onPermissionsChanged();
      toast({
        title: "Success",
        description: "Permission assigned successfully",
      });
    },
    onError: (error: Error) => {
      toast({
        title: "Error",
        description: error.message || "Failed to assign permission",
        variant: "destructive",
      });
    },
  });

  const removePermissionMutation = useMutation({
    mutationFn: async (permissionId: number) => {
      const response = await apiClient.roles.removePermissionFromRole(
        role.id,
        permissionId
      );
      return response.data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: ["roleWithPermissions", role.id],
      });
      onPermissionsChanged();
      toast({
        title: "Success",
        description: "Permission removed successfully",
      });
    },
    onError: (error: Error) => {
      toast({
        title: "Error",
        description: error.message || "Failed to remove permission",
        variant: "destructive",
      });
    },
  });

  const rolePermissionIds = role.permissions.map((p) => p.id);
  const filteredPermissions = allPermissions.filter(
    (permission) =>
      permission.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
      permission.description.toLowerCase().includes(searchTerm.toLowerCase())
  );

  const handlePermissionToggle = (
    permission: Permission,
    isChecked: boolean
  ) => {
    if (isChecked) {
      assignPermissionMutation.mutate(permission.id);
    } else {
      removePermissionMutation.mutate(permission.id);
    }
  };

  // Group permissions by category (based on permission name prefixes)
  const groupedPermissions = filteredPermissions.reduce(
    (groups, permission) => {
      let category = "General";

      if (permission.name.includes("User")) category = "User Management";
      else if (permission.name.includes("Project"))
        category = "Project Management";
      else if (permission.name.includes("Bug")) category = "Bug Management";
      else if (permission.name.includes("Repository"))
        category = "Repository Management";
      else if (permission.name.includes("Notification"))
        category = "Notifications";

      if (!groups[category]) {
        groups[category] = [];
      }
      groups[category].push(permission);
      return groups;
    },
    {} as Record<string, Permission[]>
  );

  return (
    <Dialog open={isOpen} onOpenChange={onClose}>
      <DialogContent className="max-w-4xl max-h-[80vh] overflow-hidden">
        <DialogHeader>
          <DialogTitle className="flex items-center gap-2">
            {roleIcons[role.name] || <Shield className="h-4 w-4" />}
            Manage Permissions for {role.name}
          </DialogTitle>
          <DialogDescription>
            Configure which permissions are assigned to the {role.name} role.
          </DialogDescription>
        </DialogHeader>

        <div className="flex flex-col gap-4 overflow-hidden">
          {/* Search */}
          <div className="relative">
            <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 h-4 w-4" />
            <Input
              placeholder="Search permissions..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              className="pl-10"
            />
          </div>

          {/* Permissions List */}
          <div className="flex-1 overflow-y-auto max-h-96">
            <div className="space-y-6">
              {Object.entries(groupedPermissions).map(
                ([category, permissions]) => (
                  <div key={category}>
                    <h3 className="text-sm font-semibold text-gray-900 mb-3">
                      {category}
                    </h3>
                    <div className="grid gap-3">
                      {permissions.map((permission) => {
                        const isAssigned = rolePermissionIds.includes(
                          permission.id
                        );
                        return (
                          <div
                            key={permission.id}
                            className="flex items-start space-x-3 p-3 border rounded-lg hover:bg-gray-50"
                          >
                            <Checkbox
                              id={`permission-${permission.id}`}
                              checked={isAssigned}
                              onCheckedChange={(checked) =>
                                handlePermissionToggle(
                                  permission,
                                  checked as boolean
                                )
                              }
                              disabled={
                                assignPermissionMutation.isPending ||
                                removePermissionMutation.isPending
                              }
                            />
                            <div className="flex-1 min-w-0">
                              <Label
                                htmlFor={`permission-${permission.id}`}
                                className="text-sm font-medium cursor-pointer"
                              >
                                {permission.name}
                              </Label>
                              <p className="text-xs text-gray-500 mt-1">
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
          </div>
        </div>

        <DialogFooter>
          <Button variant="outline" onClick={onClose}>
            <X className="h-4 w-4 mr-2" />
            Close
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
};

export const RolePermissionsPage: React.FC = () => {
  const [selectedRole, setSelectedRole] = useState<Role | null>(null);
  const [isPermissionsDialogOpen, setIsPermissionsDialogOpen] = useState(false);
  const queryClient = useQueryClient();

  // Fetch all roles
  const {
    data: rolesData,
    isLoading: rolesLoading,
    error: rolesError,
  } = useQuery({
    queryKey: ["roles"],
    queryFn: async () => {
      const response = await apiClient.roles.getAll();
      return response.data.value;
    },
  });

  // Fetch all permissions
  const { data: permissionsData, isLoading: permissionsLoading } = useQuery({
    queryKey: ["permissions"],
    queryFn: async () => {
      const response = await apiClient.roles.getPermissions();
      return response.data.value;
    },
  });

  // Fetch role with permissions when role is selected
  const { data: roleWithPermissions } = useQuery({
    queryKey: ["roleWithPermissions", selectedRole?.id],
    queryFn: async () => {
      if (!selectedRole) return null;
      const response = await apiClient.roles.getRoleWithPermissions(
        selectedRole.id
      );
      return response.data.value;
    },
    enabled: !!selectedRole,
  });

  const handleManagePermissions = (role: Role) => {
    setSelectedRole(role);
    setIsPermissionsDialogOpen(true);
  };

  const handlePermissionsChanged = () => {
    if (selectedRole) {
      queryClient.invalidateQueries({
        queryKey: ["roleWithPermissions", selectedRole.id],
      });
      queryClient.invalidateQueries({ queryKey: ["roles"] });
    }
  };

  if (rolesLoading || permissionsLoading) {
    return (
      <div className="container mx-auto p-6">
        <div className="space-y-6">
          <div className="flex items-center justify-between">
            <div>
              <h1 className="text-3xl font-bold">
                Role & Permission Management
              </h1>
              <p className="text-muted-foreground">
                Manage roles and their associated permissions
              </p>
            </div>
          </div>
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
            {[...Array(5)].map((_, i) => (
              <CardLoadingSkeleton key={i} />
            ))}
          </div>
        </div>
      </div>
    );
  }

  if (rolesError) {
    return (
      <div className="container mx-auto p-6">
        <Card>
          <CardContent className="p-6">
            <div className="text-center">
              <p className="text-red-600">
                Error loading roles: {rolesError.message}
              </p>
            </div>
          </CardContent>
        </Card>
      </div>
    );
  }

  const roles = rolesData || [];
  const allPermissions = permissionsData || [];

  return (
    <div className="container mx-auto p-6">
      <div className="space-y-6">
        {/* Header */}
        <div className="flex items-center justify-between">
          <div>
            <h1 className="text-3xl font-bold">Role & Permission Management</h1>
            <p className="text-muted-foreground">
              Manage roles and their associated permissions
            </p>
          </div>
        </div>

        {/* Roles Grid */}
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          {roles.map((role) => (
            <Card key={role.id} className="hover:shadow-md transition-shadow">
              <CardHeader>
                <CardTitle className="flex items-center justify-between">
                  <div className="flex items-center gap-2">
                    {roleIcons[role.name] || <Shield className="h-4 w-4" />}
                    {role.name}
                  </div>
                  <Badge
                    variant="secondary"
                    className={
                      roleColors[role.name] || "bg-gray-100 text-gray-800"
                    }
                  >
                    {role.permissions.length} permissions
                  </Badge>
                </CardTitle>
                <CardDescription>
                  Manage permissions for {role.name} role
                </CardDescription>
              </CardHeader>
              <CardContent>
                <div className="space-y-4">
                  {/* Show some key permissions */}
                  <div>
                    <Label className="text-sm font-medium">
                      Key Permissions:
                    </Label>
                    <div className="flex flex-wrap gap-1 mt-2">
                      {role.permissions.slice(0, 3).map((permission) => (
                        <Badge
                          key={permission}
                          variant="outline"
                          className="text-xs"
                        >
                          {permission.replace(/([A-Z])/g, " $1").trim()}
                        </Badge>
                      ))}
                      {role.permissions.length > 3 && (
                        <Badge variant="outline" className="text-xs">
                          +{role.permissions.length - 3} more
                        </Badge>
                      )}
                    </div>
                  </div>

                  <Button
                    onClick={() => handleManagePermissions(role)}
                    className="w-full"
                  >
                    <Settings className="h-4 w-4 mr-2" />
                    Manage Permissions
                  </Button>
                </div>
              </CardContent>
            </Card>
          ))}
        </div>

        {/* Total Stats */}
        <div className="grid grid-cols-1 md:grid-cols-3 gap-6 mt-8">
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <Users className="h-4 w-4" />
                Total Roles
              </CardTitle>
            </CardHeader>
            <CardContent>
              <div className="text-3xl font-bold">{roles.length}</div>
              <p className="text-muted-foreground">System roles</p>
            </CardContent>
          </Card>

          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <Shield className="h-4 w-4" />
                Total Permissions
              </CardTitle>
            </CardHeader>
            <CardContent>
              <div className="text-3xl font-bold">{allPermissions.length}</div>
              <p className="text-muted-foreground">Available permissions</p>
            </CardContent>
          </Card>

          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <Settings className="h-4 w-4" />
                Most Privileged Role
              </CardTitle>
            </CardHeader>
            <CardContent>
              <div className="text-xl font-bold">
                {roles.reduce((prev, current) =>
                  prev.permissions.length > current.permissions.length
                    ? prev
                    : current
                )?.name || "N/A"}
              </div>
              <p className="text-muted-foreground">
                {Math.max(...roles.map((r) => r.permissions.length))}{" "}
                permissions
              </p>
            </CardContent>
          </Card>
        </div>
      </div>

      {/* Manage Permissions Dialog */}
      {selectedRole && roleWithPermissions && (
        <ManagePermissionsDialog
          role={roleWithPermissions}
          allPermissions={allPermissions}
          isOpen={isPermissionsDialogOpen}
          onClose={() => {
            setIsPermissionsDialogOpen(false);
            setSelectedRole(null);
          }}
          onPermissionsChanged={handlePermissionsChanged}
        />
      )}
    </div>
  );
};
