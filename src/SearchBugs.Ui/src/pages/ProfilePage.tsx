import React, { useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader } from "@/components/ui/card";
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
} from "lucide-react";
import { CardLoadingSkeleton } from "@/components/ui/loading";
import {
  profileSettingsSchema,
  type ProfileSettingsFormData,
} from "@/lib/validations";
import { apiClient, type UserProfile } from "@/lib/api";
import { formatDistanceToNow } from "date-fns";

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
                      <Badge key={role} variant="secondary" className="text-xs">
                        {role}
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
    <div className="space-y-6">
      <Card>
        <CardHeader>
          <h3 className="text-lg font-semibold">Personal Information</h3>
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

              <FormInput
                control={control}
                name="avatarUrl"
                label="Avatar URL"
                placeholder="https://example.com/avatar.jpg"
                disabled={isSubmitting}
                error={errors.avatarUrl}
              />
            </div>

            {/* Professional Info Section */}
            <div className="space-y-4">
              <h4 className="text-md font-medium">Professional Information</h4>
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

            {/* Contact & Location Section */}
            <div className="space-y-4">
              <h4 className="text-md font-medium">Contact & Location</h4>
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <FormInput
                  control={control}
                  name="location"
                  label="Location"
                  placeholder="City, Country"
                  disabled={isSubmitting}
                  error={errors.location}
                />
                <FormInput
                  control={control}
                  name="phoneNumber"
                  label="Phone Number"
                  placeholder="+1 (555) 123-4567"
                  disabled={isSubmitting}
                  error={errors.phoneNumber}
                />
              </div>

              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <FormInput
                  control={control}
                  name="timeZone"
                  label="Time Zone"
                  placeholder="America/New_York"
                  disabled={isSubmitting}
                  error={errors.timeZone}
                />
                <FormInput
                  control={control}
                  name="dateOfBirth"
                  label="Date of Birth"
                  placeholder="YYYY-MM-DD"
                  disabled={isSubmitting}
                  error={errors.dateOfBirth}
                />
              </div>
            </div>

            {/* Social Links Section */}
            <div className="space-y-4">
              <h4 className="text-md font-medium">Social Links</h4>
              <FormInput
                control={control}
                name="website"
                label="Website"
                placeholder="https://yourwebsite.com"
                disabled={isSubmitting}
                error={errors.website}
              />

              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <FormInput
                  control={control}
                  name="gitHubUsername"
                  label="GitHub Username"
                  placeholder="yourusername"
                  disabled={isSubmitting}
                  error={errors.gitHubUsername}
                />
                <FormInput
                  control={control}
                  name="twitterHandle"
                  label="Twitter Handle"
                  placeholder="@yourusername"
                  disabled={isSubmitting}
                  error={errors.twitterHandle}
                />
              </div>

              <FormInput
                control={control}
                name="linkedInUrl"
                label="LinkedIn URL"
                placeholder="https://linkedin.com/in/yourusername"
                disabled={isSubmitting}
                error={errors.linkedInUrl}
              />
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
                <ActivityFeed />
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
