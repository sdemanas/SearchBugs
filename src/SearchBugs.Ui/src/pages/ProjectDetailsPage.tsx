import { useParams, useNavigate } from "react-router-dom";
import { useApi } from "@/hooks/useApi";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { Badge } from "@/components/ui/badge";
import {
  ArrowLeft,
  Calendar,
  Edit,
  Bug,
  GitBranch,
  Activity,
  Settings,
  Plus,
  FolderKanban,
} from "lucide-react";
import { useTimezone } from "@/hooks/useTimezone";
import { PageLoadingState } from "@/components/ui/loading";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { MoreHorizontal, Trash2 } from "lucide-react";

interface Project {
  id: string;
  name: string;
  description: string;
  createdOnUtc: string;
  updatedOnUtc: string;
}

interface Bug {
  id: string;
  projectId: string;
  title: string;
  description: string;
  status: string;
  priority: string;
  severity: string;
  reporterId: string;
  assigneeId?: string;
  createdOnUtc: string;
  updatedOnUtc: string;
}

interface Repository {
  id: string;
  projectId: string;
  name: string;
  url: string;
  type: string;
  createdOnUtc: string;
  updatedOnUtc: string;
}

export const ProjectDetailsPage = () => {
  const { projectId } = useParams<{ projectId: string }>();
  const navigate = useNavigate();
  const { formatDistanceToNow } = useTimezone();

  const { data: projectData, isLoading: projectLoading } = useApi<Project>(
    `projects/${projectId}`
  );
  const { data: bugsData, isLoading: bugsLoading } = useApi<Bug[]>(
    `bugs?projectId=${projectId}`
  );
  const { data: repositoriesData, isLoading: repositoriesLoading } = useApi<
    Repository[]
  >(`repositories?projectId=${projectId}`);

  if (projectLoading) {
    return <PageLoadingState />;
  }

  if (!projectData?.isSuccess || !projectData.value) {
    return (
      <div className="container mx-auto py-6 space-y-6 max-w-7xl">
        <div className="flex items-center gap-2">
          <Button
            variant="ghost"
            size="sm"
            onClick={() => navigate("/projects")}
          >
            <ArrowLeft className="h-4 w-4" />
            Back to Projects
          </Button>
        </div>
        <div className="text-center py-12">
          <h2 className="text-xl font-semibold text-muted-foreground">
            Project not found
          </h2>
        </div>
      </div>
    );
  }

  const project = projectData.value;
  const bugs = bugsData?.isSuccess ? bugsData.value : [];
  const repositories = repositoriesData?.isSuccess
    ? repositoriesData.value
    : [];

  // Calculate stats
  const totalBugs = bugs.length;
  const openBugs = bugs.filter(
    (bug) => bug.status !== "Closed" && bug.status !== "Resolved"
  ).length;
  const closedBugs = bugs.filter(
    (bug) => bug.status === "Closed" || bug.status === "Resolved"
  ).length;
  const highPriorityBugs = bugs.filter(
    (bug) => bug.priority === "High" || bug.priority === "Critical"
  ).length;

  const getStatusColor = (status: string) => {
    switch (status.toLowerCase()) {
      case "new":
        return "bg-blue-500";
      case "inprogress":
        return "bg-yellow-500";
      case "resolved":
        return "bg-green-500";
      case "closed":
        return "bg-gray-500";
      case "reopened":
        return "bg-red-500";
      default:
        return "bg-gray-500";
    }
  };

  const getPriorityColor = (priority: string) => {
    switch (priority.toLowerCase()) {
      case "low":
        return "bg-green-100 text-green-800 border-green-200";
      case "medium":
        return "bg-yellow-100 text-yellow-800 border-yellow-200";
      case "high":
        return "bg-orange-100 text-orange-800 border-orange-200";
      case "critical":
        return "bg-red-100 text-red-800 border-red-200";
      default:
        return "bg-gray-100 text-gray-800 border-gray-200";
    }
  };

  return (
    <div className="container mx-auto py-6 space-y-6 max-w-7xl">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-4">
          <Button
            variant="ghost"
            size="sm"
            onClick={() => navigate("/projects")}
          >
            <ArrowLeft className="h-4 w-4" />
            Back to Projects
          </Button>
          <div className="flex items-center gap-2">
            <FolderKanban className="h-6 w-6 text-primary" />
            <h1 className="text-2xl font-bold">{project.name}</h1>
          </div>
        </div>
        <Button variant="outline" size="sm">
          <Edit className="h-4 w-4 mr-2" />
          Edit Project
        </Button>
      </div>

      {/* Project Info */}
      <Card>
        <CardHeader>
          <CardTitle>Project Overview</CardTitle>
        </CardHeader>
        <CardContent className="space-y-4">
          <div>
            <h3 className="font-medium text-sm text-muted-foreground mb-1">
              Description
            </h3>
            <p className="text-sm">
              {project.description || "No description provided"}
            </p>
          </div>
          <div className="flex items-center gap-6 text-sm text-muted-foreground">
            <div className="flex items-center gap-2">
              <Calendar className="h-4 w-4" />
              <span>Created {formatDistanceToNow(project.createdOnUtc)}</span>
            </div>
            {project.updatedOnUtc && (
              <div className="flex items-center gap-2">
                <Activity className="h-4 w-4" />
                <span>Updated {formatDistanceToNow(project.updatedOnUtc)}</span>
              </div>
            )}
          </div>
        </CardContent>
      </Card>

      {/* Stats Cards */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-6">
        <Card>
          <CardContent className="p-6">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm font-medium text-muted-foreground">
                  Total Bugs
                </p>
                <p className="text-2xl font-bold">{totalBugs}</p>
              </div>
              <Bug className="h-8 w-8 text-muted-foreground" />
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardContent className="p-6">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm font-medium text-muted-foreground">
                  Open Bugs
                </p>
                <p className="text-2xl font-bold text-orange-600">{openBugs}</p>
              </div>
              <Bug className="h-8 w-8 text-orange-600" />
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardContent className="p-6">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm font-medium text-muted-foreground">
                  Closed Bugs
                </p>
                <p className="text-2xl font-bold text-green-600">
                  {closedBugs}
                </p>
              </div>
              <Bug className="h-8 w-8 text-green-600" />
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardContent className="p-6">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm font-medium text-muted-foreground">
                  High Priority
                </p>
                <p className="text-2xl font-bold text-red-600">
                  {highPriorityBugs}
                </p>
              </div>
              <Bug className="h-8 w-8 text-red-600" />
            </div>
          </CardContent>
        </Card>
      </div>

      {/* Tabs */}
      <Tabs defaultValue="bugs" className="space-y-6">
        <TabsList className="grid w-full grid-cols-3">
          <TabsTrigger value="bugs" className="flex items-center gap-2">
            <Bug className="h-4 w-4" />
            Bugs ({totalBugs})
          </TabsTrigger>
          <TabsTrigger value="repositories" className="flex items-center gap-2">
            <GitBranch className="h-4 w-4" />
            Repositories ({repositories.length})
          </TabsTrigger>
          <TabsTrigger value="settings" className="flex items-center gap-2">
            <Settings className="h-4 w-4" />
            Settings
          </TabsTrigger>
        </TabsList>

        {/* Bugs Tab */}
        <TabsContent value="bugs" className="space-y-4">
          <div className="flex items-center justify-between">
            <h3 className="text-lg font-semibold">Project Bugs</h3>
            <Button
              size="sm"
              onClick={() => navigate(`/projects/${projectId}/add-bug`)}
              className="gap-2"
            >
              <Plus className="h-4 w-4" />
              Add Bug
            </Button>
          </div>

          <Card>
            <CardContent className="p-0">
              {bugsLoading ? (
                <div className="p-6 text-center text-muted-foreground">
                  Loading bugs...
                </div>
              ) : bugs.length === 0 ? (
                <div className="p-6 text-center text-muted-foreground">
                  No bugs found for this project
                </div>
              ) : (
                <Table>
                  <TableHeader>
                    <TableRow>
                      <TableHead>Title</TableHead>
                      <TableHead>Status</TableHead>
                      <TableHead>Priority</TableHead>
                      <TableHead>Created</TableHead>
                      <TableHead className="w-[50px]"></TableHead>
                    </TableRow>
                  </TableHeader>
                  <TableBody>
                    {bugs.map((bug) => (
                      <TableRow
                        key={bug.id}
                        className="cursor-pointer hover:bg-muted/50"
                        onClick={() => navigate(`/bugs/${bug.id}`)}
                      >
                        <TableCell className="font-medium">
                          <div>
                            <p className="font-medium">{bug.title}</p>
                            <p className="text-sm text-muted-foreground truncate max-w-md">
                              {bug.description}
                            </p>
                          </div>
                        </TableCell>
                        <TableCell>
                          <Badge
                            variant="secondary"
                            className={`${getStatusColor(
                              bug.status
                            )} text-white border-0`}
                          >
                            {bug.status}
                          </Badge>
                        </TableCell>
                        <TableCell>
                          <Badge
                            variant="outline"
                            className={getPriorityColor(bug.priority)}
                          >
                            {bug.priority}
                          </Badge>
                        </TableCell>
                        <TableCell className="text-muted-foreground">
                          {formatDistanceToNow(bug.createdOnUtc)}
                        </TableCell>
                        <TableCell>
                          <DropdownMenu>
                            <DropdownMenuTrigger asChild>
                              <Button variant="ghost" className="h-8 w-8 p-0">
                                <MoreHorizontal className="h-4 w-4" />
                              </Button>
                            </DropdownMenuTrigger>
                            <DropdownMenuContent align="end">
                              <DropdownMenuLabel>Actions</DropdownMenuLabel>
                              <DropdownMenuItem
                                onClick={() => navigate(`/bugs/${bug.id}`)}
                              >
                                View Details
                              </DropdownMenuItem>
                              <DropdownMenuItem>
                                <Edit className="mr-2 h-4 w-4" />
                                Edit
                              </DropdownMenuItem>
                              <DropdownMenuItem className="text-red-600">
                                <Trash2 className="mr-2 h-4 w-4" />
                                Delete
                              </DropdownMenuItem>
                            </DropdownMenuContent>
                          </DropdownMenu>
                        </TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              )}
            </CardContent>
          </Card>
        </TabsContent>

        {/* Repositories Tab */}
        <TabsContent value="repositories" className="space-y-4">
          <div className="flex items-center justify-between">
            <h3 className="text-lg font-semibold">Project Repositories</h3>
            <Button
              size="sm"
              onClick={() => navigate("/repositories/create")}
              className="gap-2"
            >
              <Plus className="h-4 w-4" />
              Add Repository
            </Button>
          </div>

          <Card>
            <CardContent className="p-0">
              {repositoriesLoading ? (
                <div className="p-6 text-center text-muted-foreground">
                  Loading repositories...
                </div>
              ) : repositories.length === 0 ? (
                <div className="p-6 text-center text-muted-foreground">
                  No repositories found for this project
                </div>
              ) : (
                <Table>
                  <TableHeader>
                    <TableRow>
                      <TableHead>Name</TableHead>
                      <TableHead>Type</TableHead>
                      <TableHead>URL</TableHead>
                      <TableHead>Created</TableHead>
                      <TableHead className="w-[50px]"></TableHead>
                    </TableRow>
                  </TableHeader>
                  <TableBody>
                    {repositories.map((repo) => (
                      <TableRow
                        key={repo.id}
                        className="cursor-pointer hover:bg-muted/50"
                      >
                        <TableCell className="font-medium">
                          <div className="flex items-center gap-2">
                            <GitBranch className="h-4 w-4" />
                            {repo.name}
                          </div>
                        </TableCell>
                        <TableCell>
                          <Badge variant="outline">{repo.type}</Badge>
                        </TableCell>
                        <TableCell className="font-mono text-sm text-muted-foreground">
                          {repo.url}
                        </TableCell>
                        <TableCell className="text-muted-foreground">
                          {formatDistanceToNow(repo.createdOnUtc)}
                        </TableCell>
                        <TableCell>
                          <DropdownMenu>
                            <DropdownMenuTrigger asChild>
                              <Button variant="ghost" className="h-8 w-8 p-0">
                                <MoreHorizontal className="h-4 w-4" />
                              </Button>
                            </DropdownMenuTrigger>
                            <DropdownMenuContent align="end">
                              <DropdownMenuLabel>Actions</DropdownMenuLabel>
                              <DropdownMenuItem>
                                View Repository
                              </DropdownMenuItem>
                              <DropdownMenuItem>
                                <Edit className="mr-2 h-4 w-4" />
                                Edit
                              </DropdownMenuItem>
                              <DropdownMenuItem className="text-red-600">
                                <Trash2 className="mr-2 h-4 w-4" />
                                Delete
                              </DropdownMenuItem>
                            </DropdownMenuContent>
                          </DropdownMenu>
                        </TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              )}
            </CardContent>
          </Card>
        </TabsContent>

        {/* Settings Tab */}
        <TabsContent value="settings" className="space-y-4">
          <Card>
            <CardHeader>
              <CardTitle>Project Settings</CardTitle>
            </CardHeader>
            <CardContent className="space-y-4">
              <div>
                <h4 className="font-medium mb-2">Danger Zone</h4>
                <p className="text-sm text-muted-foreground mb-4">
                  Permanently delete this project and all associated data.
                </p>
                <Button variant="destructive" size="sm">
                  <Trash2 className="h-4 w-4 mr-2" />
                  Delete Project
                </Button>
              </div>
            </CardContent>
          </Card>
        </TabsContent>
      </Tabs>
    </div>
  );
};
