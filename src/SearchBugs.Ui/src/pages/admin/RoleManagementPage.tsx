import React from "react";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { RoleManagement } from "./components/RoleManagement";
import { UserRoleManagement } from "./components/UserRoleManagement";
import { PermissionMatrix } from "./components/PermissionMatrix";

export const RoleManagementPage: React.FC = () => {
  return (
    <div className="container mx-auto py-6 space-y-6">
      <div>
        <h1 className="text-3xl font-bold tracking-tight">
          Role & Permission Management
        </h1>
        <p className="text-muted-foreground">
          Manage system roles, permissions, and user role assignments.
        </p>
      </div>

      <Tabs defaultValue="roles" className="space-y-4">
        <TabsList className="grid w-full grid-cols-3">
          <TabsTrigger value="roles">Roles & Permissions</TabsTrigger>
          <TabsTrigger value="users">User Roles</TabsTrigger>
          <TabsTrigger value="matrix">Permission Matrix</TabsTrigger>
        </TabsList>

        <TabsContent value="roles" className="space-y-4">
          <Card>
            <CardHeader>
              <CardTitle>Role & Permission Management</CardTitle>
              <CardDescription>
                Assign or remove permissions for each system role.
              </CardDescription>
            </CardHeader>
            <CardContent>
              <RoleManagement />
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="users" className="space-y-4">
          <Card>
            <CardHeader>
              <CardTitle>User Role Assignment</CardTitle>
              <CardDescription>
                Manage role assignments for individual users.
              </CardDescription>
            </CardHeader>
            <CardContent>
              <UserRoleManagement />
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="matrix" className="space-y-4">
          <Card>
            <CardHeader>
              <CardTitle>Permission Matrix</CardTitle>
              <CardDescription>
                View all permissions across all roles in a matrix format.
              </CardDescription>
            </CardHeader>
            <CardContent>
              <PermissionMatrix />
            </CardContent>
          </Card>
        </TabsContent>
      </Tabs>
    </div>
  );
};
