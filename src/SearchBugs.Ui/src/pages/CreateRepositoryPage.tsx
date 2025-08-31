import React from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { useNavigate } from "react-router-dom";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Textarea } from "@/components/ui/textarea";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { useToast } from "@/components/ui/use-toast";
import { ArrowLeft, GitBranch, Loader2 } from "lucide-react";
import { apiClient, Project, RepositoryType } from "@/lib/api";
import { PageLoadingState } from "@/components/ui/loading";

// Zod schema for repository validation
const createRepositorySchema = z.object({
  name: z
    .string()
    .min(1, "Repository name is required")
    .min(3, "Repository name must be at least 3 characters")
    .max(100, "Repository name must not exceed 100 characters")
    .regex(
      /^[a-zA-Z0-9_-]+$/,
      "Repository name can only contain letters, numbers, underscores, and hyphens"
    ),
  description: z
    .string()
    .min(1, "Description is required")
    .min(10, "Description must be at least 10 characters")
    .max(500, "Description must not exceed 500 characters"),
  url: z
    .string()
    .min(1, "Repository URL is required")
    .url("Please enter a valid URL")
    .refine((url) => {
      // Check if it's a valid Git repository URL pattern
      const gitPatterns = [
        /^https?:\/\/github\.com\/.+\/.+\.git$/,
        /^https?:\/\/gitlab\.com\/.+\/.+\.git$/,
        /^https?:\/\/bitbucket\.org\/.+\/.+\.git$/,
        /^git@github\.com:.+\/.+\.git$/,
        /^git@gitlab\.com:.+\/.+\.git$/,
        /^https?:\/\/.+\/.+\.git$/,
      ];
      return gitPatterns.some((pattern) => pattern.test(url));
    }, "Please enter a valid Git repository URL (should end with .git)"),
  projectId: z.string().min(1, "Please select a project"),
  type: z.nativeEnum(RepositoryType, {
    errorMap: () => ({ message: "Please select a repository type" }),
  }),
});

type CreateRepositoryForm = z.infer<typeof createRepositorySchema>;

export const CreateRepositoryPage: React.FC = () => {
  const navigate = useNavigate();
  const { toast } = useToast();
  const queryClient = useQueryClient();

  // Fetch projects for the dropdown
  const {
    data: projectsResponse,
    isLoading: isLoadingProjects,
    error: projectsError,
  } = useQuery({
    queryKey: ["projects"],
    queryFn: async () => {
      const response = await apiClient.projects.getAll();
      return response.data;
    },
  });

  const projects = projectsResponse?.value || [];

  // Form setup
  const {
    register,
    handleSubmit,
    formState: { errors },
    setValue,
  } = useForm<CreateRepositoryForm>({
    resolver: zodResolver(createRepositorySchema),
    defaultValues: {
      name: "",
      description: "",
      url: "",
      projectId: "",
      type: RepositoryType.Git,
    },
  });

  // Create repository mutation
  const createRepositoryMutation = useMutation({
    mutationFn: async (data: CreateRepositoryForm) => {
      // Note: The API expects a different structure than our form
      const apiData = {
        name: data.name,
        description: data.description,
        url: data.url,
        projectId: data.projectId,
      };
      const response = await apiClient.repositories.create(apiData);
      return response.data;
    },
    onSuccess: () => {
      toast({
        title: "Success",
        description: "Repository created successfully!",
      });
      // Invalidate repositories cache
      queryClient.invalidateQueries({ queryKey: ["repositories"] });
      // Navigate back to repositories page
      navigate("/repositories");
    },
    onError: (error: Error) => {
      toast({
        title: "Error",
        description: error.message || "Failed to create repository",
        variant: "destructive",
      });
    },
  });

  // Handle form submission
  const onSubmit = (data: CreateRepositoryForm) => {
    createRepositoryMutation.mutate(data);
  };

  // Repository type options
  const repositoryTypes = [
    { value: RepositoryType.Git, label: "Git" },
    { value: RepositoryType.Svn, label: "Subversion (SVN)" },
    { value: RepositoryType.Mercurial, label: "Mercurial" },
  ];

  // Loading state for projects
  if (isLoadingProjects) {
    return <PageLoadingState text="Loading projects..." />;
  }

  // Error state for projects
  if (projectsError) {
    return (
      <div className="container mx-auto p-6">
        <Card>
          <CardContent className="pt-6">
            <div className="text-center">
              <GitBranch className="h-12 w-12 text-muted-foreground mx-auto mb-4" />
              <h3 className="text-lg font-semibold mb-2">
                Error loading projects
              </h3>
              <p className="text-muted-foreground mb-4">
                Unable to load projects. Please try again.
              </p>
              <Button onClick={() => window.location.reload()}>
                Try Again
              </Button>
            </div>
          </CardContent>
        </Card>
      </div>
    );
  }

  return (
    <div className="container mx-auto p-6 max-w-2xl">
      <div className="space-y-6">
        {/* Header */}
        <div className="flex items-center gap-4">
          <Button variant="ghost" onClick={() => navigate("/repositories")}>
            <ArrowLeft className="h-4 w-4 mr-2" />
            Back to Repositories
          </Button>
        </div>

        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <GitBranch className="h-5 w-5" />
              Create New Repository
            </CardTitle>
            <CardDescription>
              Add a new repository to track and manage your code.
            </CardDescription>
          </CardHeader>
          <CardContent>
            <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
              {/* Project Selection */}
              <div className="space-y-2">
                <Label htmlFor="projectId">Project *</Label>
                <Select onValueChange={(value) => setValue("projectId", value)}>
                  <SelectTrigger>
                    <SelectValue placeholder="Select a project" />
                  </SelectTrigger>
                  <SelectContent>
                    {projects.length === 0 ? (
                      <SelectItem value="no-projects" disabled>
                        No projects available
                      </SelectItem>
                    ) : (
                      projects.map((project: Project) => (
                        <SelectItem key={project.id} value={project.id}>
                          {project.name}
                        </SelectItem>
                      ))
                    )}
                  </SelectContent>
                </Select>
                <p className="text-sm text-muted-foreground">
                  Choose the project this repository belongs to.
                </p>
                {errors.projectId && (
                  <p className="text-sm text-destructive">
                    {errors.projectId.message}
                  </p>
                )}
              </div>

              {/* Repository Name */}
              <div className="space-y-2">
                <Label htmlFor="name">Repository Name *</Label>
                <Input
                  id="name"
                  placeholder="my-awesome-project"
                  {...register("name")}
                />
                <p className="text-sm text-muted-foreground">
                  A unique name for your repository. Use letters, numbers,
                  underscores, and hyphens only.
                </p>
                {errors.name && (
                  <p className="text-sm text-destructive">
                    {errors.name.message}
                  </p>
                )}
              </div>

              {/* Repository URL */}
              <div className="space-y-2">
                <Label htmlFor="url">Repository URL *</Label>
                <Input
                  id="url"
                  type="url"
                  placeholder="https://github.com/username/repository.git"
                  {...register("url")}
                />
                <p className="text-sm text-muted-foreground">
                  The URL to your Git repository (should end with .git).
                </p>
                {errors.url && (
                  <p className="text-sm text-destructive">
                    {errors.url.message}
                  </p>
                )}
              </div>

              {/* Repository Type */}
              <div className="space-y-2">
                <Label htmlFor="type">Repository Type *</Label>
                <Select
                  onValueChange={(value) =>
                    setValue("type", value as RepositoryType)
                  }
                  defaultValue={RepositoryType.Git}
                >
                  <SelectTrigger>
                    <SelectValue placeholder="Select repository type" />
                  </SelectTrigger>
                  <SelectContent>
                    {repositoryTypes.map((type) => (
                      <SelectItem key={type.value} value={type.value}>
                        {type.label}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
                <p className="text-sm text-muted-foreground">
                  The version control system used by your repository.
                </p>
                {errors.type && (
                  <p className="text-sm text-destructive">
                    {errors.type.message}
                  </p>
                )}
              </div>

              {/* Description */}
              <div className="space-y-2">
                <Label htmlFor="description">Description *</Label>
                <Textarea
                  id="description"
                  placeholder="Describe what this repository is for..."
                  className="min-h-[100px]"
                  {...register("description")}
                />
                <p className="text-sm text-muted-foreground">
                  Provide a detailed description of the repository's purpose and
                  contents.
                </p>
                {errors.description && (
                  <p className="text-sm text-destructive">
                    {errors.description.message}
                  </p>
                )}
              </div>

              {/* Form Actions */}
              <div className="flex items-center justify-between pt-6">
                <Button
                  type="button"
                  variant="outline"
                  onClick={() => navigate("/repositories")}
                  disabled={createRepositoryMutation.isPending}
                >
                  Cancel
                </Button>
                <Button
                  type="submit"
                  disabled={createRepositoryMutation.isPending}
                >
                  {createRepositoryMutation.isPending ? (
                    <>
                      <Loader2 className="h-4 w-4 mr-2 animate-spin" />
                      Creating...
                    </>
                  ) : (
                    <>
                      <GitBranch className="h-4 w-4 mr-2" />
                      Create Repository
                    </>
                  )}
                </Button>
              </div>
            </form>
          </CardContent>
        </Card>

        {/* Help Section */}
        <Card>
          <CardHeader>
            <CardTitle className="text-sm">Need Help?</CardTitle>
          </CardHeader>
          <CardContent className="text-sm text-muted-foreground">
            <ul className="list-disc list-inside space-y-1">
              <li>
                Make sure your repository URL is accessible and ends with .git
              </li>
              <li>
                Repository names should be descriptive and follow naming
                conventions
              </li>
              <li>
                Select the correct project to organize your repositories
                properly
              </li>
              <li>Git is the most commonly used version control system</li>
            </ul>
          </CardContent>
        </Card>
      </div>
    </div>
  );
};
