import React from "react";
import { useAuth } from "@/hooks/useAuth";
import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import { User, LogOut } from "lucide-react";

export const ImpersonationBanner: React.FC = () => {
  const { user, stopImpersonate } = useAuth();

  if (!user?.isImpersonating) {
    return null;
  }

  const handleStopImpersonate = async () => {
    try {
      await stopImpersonate();
    } catch (error) {
      console.error("Failed to stop impersonation:", error);
      // You might want to show a toast notification here
    }
  };

  return (
    <Card className="mb-4 bg-orange-50 border-orange-200">
      <CardContent className="py-3">
        <div className="flex items-center justify-between">
          <div className="flex items-center">
            <User className="h-4 w-4 text-orange-600 mr-2" />
            <span className="text-orange-800">
              You are impersonating <strong>{user.email}</strong>
              {user.originalUserEmail && (
                <span className="text-sm text-orange-600 ml-2">
                  (originally logged in as {user.originalUserEmail})
                </span>
              )}
            </span>
          </div>
          <Button
            variant="outline"
            size="sm"
            onClick={handleStopImpersonate}
            className="ml-4 border-orange-300 text-orange-700 hover:bg-orange-100"
          >
            <LogOut className="h-4 w-4 mr-1" />
            Stop Impersonating
          </Button>
        </div>
      </CardContent>
    </Card>
  );
};
