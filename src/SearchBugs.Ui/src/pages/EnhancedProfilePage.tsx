import React from "react";
import { useQuery } from "@tanstack/react-query";
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader } from "@/components/ui/card";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Textarea } from "@/components/ui/textarea";
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
} from "lucide-react";
import { CardLoadingSkeleton } from "@/components/ui/loading";

interface UserProfile {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  bio?: string;
  location?: string;
  website?: string;
  avatarUrl?: string;
  joinDate: string;
  publicRepositories: number;
  followers: number;
  following: number;
  totalStars: number;
}

const ProfileHeader: React.FC<{
  profile: UserProfile;
  isOwnProfile?: boolean;
}> = ({ profile, isOwnProfile = false }) => {
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
              </div>
              {isOwnProfile && (
                <Button variant="outline" size="sm">
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
                    className="text-primary hover:underline"
                    target="_blank"
                    rel="noopener noreferrer"
                  >
                    Website
                  </a>
                </div>
              )}
              <div className="flex items-center gap-1">
                <Mail className="h-4 w-4" />
                {profile.email}
              </div>
              <div className="flex items-center gap-1">
                <Calendar className="h-4 w-4" />
                Joined{" "}
                {new Date(profile.joinDate).toLocaleDateString("en-US", {
                  month: "long",
                  year: "numeric",
                })}
              </div>
            </div>
          </div>
        </div>
      </CardHeader>

      <CardContent className="pt-0">
        <div className="flex flex-wrap gap-6 text-sm">
          <div className="flex items-center gap-2">
            <Users className="h-4 w-4" />
            <span className="font-medium">{profile.followers}</span>
            <span className="text-muted-foreground">followers</span>
            <span className="text-muted-foreground">·</span>
            <span className="font-medium">{profile.following}</span>
            <span className="text-muted-foreground">following</span>
          </div>
          <div className="flex items-center gap-2">
            <BookOpen className="h-4 w-4" />
            <span className="font-medium">{profile.publicRepositories}</span>
            <span className="text-muted-foreground">repositories</span>
          </div>
          <div className="flex items-center gap-2">
            <Star className="h-4 w-4" />
            <span className="font-medium">{profile.totalStars}</span>
            <span className="text-muted-foreground">stars</span>
          </div>
        </div>
      </CardContent>
    </Card>
  );
};

const ProfileSettings: React.FC<{ profile: UserProfile }> = ({ profile }) => {
  const [formData, setFormData] = React.useState({
    firstName: profile.firstName,
    lastName: profile.lastName,
    email: profile.email,
    bio: profile.bio || "",
    location: profile.location || "",
    website: profile.website || "",
  });

  const handleInputChange = (field: string, value: string) => {
    setFormData((prev) => ({ ...prev, [field]: value }));
  };

  return (
    <div className="space-y-6">
      <Card>
        <CardHeader>
          <h3 className="text-lg font-semibold">Personal Information</h3>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div className="space-y-2">
              <Label htmlFor="firstName">First Name</Label>
              <Input
                id="firstName"
                value={formData.firstName}
                onChange={(e) => handleInputChange("firstName", e.target.value)}
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="lastName">Last Name</Label>
              <Input
                id="lastName"
                value={formData.lastName}
                onChange={(e) => handleInputChange("lastName", e.target.value)}
              />
            </div>
          </div>

          <div className="space-y-2">
            <Label htmlFor="email">Email</Label>
            <Input
              id="email"
              type="email"
              value={formData.email}
              onChange={(e) => handleInputChange("email", e.target.value)}
            />
          </div>

          <div className="space-y-2">
            <Label htmlFor="bio">Bio</Label>
            <Textarea
              id="bio"
              placeholder="Tell us about yourself..."
              value={formData.bio}
              onChange={(e) => handleInputChange("bio", e.target.value)}
              rows={3}
            />
          </div>

          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div className="space-y-2">
              <Label htmlFor="location">Location</Label>
              <Input
                id="location"
                placeholder="City, Country"
                value={formData.location}
                onChange={(e) => handleInputChange("location", e.target.value)}
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="website">Website</Label>
              <Input
                id="website"
                placeholder="https://yourwebsite.com"
                value={formData.website}
                onChange={(e) => handleInputChange("website", e.target.value)}
              />
            </div>
          </div>

          <div className="flex justify-end gap-2">
            <Button variant="outline">Cancel</Button>
            <Button>Save Changes</Button>
          </div>
        </CardContent>
      </Card>
    </div>
  );
};

const ActivityFeed: React.FC = () => {
  const activities = [
    {
      id: "1",
      type: "repository",
      action: "created",
      target: "my-awesome-project",
      timestamp: "2 hours ago",
      icon: GitFork,
    },
    {
      id: "2",
      type: "issue",
      action: "opened",
      target: "Bug in user authentication",
      timestamp: "1 day ago",
      icon: Activity,
    },
    {
      id: "3",
      type: "star",
      action: "starred",
      target: "react-query",
      timestamp: "3 days ago",
      icon: Star,
    },
  ];

  return (
    <Card>
      <CardHeader>
        <h3 className="text-lg font-semibold">Recent Activity</h3>
      </CardHeader>
      <CardContent>
        <div className="space-y-4">
          {activities.map((activity) => {
            const IconComponent = activity.icon;
            return (
              <div
                key={activity.id}
                className="flex items-start gap-3 pb-3 border-b last:border-b-0"
              >
                <div className="p-2 bg-muted rounded-full">
                  <IconComponent className="h-4 w-4" />
                </div>
                <div className="flex-1 min-w-0">
                  <p className="text-sm">
                    <span className="font-medium">{activity.action}</span>{" "}
                    <span className="text-primary">{activity.target}</span>
                  </p>
                  <p className="text-xs text-muted-foreground">
                    {activity.timestamp}
                  </p>
                </div>
              </div>
            );
          })}
        </div>
      </CardContent>
    </Card>
  );
};

export const EnhancedProfilePage: React.FC = () => {
  const {
    data: profile,
    isLoading,
    error,
  } = useQuery({
    queryKey: ["user-profile"],
    queryFn: async () => {
      // Mock data for now - replace with actual API call
      return {
        id: "1",
        firstName: "Vichea",
        lastName: "Nath",
        email: "vichea@searchbugs.com",
        bio: "Full-stack developer passionate about building great software. Love working with React, TypeScript, and .NET.",
        location: "San Francisco, CA",
        website: "https://vicheanath.dev",
        avatarUrl: "https://avatars.githubusercontent.com/u/48352653",
        joinDate: "2023-01-15T00:00:00Z",
        publicRepositories: 24,
        followers: 156,
        following: 89,
        totalStars: 1247,
      } as UserProfile;
    },
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

  if (error || !profile) {
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

  return (
    <div className="container mx-auto p-6 max-w-6xl">
      <div className="space-y-6">
        <ProfileHeader profile={profile} isOwnProfile={true} />

        <Tabs defaultValue="overview" className="space-y-6">
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
            <ProfileSettings profile={profile} />
          </TabsContent>
        </Tabs>
      </div>
    </div>
  );
};
