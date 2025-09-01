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
import { FormInput } from "@/components/ui/form";
import { Card, CardContent } from "@/components/ui/card";
import { User, Search } from "lucide-react";
import { useQuery } from "@tanstack/react-query";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { searchSchema, type SearchFormData } from "@/lib/validations";

export const ImpersonationDialog: React.FC = () => {
  const { user, impersonate } = useAuth();
  const [isOpen, setIsOpen] = useState(false);
  const [isImpersonating, setIsImpersonating] = useState(false);

  const {
    control,
    watch,
    formState: { errors },
  } = useForm<SearchFormData>({
    resolver: zodResolver(
      searchSchema.omit({ query: true }).extend({
        query: searchSchema.shape.query.optional().default(""),
      })
    ),
    defaultValues: {
      query: "",
    },
  });

  const searchTerm = watch("query") || "";

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
          <div className="space-y-2">
            <div className="relative">
              <Search className="absolute left-3 top-2.5 h-4 w-4 text-muted-foreground" />
              <FormInput
                control={control}
                name="query"
                placeholder="Search by name or email..."
                className="pl-10"
                error={errors.query}
              />
            </div>
          </div>

          <div className="space-y-2 max-h-[400px] overflow-y-auto">
            {isLoading ? (
              <div className="text-center py-4 text-muted-foreground">
                Loading users...
              </div>
            ) : filteredUsers.length === 0 ? (
              <div className="text-center py-4 text-muted-foreground">
                {searchTerm
                  ? "No users found matching your search"
                  : "No users available"}
              </div>
            ) : (
              filteredUsers.map((userOption) => (
                <Card
                  key={userOption.id}
                  className="cursor-pointer hover:bg-muted/50"
                >
                  <CardContent className="p-3">
                    <div className="flex items-center justify-between">
                      <div>
                        <div className="font-medium">
                          {userOption.firstName} {userOption.lastName}
                        </div>
                        <div className="text-sm text-muted-foreground">
                          {userOption.email}
                        </div>
                        {userOption.roles && userOption.roles.length > 0 && (
                          <div className="text-xs text-muted-foreground mt-1">
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
