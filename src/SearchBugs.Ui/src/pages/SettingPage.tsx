import { useState } from "react";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { TimezoneSettings } from "@/components/ui/timezone-settings";
import { ThemeSettings } from "@/components/settings/ThemeSettings";
import {
  SecuritySettingsPanel,
  NotificationSettingsPanel,
  IntegrationSettingsPanel,
  AdvancedSettingsPanel,
} from "@/components/settings/SettingsPanels";
import {
  Settings,
  User,
  Shield,
  Bell,
  Puzzle,
  HelpCircle,
  Building,
  Cog,
} from "lucide-react";

export const SettingPage = () => {
  const [activeTab, setActiveTab] = useState("general");

  const settingsNavigation = [
    { id: "general", label: "General", icon: User },
    { id: "security", label: "Security", icon: Shield },
    { id: "notifications", label: "Notifications", icon: Bell },
    { id: "integrations", label: "Integrations", icon: Puzzle },
    { id: "support", label: "Support", icon: HelpCircle },
    { id: "organizations", label: "Organizations", icon: Building },
    { id: "advanced", label: "Advanced", icon: Cog },
  ];

  return (
    <div className="flex flex-col gap-3 md:gap-8">
      {/* Header */}
      <div className="mx-auto grid w-full max-w-6xl gap-2">
        <div className="flex items-center gap-3">
          <Settings className="h-8 w-8" />
          <h1 className="text-3xl font-semibold">Settings</h1>
        </div>
        <p className="text-muted-foreground">
          Manage your account settings, security preferences, and integrations.
        </p>
      </div>

      {/* Main Content */}
      <div className="mx-auto grid w-full max-w-6xl items-start gap-6 md:grid-cols-[220px_1fr] lg:grid-cols-[280px_1fr]">
        {/* Navigation Sidebar */}
        <nav className="grid gap-2 text-sm">
          {settingsNavigation.map((item) => {
            const IconComponent = item.icon;
            const isActive = activeTab === item.id;

            return (
              <Button
                key={item.id}
                variant={isActive ? "secondary" : "ghost"}
                className={`justify-start gap-3 ${
                  isActive ? "bg-muted font-medium" : ""
                }`}
                onClick={() => setActiveTab(item.id)}
              >
                <IconComponent className="h-4 w-4" />
                {item.label}
              </Button>
            );
          })}
        </nav>

        {/* Settings Content */}
        <div className="grid gap-6">
          {/* General Settings */}
          {activeTab === "general" && (
            <div className="space-y-6">
              <Card>
                <CardHeader>
                  <CardTitle className="flex items-center gap-2">
                    <User className="h-5 w-5" />
                    General Settings
                  </CardTitle>
                </CardHeader>
                <CardContent className="space-y-6">
                  <div>
                    <h4 className="text-sm font-medium mb-3">
                      Account Information
                    </h4>
                    <div className="grid gap-4">
                      <div className="flex items-center justify-between p-3 border rounded-lg">
                        <div>
                          <p className="font-medium">Profile</p>
                          <p className="text-sm text-muted-foreground">
                            Update your personal information
                          </p>
                        </div>
                        <Button variant="outline" size="sm">
                          Edit Profile
                        </Button>
                      </div>
                    </div>
                  </div>

                  {/* Timezone Settings */}
                  <TimezoneSettings />

                  {/* Theme Settings */}
                  <ThemeSettings />

                  {/* Language Settings */}
                  <div>
                    <h4 className="text-sm font-medium mb-3">
                      Language & Region
                    </h4>
                    <div className="grid gap-4">
                      <div className="flex items-center justify-between p-3 border rounded-lg">
                        <div>
                          <p className="font-medium">Language</p>
                          <p className="text-sm text-muted-foreground">
                            English (US)
                          </p>
                        </div>
                        <Button variant="outline" size="sm">
                          Change
                        </Button>
                      </div>
                    </div>
                  </div>
                </CardContent>
              </Card>
            </div>
          )}

          {/* Security Settings */}
          {activeTab === "security" && <SecuritySettingsPanel />}

          {/* Notification Settings */}
          {activeTab === "notifications" && <NotificationSettingsPanel />}

          {/* Integration Settings */}
          {activeTab === "integrations" && <IntegrationSettingsPanel />}

          {/* Support Settings */}
          {activeTab === "support" && (
            <Card>
              <CardHeader>
                <CardTitle className="flex items-center gap-2">
                  <HelpCircle className="h-5 w-5" />
                  Support & Help
                </CardTitle>
              </CardHeader>
              <CardContent className="space-y-4">
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                  <div className="p-4 border rounded-lg hover:bg-muted/50 cursor-pointer transition-colors">
                    <h4 className="font-medium">Help Center</h4>
                    <p className="text-sm text-muted-foreground mt-1">
                      Browse our comprehensive guides and documentation
                    </p>
                    <div className="mt-3">
                      <Badge variant="outline">Available 24/7</Badge>
                    </div>
                  </div>
                  <div className="p-4 border rounded-lg hover:bg-muted/50 cursor-pointer transition-colors">
                    <h4 className="font-medium">Contact Support</h4>
                    <p className="text-sm text-muted-foreground mt-1">
                      Get help from our support team
                    </p>
                    <div className="mt-3">
                      <Badge variant="outline">Response in 24h</Badge>
                    </div>
                  </div>
                  <div className="p-4 border rounded-lg hover:bg-muted/50 cursor-pointer transition-colors">
                    <h4 className="font-medium">Bug Reports</h4>
                    <p className="text-sm text-muted-foreground mt-1">
                      Report issues or request new features
                    </p>
                    <div className="mt-3">
                      <Badge variant="outline">Community Driven</Badge>
                    </div>
                  </div>
                  <div className="p-4 border rounded-lg hover:bg-muted/50 cursor-pointer transition-colors">
                    <h4 className="font-medium">System Status</h4>
                    <p className="text-sm text-muted-foreground mt-1">
                      Check current service availability
                    </p>
                    <div className="mt-3">
                      <Badge variant="secondary">All systems operational</Badge>
                    </div>
                  </div>
                </div>
              </CardContent>
            </Card>
          )}

          {/* Organizations Settings */}
          {activeTab === "organizations" && (
            <Card>
              <CardHeader>
                <div className="flex items-center justify-between">
                  <CardTitle className="flex items-center gap-2">
                    <Building className="h-5 w-5" />
                    Organizations
                  </CardTitle>
                  <Button variant="outline" size="sm">
                    <Building className="h-4 w-4 mr-2" />
                    Create Organization
                  </Button>
                </div>
              </CardHeader>
              <CardContent className="space-y-4">
                <div className="space-y-3">
                  <div className="flex items-center justify-between p-4 border rounded-lg">
                    <div className="flex items-center gap-3">
                      <div className="w-10 h-10 bg-primary/20 rounded-full flex items-center justify-center">
                        <Building className="h-5 w-5" />
                      </div>
                      <div>
                        <h4 className="font-medium">SearchBugs Team</h4>
                        <p className="text-sm text-muted-foreground">
                          Owner • 8 members • 12 projects
                        </p>
                      </div>
                    </div>
                    <div className="flex items-center gap-2">
                      <Badge variant="secondary">Active</Badge>
                      <Button variant="outline" size="sm">
                        Manage
                      </Button>
                    </div>
                  </div>

                  <div className="flex items-center justify-between p-4 border rounded-lg">
                    <div className="flex items-center gap-3">
                      <div className="w-10 h-10 bg-secondary/20 rounded-full flex items-center justify-center">
                        <Building className="h-5 w-5" />
                      </div>
                      <div>
                        <h4 className="font-medium">
                          Open Source Contributors
                        </h4>
                        <p className="text-sm text-muted-foreground">
                          Member • 156 members • 45 projects
                        </p>
                      </div>
                    </div>
                    <div className="flex items-center gap-2">
                      <Badge variant="outline">Member</Badge>
                      <Button variant="outline" size="sm">
                        View
                      </Button>
                    </div>
                  </div>
                </div>
              </CardContent>
            </Card>
          )}

          {/* Advanced Settings */}
          {activeTab === "advanced" && <AdvancedSettingsPanel />}
        </div>
      </div>
    </div>
  );
};
