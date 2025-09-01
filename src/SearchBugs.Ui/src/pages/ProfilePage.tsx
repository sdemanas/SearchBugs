import React, { useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { FormInput, FormTextarea } from "@/components/ui/form";
import { Badge } from "@/components/ui/badge";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { toast } from "sonner";
import {
  MapPin,
  Calendar,
  Mail,
  Link as LinkIcon,
  Edit,
  Users,
  GitFork,
  Star,
  BookOpen,
  Activity,
  Settings,
  CheckCircle,
  XCircle,
  Clock,
  ExternalLink,
  RefreshCw,
  Shield,
  Puzzle,
  HelpCircle,
  Building,
  Cog,
  Lock,
  Eye,
  Smartphone,
  Bell,
  Globe,
  Key,
  Database,
  Trash2,
} from "lucide-react";
import { CardLoadingSkeleton } from "@/components/ui/loading";
import {
  profileSettingsSchema,
  type ProfileSettingsFormData,
} from "@/lib/validations";
import { apiClient, type UserProfile } from "@/lib/api";
import { formatDistanceToNow } from "date-fns";
import { RecentActivitySmall } from "@/components/profile/RecentActivitySmall";
import {
  SecuritySettingsPanel,
  NotificationSettingsPanel,
  IntegrationSettingsPanel,
  AdvancedSettingsPanel,
} from "@/components/settings/SettingsPanels";

const ProfileHeader: React.FC<{
  profile: UserProfile;
  isOwnProfile?: boolean;
  onEditClick?: () => void;
}> = ({ profile, isOwnProfile = false, onEditClick }) => {
  return (
    <Card>
      <CardHeader className="pb-6">
        <div className="flex flex-col sm:flex-row items-start sm:items-center gap-4">
          <Avatar className="h-24 w-24 border-4 border-background shadow-lg">
            <AvatarImage
              src={profile.avatarUrl}
              alt={`${profile.firstName} ${profile.lastName}`}
            />
            <AvatarFallback className="text-2xl font-bold bg-primary text-primary-foreground">
              {profile.firstName[0]}
              {profile.lastName[0]}
            </AvatarFallback>
          </Avatar>

          <div className="flex-1 space-y-2">
            <div className="flex items-center justify-between">
              <div>
                <h1 className="text-2xl font-bold">
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
              {isOwnProfile && onEditClick && (
                <Button variant="outline" size="sm" onClick={onEditClick}>
                  <Edit className="h-4 w-4 mr-2" />
                  Edit Profile
                </Button>
              )}
            </div>

            {profile.bio && (
              <p className="text-sm text-muted-foreground max-w-lg">
                {profile.bio}
              </p>
            )}

            <div className="flex flex-wrap gap-4 text-sm text-muted-foreground">
              {profile.location && (
                <div className="flex items-center gap-1">
                  <MapPin className="h-4 w-4" />
                  {profile.location}
                </div>
              )}
              {profile.website && (
                <div className="flex items-center gap-1">
                  <LinkIcon className="h-4 w-4" />
                  <a
                    href={profile.website}
                    className="text-primary hover:underline flex items-center gap-1"
                    target="_blank"
                    rel="noopener noreferrer"
                  >
                    Website
                    <ExternalLink className="h-3 w-3" />
                  </a>
                </div>
              )}
              {profile.company && (
                <div className="flex items-center gap-1">
                  <Activity className="h-4 w-4" />
                  {profile.jobTitle
                    ? `${profile.jobTitle} at ${profile.company}`
                    : profile.company}
                </div>
              )}
              <div className="flex items-center gap-1">
                <Mail className="h-4 w-4" />
                {profile.email}
              </div>
              {profile.createdOnUtc && (
                <div className="flex items-center gap-1">
                  <Calendar className="h-4 w-4" />
                  Joined{" "}
                  {new Date(profile.createdOnUtc).toLocaleDateString("en-US", {
                    month: "long",
                    year: "numeric",
                  })}
                </div>
              )}
            </div>

            {/* Social Links */}
            {(profile.gitHubProfile ||
              profile.linkedInProfile ||
              profile.twitterHandle) && (
              <div className="flex gap-4 text-sm">
                {profile.gitHubProfile && (
                  <a
                    href={`https://github.com/${profile.gitHubProfile}`}
                    className="text-primary hover:underline flex items-center gap-1"
                    target="_blank"
                    rel="noopener noreferrer"
                  >
                    <GitFork className="h-4 w-4" />
                    GitHub
                    <ExternalLink className="h-3 w-3" />
                  </a>
                )}
                {profile.linkedInProfile && (
                  <a
                    href={profile.linkedInProfile}
                    className="text-primary hover:underline flex items-center gap-1"
                    target="_blank"
                    rel="noopener noreferrer"
                  >
                    <Users className="h-4 w-4" />
                    LinkedIn
                    <ExternalLink className="h-3 w-3" />
                  </a>
                )}
                {profile.twitterHandle && (
                  <a
                    href={`https://twitter.com/${profile.twitterHandle}`}
                    className="text-primary hover:underline flex items-center gap-1"
                    target="_blank"
                    rel="noopener noreferrer"
                  >
                    <Activity className="h-4 w-4" />
                    Twitter
                    <ExternalLink className="h-3 w-3" />
                  </a>
                )}
              </div>
            )}
          </div>
        </div>
      </CardHeader>
    </Card>
  );
};

const ProfileSettings: React.FC<{
  profile: UserProfile;
  onCancel: () => void;
}> = ({ profile, onCancel }) => {
  const queryClient = useQueryClient();
  const [activeSettingsTab, setActiveSettingsTab] = useState("profile");
  const {
    control,
    handleSubmit,
    reset,
    formState: { errors, isSubmitting },
  } = useForm<ProfileSettingsFormData>({
    resolver: zodResolver(profileSettingsSchema),
    defaultValues: {
      firstName: profile.firstName,
      lastName: profile.lastName,
      email: profile.email,
      bio: profile.bio || "",
      location: profile.location || "",
      website: profile.website || "",
      company: profile.company || "",
      jobTitle: profile.jobTitle || "",
      twitterHandle: profile.twitterHandle || "",
      linkedInUrl: profile.linkedInProfile || "",
      gitHubUsername: profile.gitHubProfile || "",
      phoneNumber: profile.phoneNumber || "",
      timeZone: profile.timeZone || "",
      avatarUrl: profile.avatarUrl || "",
      dateOfBirth: profile.dateOfBirth ? profile.dateOfBirth.split("T")[0] : "", // Convert to date input format
    },
  });

  const updateProfileMutation = useMutation({
    mutationFn: apiClient.profile.update,
    onSuccess: (response) => {
      if (response.data.isSuccess) {
        toast.success("Profile updated successfully!");
        queryClient.invalidateQueries({ queryKey: ["user-profile"] });
        onCancel();
      } else {
        toast.error(response.data.error.message || "Failed to update profile");
      }
    },
    onError: (error) => {
      console.error("Profile update error:", error);
      toast.error("Failed to update profile");
    },
  });

  const onSubmit = async (data: ProfileSettingsFormData) => {
    updateProfileMutation.mutate({
      firstName: data.firstName,
      lastName: data.lastName,
      bio: data.bio,
      location: data.location,
      website: data.website,
      gitHubUsername: data.gitHubUsername,
      linkedInUrl: data.linkedInUrl,
      twitterHandle: data.twitterHandle,
      company: data.company,
      jobTitle: data.jobTitle,
      phoneNumber: data.phoneNumber,
      timeZone: data.timeZone,
      avatarUrl: data.avatarUrl,
      dateOfBirth: data.dateOfBirth
        ? new Date(data.dateOfBirth).toISOString()
        : undefined,
    });
  };

  const handleCancel = () => {
    reset({
      firstName: profile.firstName,
      lastName: profile.lastName,
      email: profile.email,
      bio: profile.bio || "",
      location: profile.location || "",
      website: profile.website || "",
      company: profile.company || "",
      jobTitle: profile.jobTitle || "",
      twitterHandle: profile.twitterHandle || "",
      linkedInUrl: profile.linkedInProfile || "",
      gitHubUsername: profile.gitHubProfile || "",
      phoneNumber: profile.phoneNumber || "",
      timeZone: profile.timeZone || "",
      avatarUrl: profile.avatarUrl || "",
      dateOfBirth: profile.dateOfBirth ? profile.dateOfBirth.split("T")[0] : "",
    });
    onCancel();
  };

  return (
    <div className="grid grid-cols-1 lg:grid-cols-4 gap-6">
      {/* Settings Navigation */}
      <Card className="lg:col-span-1 h-fit">
        <CardHeader className="pb-3">
          <CardTitle className="text-lg">Settings</CardTitle>
        </CardHeader>
        <CardContent>
          <nav className="space-y-2">
            <button
              className={`w-full text-left p-3 rounded-lg transition-colors flex items-center gap-3 ${
                activeSettingsTab === "profile"
                  ? "bg-primary/10 text-primary"
                  : "hover:bg-muted"
              }`}
              onClick={() => setActiveSettingsTab("profile")}
            >
              <Settings className="h-4 w-4" />
              <span
                className={activeSettingsTab === "profile" ? "font-medium" : ""}
              >
                Profile
              </span>
            </button>
            <button
              className={`w-full text-left p-3 rounded-lg transition-colors flex items-center gap-3 ${
                activeSettingsTab === "security"
                  ? "bg-primary/10 text-primary"
                  : "hover:bg-muted"
              }`}
              onClick={() => setActiveSettingsTab("security")}
            >
              <Shield className="h-4 w-4" />
              <span
                className={
                  activeSettingsTab === "security" ? "font-medium" : ""
                }
              >
                Security
              </span>
            </button>
            <button
              className={`w-full text-left p-3 rounded-lg transition-colors flex items-center gap-3 ${
                activeSettingsTab === "integrations"
                  ? "bg-primary/10 text-primary"
                  : "hover:bg-muted"
              }`}
              onClick={() => setActiveSettingsTab("integrations")}
            >
              <Puzzle className="h-4 w-4" />
              <span
                className={
                  activeSettingsTab === "integrations" ? "font-medium" : ""
                }
              >
                Integrations
              </span>
            </button>
            <button
              className={`w-full text-left p-3 rounded-lg transition-colors flex items-center gap-3 ${
                activeSettingsTab === "support"
                  ? "bg-primary/10 text-primary"
                  : "hover:bg-muted"
              }`}
              onClick={() => setActiveSettingsTab("support")}
            >
              <HelpCircle className="h-4 w-4" />
              <span
                className={activeSettingsTab === "support" ? "font-medium" : ""}
              >
                Support
              </span>
            </button>
            <button
              className={`w-full text-left p-3 rounded-lg transition-colors flex items-center gap-3 ${
                activeSettingsTab === "organizations"
                  ? "bg-primary/10 text-primary"
                  : "hover:bg-muted"
              }`}
              onClick={() => setActiveSettingsTab("organizations")}
            >
              <Building className="h-4 w-4" />
              <span
                className={
                  activeSettingsTab === "organizations" ? "font-medium" : ""
                }
              >
                Organizations
              </span>
            </button>
            <button
              className={`w-full text-left p-3 rounded-lg transition-colors flex items-center gap-3 ${
                activeSettingsTab === "advanced"
                  ? "bg-primary/10 text-primary"
                  : "hover:bg-muted"
              }`}
              onClick={() => setActiveSettingsTab("advanced")}
            >
              <Cog className="h-4 w-4" />
              <span
                className={
                  activeSettingsTab === "advanced" ? "font-medium" : ""
                }
              >
                Advanced
              </span>
            </button>
          </nav>
        </CardContent>
      </Card>

      {/* Settings Content */}
      <div className="lg:col-span-3 space-y-6">
        {/* Profile Settings */}
        {activeSettingsTab === "profile" && (
          <Card>
            <CardHeader>
              <CardTitle className="text-lg flex items-center gap-2">
                <Settings className="h-5 w-5" />
                Profile Settings
              </CardTitle>
            </CardHeader>
            <CardContent className="space-y-4">
              <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
                {/* Basic Info Section */}
                <div className="space-y-4">
                  <h4 className="text-md font-medium">Basic Information</h4>
                  <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                    <FormInput
                      control={control}
                      name="firstName"
                      label="First Name"
                      required
                      disabled={isSubmitting}
                      error={errors.firstName}
                    />
                    <FormInput
                      control={control}
                      name="lastName"
                      label="Last Name"
                      required
                      disabled={isSubmitting}
                      error={errors.lastName}
                    />
                  </div>

                  <FormInput
                    control={control}
                    name="email"
                    label="Email"
                    type="email"
                    required
                    disabled={true} // Email should not be editable
                    error={errors.email}
                  />

                  <FormTextarea
                    control={control}
                    name="bio"
                    label="Bio"
                    placeholder="Tell us about yourself..."
                    rows={3}
                    disabled={isSubmitting}
                    error={errors.bio}
                  />
                </div>

                {/* Professional Info Section */}
                <div className="space-y-4">
                  <h4 className="text-md font-medium">
                    Professional Information
                  </h4>
                  <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                    <FormInput
                      control={control}
                      name="company"
                      label="Company"
                      placeholder="Your company name"
                      disabled={isSubmitting}
                      error={errors.company}
                    />
                    <FormInput
                      control={control}
                      name="jobTitle"
                      label="Job Title"
                      placeholder="Your job title"
                      disabled={isSubmitting}
                      error={errors.jobTitle}
                    />
                  </div>
                </div>

                <div className="flex justify-end gap-2">
                  <Button
                    type="button"
                    variant="outline"
                    onClick={handleCancel}
                    disabled={isSubmitting}
                  >
                    Cancel
                  </Button>
                  <Button type="submit" disabled={isSubmitting}>
                    {isSubmitting ? (
                      <>
                        <RefreshCw className="h-4 w-4 mr-2 animate-spin" />
                        Saving...
                      </>
                    ) : (
                      "Save Changes"
                    )}
                  </Button>
                </div>
              </form>
            </CardContent>
          </Card>
        )}

        {/* Security Settings */}
        {activeSettingsTab === "security" && <SecuritySettingsPanel />}

        {/* Integrations Settings */}
        {activeSettingsTab === "integrations" && <IntegrationSettingsPanel />}

        {/* Support Settings */}
        {activeSettingsTab === "support" && (
          <Card>
            <CardHeader>
              <CardTitle className="text-lg flex items-center gap-2">
                <HelpCircle className="h-5 w-5" />
                Support & Help
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
        )}

        {/* Organizations Settings */}
        {activeSettingsTab === "organizations" && (
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
                      <h4 className="font-medium">OpenSource Contributors</h4>
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
        )}

        {/* Advanced Settings */}
        {activeSettingsTab === "advanced" && (
          <div className="space-y-6">
            <NotificationSettingsPanel />
            <AdvancedSettingsPanel />
          </div>
        )}
      </div>
    </div>
  );
};

const getActivityIcon = (iconName: string) => {
  const iconMap: Record<string, React.ComponentType<{ className?: string }>> = {
    Bug: Activity,
    FolderOpen: BookOpen,
    User: Users,
    Shield: Star,
    LogIn: Activity,
    UserPlus: Users,
    Lock: Star,
    GitBranch: GitFork,
    Bell: Activity,
    Plus: Activity,
    Edit: Edit,
    Trash2: Activity,
    Activity: Activity,
  };

  return iconMap[iconName] || Activity;
};

const ActivityFeed: React.FC = () => {
  const [currentPage, setCurrentPage] = useState(1);
  const pageSize = 20;

  const {
    data: activityData,
    isLoading,
    error,
    refetch,
    isRefetching,
  } = useQuery({
    queryKey: ["user-activity", currentPage],
    queryFn: () =>
      apiClient.profile.getActivity({ pageNumber: currentPage, pageSize }),
  });

  if (isLoading) {
    return (
      <Card>
        <CardHeader>
          <h3 className="text-lg font-semibold">Recent Activity</h3>
        </CardHeader>
        <CardContent>
          <div className="space-y-4">
            {[...Array(5)].map((_, i) => (
              <CardLoadingSkeleton key={i} />
            ))}
          </div>
        </CardContent>
      </Card>
    );
  }

  if (error || !activityData?.data.isSuccess) {
    return (
      <Card>
        <CardHeader>
          <h3 className="text-lg font-semibold">Recent Activity</h3>
        </CardHeader>
        <CardContent>
          <div className="text-center py-8 text-muted-foreground">
            <Activity className="h-12 w-12 mx-auto mb-4 opacity-50" />
            <p>Failed to load activity</p>
            <Button
              variant="outline"
              size="sm"
              className="mt-4"
              onClick={() => refetch()}
            >
              Try Again
            </Button>
          </div>
        </CardContent>
      </Card>
    );
  }

  const activities = activityData.data.value.activities;

  return (
    <Card>
      <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-4">
        <h3 className="text-lg font-semibold">Recent Activity</h3>
        <Button
          variant="outline"
          size="sm"
          onClick={() => refetch()}
          disabled={isRefetching}
        >
          <RefreshCw
            className={`h-4 w-4 ${isRefetching ? "animate-spin" : ""}`}
          />
        </Button>
      </CardHeader>
      <CardContent>
        {activities.length === 0 ? (
          <div className="text-center py-8 text-muted-foreground">
            <Activity className="h-12 w-12 mx-auto mb-4 opacity-50" />
            <p>No activity yet</p>
            <p className="text-sm">
              Start using the system to see your activity here
            </p>
          </div>
        ) : (
          <div className="space-y-4">
            {activities.map((activity) => {
              const IconComponent = getActivityIcon(activity.icon);
              return (
                <div
                  key={activity.id}
                  className="flex items-start gap-3 pb-3 border-b last:border-b-0"
                >
                  <div
                    className={`p-2 rounded-full ${
                      activity.isSuccess
                        ? "bg-green-100 text-green-600"
                        : "bg-red-100 text-red-600"
                    }`}
                  >
                    <IconComponent className="h-4 w-4" />
                  </div>
                  <div className="flex-1 min-w-0">
                    <div className="flex items-center gap-2">
                      <p className="text-sm font-medium">
                        {activity.description}
                      </p>
                      {activity.isSuccess ? (
                        <CheckCircle className="h-3 w-3 text-green-600" />
                      ) : (
                        <XCircle className="h-3 w-3 text-red-600" />
                      )}
                    </div>
                    <div className="flex items-center gap-2 mt-1">
                      <p className="text-xs text-muted-foreground flex items-center gap-1">
                        <Clock className="h-3 w-3" />
                        {formatDistanceToNow(new Date(activity.timestamp), {
                          addSuffix: true,
                        })}
                      </p>
                      <Badge variant="outline" className="text-xs">
                        {activity.type}
                      </Badge>
                      <span className="text-xs text-muted-foreground">
                        Duration: {activity.duration}
                      </span>
                    </div>
                    {activity.errorMessage && (
                      <p className="text-xs text-red-600 mt-1">
                        Error: {activity.errorMessage}
                      </p>
                    )}
                  </div>
                </div>
              );
            })}

            {/* Pagination */}
            {activityData.data.value.totalCount > pageSize && (
              <div className="flex justify-between items-center pt-4">
                <p className="text-sm text-muted-foreground">
                  Page {activityData.data.value.pageNumber} of{" "}
                  {Math.ceil(activityData.data.value.totalCount / pageSize)}
                </p>
                <div className="flex gap-2">
                  <Button
                    variant="outline"
                    size="sm"
                    onClick={() => setCurrentPage((p) => Math.max(1, p - 1))}
                    disabled={!activityData.data.value.hasPreviousPage}
                  >
                    Previous
                  </Button>
                  <Button
                    variant="outline"
                    size="sm"
                    onClick={() => setCurrentPage((p) => p + 1)}
                    disabled={!activityData.data.value.hasNextPage}
                  >
                    Next
                  </Button>
                </div>
              </div>
            )}
          </div>
        )}
      </CardContent>
    </Card>
  );
};

export const ProfilePage: React.FC = () => {
  const [activeTab, setActiveTab] = useState("overview");

  const {
    data: profileData,
    isLoading,
    error,
  } = useQuery({
    queryKey: ["user-profile"],
    queryFn: apiClient.profile.getCurrent,
  });

  if (isLoading) {
    return (
      <div className="container mx-auto p-6 max-w-6xl">
        <div className="space-y-6">
          <CardLoadingSkeleton />
          <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
            <div className="lg:col-span-2 space-y-4">
              <CardLoadingSkeleton />
              <CardLoadingSkeleton />
            </div>
            <div className="space-y-4">
              <CardLoadingSkeleton />
            </div>
          </div>
        </div>
      </div>
    );
  }

  if (error || !profileData?.data.isSuccess) {
    return (
      <div className="container mx-auto p-6 max-w-6xl">
        <div className="text-center py-12">
          <h2 className="text-2xl font-bold mb-4">Profile not found</h2>
          <p className="text-muted-foreground">
            We couldn't load your profile information.
          </p>
        </div>
      </div>
    );
  }

  const profile = profileData.data.value;

  return (
    <div className="container mx-auto p-6 max-w-6xl">
      <div className="space-y-6">
        <ProfileHeader
          profile={profile}
          isOwnProfile={true}
          onEditClick={() => setActiveTab("settings")}
        />

        {/* Quick Dashboard Section */}
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
          <div className="md:col-span-2 lg:col-span-3">
            <RecentActivitySmall
              maxItems={3}
              showViewAll={true}
              onViewAll={() => setActiveTab("activity")}
            />
          </div>
          <Card>
            <CardHeader className="pb-3">
              <CardTitle className="text-sm">Profile Summary</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="space-y-2 text-sm">
                <div className="flex justify-between">
                  <span>Member since:</span>
                  <span className="font-medium">
                    {profile.createdOnUtc &&
                      new Date(profile.createdOnUtc).getFullYear()}
                  </span>
                </div>
                <div className="flex justify-between">
                  <span>Role:</span>
                  <span className="font-medium">
                    {profile.roles?.[0]?.name || "User"}
                  </span>
                </div>
              </div>
            </CardContent>
          </Card>
        </div>

        <Tabs
          value={activeTab}
          onValueChange={setActiveTab}
          className="space-y-6"
        >
          <TabsList className="grid grid-cols-4 w-full max-w-md">
            <TabsTrigger value="overview">Overview</TabsTrigger>
            <TabsTrigger value="repositories">Repositories</TabsTrigger>
            <TabsTrigger value="activity">Activity</TabsTrigger>
            <TabsTrigger value="settings">
              <Settings className="h-4 w-4 mr-2" />
              Settings
            </TabsTrigger>
          </TabsList>

          <TabsContent value="overview" className="space-y-6">
            <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
              <div className="lg:col-span-2">
                <Card>
                  <CardHeader>
                    <h3 className="text-lg font-semibold">
                      Popular Repositories
                    </h3>
                  </CardHeader>
                  <CardContent>
                    <div className="text-center py-8 text-muted-foreground">
                      <BookOpen className="h-12 w-12 mx-auto mb-4 opacity-50" />
                      <p>No repositories yet</p>
                      <Button variant="outline" size="sm" className="mt-4">
                        Create your first repository
                      </Button>
                    </div>
                  </CardContent>
                </Card>
              </div>
              <div className="space-y-6">
                {/* Compact Recent Activity for sidebar */}
                <RecentActivitySmall
                  maxItems={4}
                  showViewAll={true}
                  onViewAll={() => setActiveTab("activity")}
                />

                {/* Additional Profile Widgets */}
                <Card>
                  <CardHeader className="pb-3">
                    <CardTitle className="text-base">Quick Stats</CardTitle>
                  </CardHeader>
                  <CardContent>
                    <div className="space-y-2 text-sm">
                      <div className="flex justify-between">
                        <span>Profile Views:</span>
                        <span className="font-medium">247</span>
                      </div>
                      <div className="flex justify-between">
                        <span>Contributions:</span>
                        <span className="font-medium">32</span>
                      </div>
                      <div className="flex justify-between">
                        <span>Followers:</span>
                        <span className="font-medium">15</span>
                      </div>
                    </div>
                  </CardContent>
                </Card>
              </div>
            </div>
          </TabsContent>

          <TabsContent value="repositories">
            <Card>
              <CardHeader>
                <div className="flex justify-between items-center">
                  <h3 className="text-lg font-semibold">Repositories</h3>
                  <Button size="sm">
                    <GitFork className="h-4 w-4 mr-2" />
                    New Repository
                  </Button>
                </div>
              </CardHeader>
              <CardContent>
                <div className="text-center py-8 text-muted-foreground">
                  <BookOpen className="h-12 w-12 mx-auto mb-4 opacity-50" />
                  <p>No repositories found</p>
                  <p className="text-sm">
                    Create your first repository to get started
                  </p>
                </div>
              </CardContent>
            </Card>
          </TabsContent>

          <TabsContent value="activity">
            <ActivityFeed />
          </TabsContent>

          <TabsContent value="settings">
            <ProfileSettings
              profile={profile}
              onCancel={() => setActiveTab("overview")}
            />
          </TabsContent>
        </Tabs>
      </div>
    </div>
  );
};

export default ProfilePage;
