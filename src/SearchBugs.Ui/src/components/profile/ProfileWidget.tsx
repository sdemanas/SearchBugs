import React, { useState } from "react";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { RecentActivitySmall } from "@/components/profile/RecentActivitySmall";
import {
  User,
  Settings,
  Activity,
  MapPin,
  Calendar,
  Mail,
  Briefcase,
  BookOpen,
  Star,
  GitFork,
  Shield,
  Puzzle,
  HelpCircle,
  Building,
  Cog,
  Lock,
  Eye,
  Smartphone,
  Bell,
  Users,
  Globe,
  Key,
  Database,
  Trash2,
} from "lucide-react";
import { type UserProfile } from "@/lib/api";

interface ProfileWidgetProps {
  profile: UserProfile;
}

const ProfileWidget: React.FC<ProfileWidgetProps> = ({ profile }) => {
  const [activeTab, setActiveTab] = useState("overview");

  return (
    <div className="container mx-auto p-6 max-w-4xl space-y-6">
      {/* Profile Header with Recent Activity */}
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* Main Profile Card */}
        <Card className="lg:col-span-2">
          <CardHeader className="pb-4">
            <div className="flex items-start gap-4">
              <Avatar className="h-16 w-16 border-4 border-background shadow-lg">
                <AvatarImage
                  src={profile.avatarUrl}
                  alt={`${profile.firstName} ${profile.lastName}`}
                />
                <AvatarFallback className="text-xl font-bold bg-primary text-primary-foreground">
                  {profile.firstName[0]}
                  {profile.lastName[0]}
                </AvatarFallback>
              </Avatar>

              <div className="flex-1">
                <h1 className="text-xl font-bold">
                  {profile.firstName} {profile.lastName}
                </h1>
                <p className="text-muted-foreground">
                  @{profile.email.split("@")[0]}
                </p>

                {profile.roles && profile.roles.length > 0 && (
                  <div className="flex gap-1 mt-2">
                    {profile.roles.map((role) => (
                      <Badge
                        key={role.id}
                        variant="secondary"
                        className="text-xs"
                      >
                        {role.name}
                      </Badge>
                    ))}
                  </div>
                )}
              </div>

              <Button variant="outline" size="sm">
                <Settings className="h-4 w-4 mr-2" />
                Edit
              </Button>
            </div>
          </CardHeader>

          <CardContent className="space-y-4">
            {profile.bio && (
              <p className="text-sm text-muted-foreground">{profile.bio}</p>
            )}

            <div className="grid grid-cols-2 gap-4 text-sm">
              {profile.location && (
                <div className="flex items-center gap-2">
                  <MapPin className="h-4 w-4 text-muted-foreground" />
                  <span>{profile.location}</span>
                </div>
              )}

              {profile.company && (
                <div className="flex items-center gap-2">
                  <Briefcase className="h-4 w-4 text-muted-foreground" />
                  <span>{profile.company}</span>
                </div>
              )}

              <div className="flex items-center gap-2">
                <Mail className="h-4 w-4 text-muted-foreground" />
                <span>{profile.email}</span>
              </div>

              {profile.createdOnUtc && (
                <div className="flex items-center gap-2">
                  <Calendar className="h-4 w-4 text-muted-foreground" />
                  <span>
                    Joined{" "}
                    {new Date(profile.createdOnUtc).toLocaleDateString(
                      "en-US",
                      {
                        month: "long",
                        year: "numeric",
                      }
                    )}
                  </span>
                </div>
              )}
            </div>
          </CardContent>
        </Card>

        {/* Recent Activity Sidebar */}
        <RecentActivitySmall
          maxItems={5}
          showViewAll={true}
          onViewAll={() => setActiveTab("activity")}
        />
      </div>

      {/* Profile Content Tabs */}
      <Tabs
        value={activeTab}
        onValueChange={setActiveTab}
        className="space-y-6"
      >
        <TabsList className="grid w-full grid-cols-5">
          <TabsTrigger value="overview">
            <User className="h-4 w-4 mr-2" />
            Overview
          </TabsTrigger>
          <TabsTrigger value="activity">
            <Activity className="h-4 w-4 mr-2" />
            Activity
          </TabsTrigger>
          <TabsTrigger value="projects">
            <BookOpen className="h-4 w-4 mr-2" />
            Projects
          </TabsTrigger>
          <TabsTrigger value="stats">
            <Star className="h-4 w-4 mr-2" />
            Stats
          </TabsTrigger>
          <TabsTrigger value="settings">
            <Settings className="h-4 w-4 mr-2" />
            Settings
          </TabsTrigger>
        </TabsList>

        <TabsContent value="overview" className="space-y-6">
          <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
            {/* Quick Stats */}
            <Card>
              <CardHeader>
                <CardTitle className="text-lg">Quick Stats</CardTitle>
              </CardHeader>
              <CardContent>
                <div className="grid grid-cols-2 gap-4">
                  <div className="text-center p-4 border rounded-lg">
                    <div className="text-2xl font-bold text-blue-600">12</div>
                    <div className="text-sm text-muted-foreground">
                      Projects
                    </div>
                  </div>
                  <div className="text-center p-4 border rounded-lg">
                    <div className="text-2xl font-bold text-green-600">45</div>
                    <div className="text-sm text-muted-foreground">
                      Contributions
                    </div>
                  </div>
                  <div className="text-center p-4 border rounded-lg">
                    <div className="text-2xl font-bold text-purple-600">8</div>
                    <div className="text-sm text-muted-foreground">
                      Following
                    </div>
                  </div>
                  <div className="text-center p-4 border rounded-lg">
                    <div className="text-2xl font-bold text-orange-600">23</div>
                    <div className="text-sm text-muted-foreground">
                      Followers
                    </div>
                  </div>
                </div>
              </CardContent>
            </Card>

            {/* Compact Recent Activity for Overview */}
            <RecentActivitySmall
              maxItems={4}
              showViewAll={true}
              onViewAll={() => setActiveTab("activity")}
            />
          </div>

          {/* Recent Projects */}
          <Card>
            <CardHeader>
              <CardTitle className="text-lg">Recent Projects</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="space-y-4">
                {[
                  {
                    name: "SearchBugs Web App",
                    status: "Active",
                    stars: 15,
                    forks: 3,
                  },
                  {
                    name: "Bug Tracker API",
                    status: "Completed",
                    stars: 8,
                    forks: 2,
                  },
                  {
                    name: "Dashboard UI Kit",
                    status: "In Progress",
                    stars: 25,
                    forks: 7,
                  },
                ].map((project, index) => (
                  <div
                    key={index}
                    className="flex items-center justify-between p-3 border rounded-lg hover:bg-muted/50"
                  >
                    <div className="flex-1">
                      <h4 className="font-medium">{project.name}</h4>
                      <div className="flex items-center gap-4 mt-1 text-sm text-muted-foreground">
                        <span className="flex items-center gap-1">
                          <Star className="h-3 w-3" />
                          {project.stars}
                        </span>
                        <span className="flex items-center gap-1">
                          <GitFork className="h-3 w-3" />
                          {project.forks}
                        </span>
                        <Badge
                          variant={
                            project.status === "Active"
                              ? "default"
                              : "secondary"
                          }
                        >
                          {project.status}
                        </Badge>
                      </div>
                    </div>
                  </div>
                ))}
              </div>
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="activity">
          <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
            {/* Full Recent Activity */}
            <RecentActivitySmall maxItems={10} showViewAll={false} />

            {/* Activity Summary */}
            <Card>
              <CardHeader>
                <CardTitle className="text-lg">Activity Summary</CardTitle>
              </CardHeader>
              <CardContent>
                <div className="space-y-4">
                  <div className="flex items-center justify-between">
                    <span>Today</span>
                    <span className="font-medium">3 activities</span>
                  </div>
                  <div className="flex items-center justify-between">
                    <span>This week</span>
                    <span className="font-medium">12 activities</span>
                  </div>
                  <div className="flex items-center justify-between">
                    <span>This month</span>
                    <span className="font-medium">45 activities</span>
                  </div>
                  <div className="flex items-center justify-between">
                    <span>Most active day</span>
                    <span className="font-medium">Tuesday</span>
                  </div>
                </div>
              </CardContent>
            </Card>
          </div>
        </TabsContent>

        <TabsContent value="projects">
          <Card>
            <CardHeader>
              <CardTitle className="text-lg">All Projects</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="text-center py-8 text-muted-foreground">
                <BookOpen className="h-12 w-12 mx-auto mb-4 opacity-50" />
                <p>No projects found</p>
                <Button variant="outline" size="sm" className="mt-4">
                  Create your first project
                </Button>
              </div>
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="stats">
          <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
            <Card>
              <CardHeader>
                <CardTitle className="text-lg">Contribution Stats</CardTitle>
              </CardHeader>
              <CardContent>
                <div className="space-y-4">
                  <div className="flex items-center justify-between">
                    <span>Total Contributions</span>
                    <span className="font-medium">156</span>
                  </div>
                  <div className="flex items-center justify-between">
                    <span>Issues Created</span>
                    <span className="font-medium">23</span>
                  </div>
                  <div className="flex items-center justify-between">
                    <span>Issues Resolved</span>
                    <span className="font-medium">45</span>
                  </div>
                  <div className="flex items-center justify-between">
                    <span>Code Reviews</span>
                    <span className="font-medium">12</span>
                  </div>
                </div>
              </CardContent>
            </Card>

            {/* Recent Activity in Stats Tab */}
            <RecentActivitySmall
              maxItems={6}
              showViewAll={true}
              onViewAll={() => setActiveTab("activity")}
            />
          </div>
        </TabsContent>

        <TabsContent value="settings">
          <div className="grid grid-cols-1 lg:grid-cols-4 gap-6">
            {/* Settings Navigation */}
            <Card className="lg:col-span-1">
              <CardHeader className="pb-3">
                <CardTitle className="text-base">Settings</CardTitle>
              </CardHeader>
              <CardContent>
                <nav className="space-y-2">
                  <button className="w-full text-left p-3 rounded-lg hover:bg-muted transition-colors flex items-center gap-3 bg-primary/10 text-primary">
                    <Shield className="h-4 w-4" />
                    <span className="font-medium">Security</span>
                  </button>
                  <button className="w-full text-left p-3 rounded-lg hover:bg-muted transition-colors flex items-center gap-3">
                    <Puzzle className="h-4 w-4" />
                    <span>Integrations</span>
                  </button>
                  <button className="w-full text-left p-3 rounded-lg hover:bg-muted transition-colors flex items-center gap-3">
                    <HelpCircle className="h-4 w-4" />
                    <span>Support</span>
                  </button>
                  <button className="w-full text-left p-3 rounded-lg hover:bg-muted transition-colors flex items-center gap-3">
                    <Building className="h-4 w-4" />
                    <span>Organizations</span>
                  </button>
                  <button className="w-full text-left p-3 rounded-lg hover:bg-muted transition-colors flex items-center gap-3">
                    <Cog className="h-4 w-4" />
                    <span>Advanced</span>
                  </button>
                </nav>
              </CardContent>
            </Card>

            {/* Settings Content */}
            <div className="lg:col-span-3 space-y-6">
              {/* Security Section */}
              <Card>
                <CardHeader>
                  <CardTitle className="text-lg flex items-center gap-2">
                    <Shield className="h-5 w-5" />
                    Security
                  </CardTitle>
                </CardHeader>
                <CardContent className="space-y-6">
                  {/* Password */}
                  <div className="flex items-center justify-between p-4 border rounded-lg">
                    <div className="flex items-center gap-3">
                      <Lock className="h-5 w-5 text-muted-foreground" />
                      <div>
                        <h4 className="font-medium">Password</h4>
                        <p className="text-sm text-muted-foreground">
                          Last changed 3 months ago
                        </p>
                      </div>
                    </div>
                    <Button variant="outline" size="sm">
                      Change Password
                    </Button>
                  </div>

                  {/* Two-Factor Authentication */}
                  <div className="flex items-center justify-between p-4 border rounded-lg">
                    <div className="flex items-center gap-3">
                      <Smartphone className="h-5 w-5 text-muted-foreground" />
                      <div>
                        <h4 className="font-medium">
                          Two-Factor Authentication
                        </h4>
                        <p className="text-sm text-muted-foreground">
                          Add an extra layer of security
                        </p>
                      </div>
                    </div>
                    <Button variant="outline" size="sm">
                      Enable 2FA
                    </Button>
                  </div>

                  {/* Privacy Settings */}
                  <div className="flex items-center justify-between p-4 border rounded-lg">
                    <div className="flex items-center gap-3">
                      <Eye className="h-5 w-5 text-muted-foreground" />
                      <div>
                        <h4 className="font-medium">Profile Privacy</h4>
                        <p className="text-sm text-muted-foreground">
                          Control who can see your profile
                        </p>
                      </div>
                    </div>
                    <Button variant="outline" size="sm">
                      Manage
                    </Button>
                  </div>

                  {/* API Keys */}
                  <div className="flex items-center justify-between p-4 border rounded-lg">
                    <div className="flex items-center gap-3">
                      <Key className="h-5 w-5 text-muted-foreground" />
                      <div>
                        <h4 className="font-medium">API Keys</h4>
                        <p className="text-sm text-muted-foreground">
                          Manage your API access tokens
                        </p>
                      </div>
                    </div>
                    <Button variant="outline" size="sm">
                      View Keys
                    </Button>
                  </div>
                </CardContent>
              </Card>

              {/* Integrations Section */}
              <Card>
                <CardHeader>
                  <CardTitle className="text-lg flex items-center gap-2">
                    <Puzzle className="h-5 w-5" />
                    Integrations
                  </CardTitle>
                </CardHeader>
                <CardContent className="space-y-4">
                  <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                    <div className="flex items-center justify-between p-4 border rounded-lg">
                      <div>
                        <h4 className="font-medium">GitHub</h4>
                        <p className="text-sm text-muted-foreground">
                          Connected
                        </p>
                      </div>
                      <Badge variant="secondary">Active</Badge>
                    </div>
                    <div className="flex items-center justify-between p-4 border rounded-lg">
                      <div>
                        <h4 className="font-medium">Slack</h4>
                        <p className="text-sm text-muted-foreground">
                          Not connected
                        </p>
                      </div>
                      <Button variant="outline" size="sm">
                        Connect
                      </Button>
                    </div>
                    <div className="flex items-center justify-between p-4 border rounded-lg">
                      <div>
                        <h4 className="font-medium">Discord</h4>
                        <p className="text-sm text-muted-foreground">
                          Not connected
                        </p>
                      </div>
                      <Button variant="outline" size="sm">
                        Connect
                      </Button>
                    </div>
                    <div className="flex items-center justify-between p-4 border rounded-lg">
                      <div>
                        <h4 className="font-medium">Jira</h4>
                        <p className="text-sm text-muted-foreground">
                          Connected
                        </p>
                      </div>
                      <Badge variant="secondary">Active</Badge>
                    </div>
                  </div>
                </CardContent>
              </Card>

              {/* Organizations Section */}
              <Card>
                <CardHeader>
                  <div className="flex items-center justify-between">
                    <CardTitle className="text-lg flex items-center gap-2">
                      <Building className="h-5 w-5" />
                      Organizations
                    </CardTitle>
                    <Button variant="outline" size="sm">
                      <Users className="h-4 w-4 mr-2" />
                      Create Organization
                    </Button>
                  </div>
                </CardHeader>
                <CardContent className="space-y-4">
                  <div className="space-y-3">
                    <div className="flex items-center justify-between p-4 border rounded-lg">
                      <div className="flex items-center gap-3">
                        <div className="w-8 h-8 bg-primary/20 rounded-full flex items-center justify-center">
                          <Building className="h-4 w-4" />
                        </div>
                        <div>
                          <h4 className="font-medium">TechCorp</h4>
                          <p className="text-sm text-muted-foreground">
                            Owner • 12 members
                          </p>
                        </div>
                      </div>
                      <Button variant="outline" size="sm">
                        Manage
                      </Button>
                    </div>
                    <div className="flex items-center justify-between p-4 border rounded-lg">
                      <div className="flex items-center gap-3">
                        <div className="w-8 h-8 bg-secondary/20 rounded-full flex items-center justify-center">
                          <Building className="h-4 w-4" />
                        </div>
                        <div>
                          <h4 className="font-medium">
                            OpenSource Contributors
                          </h4>
                          <p className="text-sm text-muted-foreground">
                            Member • 45 members
                          </p>
                        </div>
                      </div>
                      <Button variant="outline" size="sm">
                        View
                      </Button>
                    </div>
                  </div>
                </CardContent>
              </Card>

              {/* Support Section */}
              <Card>
                <CardHeader>
                  <CardTitle className="text-lg flex items-center gap-2">
                    <HelpCircle className="h-5 w-5" />
                    Support
                  </CardTitle>
                </CardHeader>
                <CardContent className="space-y-4">
                  <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                    <div className="p-4 border rounded-lg hover:bg-muted/50 cursor-pointer">
                      <h4 className="font-medium">Help Center</h4>
                      <p className="text-sm text-muted-foreground mt-1">
                        Browse our comprehensive guides
                      </p>
                    </div>
                    <div className="p-4 border rounded-lg hover:bg-muted/50 cursor-pointer">
                      <h4 className="font-medium">Contact Support</h4>
                      <p className="text-sm text-muted-foreground mt-1">
                        Get help from our support team
                      </p>
                    </div>
                    <div className="p-4 border rounded-lg hover:bg-muted/50 cursor-pointer">
                      <h4 className="font-medium">Bug Reports</h4>
                      <p className="text-sm text-muted-foreground mt-1">
                        Report issues or request features
                      </p>
                    </div>
                    <div className="p-4 border rounded-lg hover:bg-muted/50 cursor-pointer">
                      <h4 className="font-medium">System Status</h4>
                      <p className="text-sm text-muted-foreground mt-1">
                        Check service availability
                      </p>
                    </div>
                  </div>
                </CardContent>
              </Card>

              {/* Advanced Section */}
              <Card>
                <CardHeader>
                  <CardTitle className="text-lg flex items-center gap-2">
                    <Cog className="h-5 w-5" />
                    Advanced
                  </CardTitle>
                </CardHeader>
                <CardContent className="space-y-6">
                  {/* Notifications */}
                  <div className="flex items-center justify-between p-4 border rounded-lg">
                    <div className="flex items-center gap-3">
                      <Bell className="h-5 w-5 text-muted-foreground" />
                      <div>
                        <h4 className="font-medium">
                          Notification Preferences
                        </h4>
                        <p className="text-sm text-muted-foreground">
                          Customize how you receive notifications
                        </p>
                      </div>
                    </div>
                    <Button variant="outline" size="sm">
                      Configure
                    </Button>
                  </div>

                  {/* Data Export */}
                  <div className="flex items-center justify-between p-4 border rounded-lg">
                    <div className="flex items-center gap-3">
                      <Database className="h-5 w-5 text-muted-foreground" />
                      <div>
                        <h4 className="font-medium">Export Data</h4>
                        <p className="text-sm text-muted-foreground">
                          Download your account data
                        </p>
                      </div>
                    </div>
                    <Button variant="outline" size="sm">
                      Export
                    </Button>
                  </div>

                  {/* Developer Settings */}
                  <div className="flex items-center justify-between p-4 border rounded-lg">
                    <div className="flex items-center gap-3">
                      <Globe className="h-5 w-5 text-muted-foreground" />
                      <div>
                        <h4 className="font-medium">Developer Settings</h4>
                        <p className="text-sm text-muted-foreground">
                          Webhook URLs and API configurations
                        </p>
                      </div>
                    </div>
                    <Button variant="outline" size="sm">
                      Manage
                    </Button>
                  </div>

                  {/* Danger Zone */}
                  <div className="border border-red-200 rounded-lg p-4 bg-red-50/50">
                    <div className="flex items-center justify-between">
                      <div className="flex items-center gap-3">
                        <Trash2 className="h-5 w-5 text-red-500" />
                        <div>
                          <h4 className="font-medium text-red-700">
                            Delete Account
                          </h4>
                          <p className="text-sm text-red-600">
                            Permanently delete your account and all data
                          </p>
                        </div>
                      </div>
                      <Button variant="destructive" size="sm">
                        Delete Account
                      </Button>
                    </div>
                  </div>
                </CardContent>
              </Card>
            </div>
          </div>
        </TabsContent>
      </Tabs>
    </div>
  );
};

export default ProfileWidget;
