import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { FormInput, FormSelect } from "@/components/ui/form";
import { RichTextEditor } from "@/components/ui/rich-text-editor";
import { useForm, Controller } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { useState, useEffect } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { useToast } from "@/components/ui/use-toast";
import { useAuth } from "@/hooks/useAuth";
import { apiClient } from "@/lib/api";
import { LoaderIcon, ArrowLeft } from "lucide-react";
import { createBugSchema, type CreateBugFormData } from "@/lib/validations";

interface Project {
  id: string;
  name: string;
  description: string;
}

interface User {
  id: string;
  email: string;
  username?: string;
  firstName?: string;
  lastName?: string;
}

export const BugAddPage = () => {
  const [projects, setProjects] = useState<Project[]>([]);
  const [users, setUsers] = useState<User[]>([]);
  const [isLoadingData, setIsLoadingData] = useState(true);
  const [selectedProject, setSelectedProject] = useState<Project | null>(null);

  const navigate = useNavigate();
  const { projectId: urlProjectId } = useParams<{ projectId: string }>();
  const { toast } = useToast();
  const { user } = useAuth();

  const {
    control,
    handleSubmit,
    setValue,
    formState: { errors, isSubmitting },
  } = useForm<CreateBugFormData>({
    resolver: zodResolver(createBugSchema),
    defaultValues: {
      title: "",
      description: "",
      status: "Open",
      priority: "Medium",
      severity: "Medium",
      projectId: urlProjectId || "",
      assigneeId: "",
    },
  });

  useEffect(() => {
    const loadData = async () => {
      try {
        setIsLoadingData(true);
        const [projectsResponse, usersResponse] = await Promise.all([
          apiClient.projects.getAll(),
          apiClient.users.getAll(),
        ]);

        // Extract data from ApiResponse wrapper
        if (projectsResponse.data?.value) {
          setProjects(projectsResponse.data.value);
        }
        if (usersResponse.data?.value) {
          setUsers(usersResponse.data.value);
        }

        // Auto-select project if projectId is in URL
        if (urlProjectId && projectsResponse.data?.value) {
          const project = projectsResponse.data.value.find(
            (p) => p.id === urlProjectId
          );
          if (project) {
            setValue("projectId", urlProjectId);
            setSelectedProject(project);
          }
        }
      } catch (error) {
        console.error("Error loading data:", error);
        toast({
          title: "Error loading data",
          description: "Failed to load projects and users.",
          variant: "destructive",
        });
      } finally {
        setIsLoadingData(false);
      }
    };

    loadData();
  }, [toast, urlProjectId, setValue]);

  // Prepare options for selects
  const projectOptions = projects.map((project) => ({
    value: project.id,
    label: project.name,
  }));

  const userOptions = users.map((user) => ({
    value: user.id,
    label: `${user.firstName || user.username} ${user.lastName} (${
      user.email
    })`,
  }));

  const statusOptions = [
    { value: "Open", label: "Open" },
    { value: "In Progress", label: "In Progress" },
    { value: "Resolved", label: "Resolved" },
    { value: "Closed", label: "Closed" },
  ];

  const priorityOptions = [
    { value: "Low", label: "Low" },
    { value: "Medium", label: "Medium" },
    { value: "High", label: "High" },
  ];

  const severityOptions = [
    { value: "Low", label: "Low" },
    { value: "Medium", label: "Medium" },
    { value: "High", label: "High" },
  ];

  const onSubmit = async (data: CreateBugFormData) => {
    if (!user) return;

    try {
      const response = await apiClient.bugs.create({
        title: data.title,
        description: data.description,
        status: data.status,
        priority: data.priority,
        severity: data.severity,
        projectId: data.projectId,
        assigneeId: data.assigneeId || user.id,
        reporterId: user.id,
      });

      if (response.data?.isSuccess) {
        toast({
          title: "Bug created successfully",
          description: "The bug has been reported and added to the system.",
        });
        navigate("/bugs");
      } else {
        throw new Error(
          response.data?.error?.message || "Failed to create bug"
        );
      }
    } catch (error: unknown) {
      console.error("Error creating bug:", error);
      const errorMessage =
        error instanceof Error
          ? error.message
          : "Failed to create the bug. Please try again.";
      toast({
        title: "Error creating bug",
        description: errorMessage,
        variant: "destructive",
      });
    }
  };

  if (isLoadingData) {
    return (
      <div className="flex items-center justify-center min-h-[400px]">
        <LoaderIcon className="h-8 w-8 animate-spin" />
      </div>
    );
  }

  return (
    <div className="container mx-auto py-6 max-w-4xl">
      <div className="flex items-center gap-4 mb-6">
        <Button
          variant="ghost"
          size="sm"
          onClick={() => navigate(-1)}
          className="gap-1"
        >
          <ArrowLeft className="h-4 w-4" />
          Back
        </Button>
        <h1 className="text-2xl font-bold">
          {selectedProject
            ? `Report Bug for ${selectedProject.name}`
            : "Report a Bug"}
        </h1>
      </div>

      <form onSubmit={handleSubmit(onSubmit)}>
        <Card>
          <CardHeader>
            <CardTitle>Bug Details</CardTitle>
          </CardHeader>
          <CardContent className="space-y-6">
            <FormInput
              control={control}
              name="title"
              label="Title"
              placeholder="Brief description of the bug"
              required
              disabled={isSubmitting}
              error={errors.title}
            />

            <div className="space-y-2">
              <label className="text-sm font-medium">
                Description <span className="text-red-500">*</span>
              </label>
              <Controller
                name="description"
                control={control}
                render={({ field }) => (
                  <RichTextEditor
                    value={field.value}
                    onChange={field.onChange}
                    placeholder="Detailed description of the bug, including steps to reproduce, expected behavior, and actual behavior"
                    disabled={isSubmitting}
                    maxLength={10000}
                  />
                )}
              />
              {errors.description && (
                <p className="text-sm text-red-500">
                  {errors.description.message}
                </p>
              )}
            </div>

            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div className="space-y-1">
                <FormSelect
                  control={control}
                  name="projectId"
                  label="Project"
                  placeholder="Select a project"
                  options={projectOptions}
                  required
                  disabled={isSubmitting || !!urlProjectId}
                  error={errors.projectId}
                />
                {urlProjectId && selectedProject && (
                  <p className="text-xs text-muted-foreground">
                    Project automatically selected from {selectedProject.name}
                  </p>
                )}
              </div>

              <FormSelect
                control={control}
                name="assigneeId"
                label="Assignee"
                placeholder="Select assignee (optional)"
                options={userOptions}
                disabled={isSubmitting}
                error={errors.assigneeId}
              />
            </div>

            <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
              <FormSelect
                control={control}
                name="status"
                label="Status"
                options={statusOptions}
                disabled={isSubmitting}
                error={errors.status}
              />

              <FormSelect
                control={control}
                name="priority"
                label="Priority"
                options={priorityOptions}
                disabled={isSubmitting}
                error={errors.priority}
              />

              <FormSelect
                control={control}
                name="severity"
                label="Severity"
                options={severityOptions}
                disabled={isSubmitting}
                error={errors.severity}
              />
            </div>
          </CardContent>
        </Card>

        <div className="flex items-center gap-2 mt-6">
          <Button
            type="button"
            variant="outline"
            onClick={() => navigate(-1)}
            disabled={isSubmitting}
          >
            Cancel
          </Button>
          <Button type="submit" disabled={isSubmitting}>
            {isSubmitting && (
              <LoaderIcon className="mr-2 h-4 w-4 animate-spin" />
            )}
            Create Bug
          </Button>
        </div>
      </form>
    </div>
  );
};
