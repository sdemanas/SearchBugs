import React, { useState } from "react";
import { useAuth } from "@/hooks/useAuth";
import { apiClient } from "@/lib/api";
import { Button } from "@/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Card, CardContent } from "@/components/ui/card";
import { User, Search } from "lucide-react";
import { useQuery } from "@tanstack/react-query";

export const ImpersonationDialog: React.FC = () => {
  const { user, impersonate } = useAuth();
  const [isOpen, setIsOpen] = useState(false);
  const [searchTerm, setSearchTerm] = useState("");
  const [isImpersonating, setIsImpersonating] = useState(false);

  const { data: usersResponse, isLoading } = useQuery({
    queryKey: ["users"],
    queryFn: () => apiClient.users.getAll(),
  });

  const users = usersResponse?.data?.value || [];

  // Filter users based on search term
  const filteredUsers = users.filter(
    (u) =>
      u.email.toLowerCase().includes(searchTerm.toLowerCase()) ||
      `${u.firstName} ${u.lastName}`
        .toLowerCase()
        .includes(searchTerm.toLowerCase())
  );

  const handleImpersonate = async (userId: string) => {
    try {
      setIsImpersonating(true);
      await impersonate(userId);
      setIsOpen(false);
      setSearchTerm("");
    } catch (error) {
      console.error("Failed to impersonate user:", error);
      // You might want to show a toast notification here
    } finally {
      setIsImpersonating(false);
    }
  };

  // Don't show the dialog if already impersonating
  if (user?.isImpersonating) {
    return null;
  }

  return (
    <Dialog open={isOpen} onOpenChange={setIsOpen}>
      <DialogTrigger asChild>
        <Button variant="outline" size="sm">
          <User className="h-4 w-4 mr-1" />
          Impersonate User
        </Button>
      </DialogTrigger>
      <DialogContent className="sm:max-w-[500px] max-h-[600px]">
        <DialogHeader>
          <DialogTitle>Impersonate User</DialogTitle>
        </DialogHeader>
        <div className="space-y-4">
          <div>
            <Label htmlFor="search">Search Users</Label>
            <div className="relative">
              <Search className="absolute left-3 top-2.5 h-4 w-4 text-gray-400" />
              <Input
                id="search"
                placeholder="Search by name or email..."
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                className="pl-10"
              />
            </div>
          </div>

          <div className="space-y-2 max-h-[400px] overflow-y-auto">
            {isLoading ? (
              <div className="text-center py-4 text-gray-500">
                Loading users...
              </div>
            ) : filteredUsers.length === 0 ? (
              <div className="text-center py-4 text-gray-500">
                {searchTerm
                  ? "No users found matching your search"
                  : "No users available"}
              </div>
            ) : (
              filteredUsers.map((userOption) => (
                <Card
                  key={userOption.id}
                  className="cursor-pointer hover:bg-gray-50"
                >
                  <CardContent className="p-3">
                    <div className="flex items-center justify-between">
                      <div>
                        <div className="font-medium">
                          {userOption.firstName} {userOption.lastName}
                        </div>
                        <div className="text-sm text-gray-500">
                          {userOption.email}
                        </div>
                        {userOption.roles && userOption.roles.length > 0 && (
                          <div className="text-xs text-gray-400 mt-1">
                            Roles: {userOption.roles.join(", ")}
                          </div>
                        )}
                      </div>
                      <Button
                        size="sm"
                        onClick={() => handleImpersonate(userOption.id)}
                        disabled={isImpersonating || userOption.id === user?.id}
                      >
                        {isImpersonating ? "..." : "Impersonate"}
                      </Button>
                    </div>
                  </CardContent>
                </Card>
              ))
            )}
          </div>
        </div>
      </DialogContent>
    </Dialog>
  );
};
