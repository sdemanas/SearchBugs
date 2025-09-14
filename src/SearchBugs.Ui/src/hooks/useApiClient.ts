import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { apiClient } from "@/lib/api";
import { useToast } from "@/components/ui/use-toast";
import type {
  Project,
  Bug,
  User,
  Repository,
  Comment,
  Attachment,
  TimeEntry,
  CustomField,
  CreateProjectDto,
  CreateBugDto,
  UpdateBugDto,
  ApiResponse,
} from "@/lib/api";

// Helper function to extract data from ApiResponse or return as-is
const extractData = <T>(response: { data: T | ApiResponse<T> }): T => {
  const data = response.data;
  if (data && typeof data === "object" && "value" in data) {
    return (data as ApiResponse<T>).value;
  }
  return data as T;
};

export const useApiClient = () => {
  const queryClient = useQueryClient();
  const { toast } = useToast();

  // Projects
  const useProjects = () => {
    return useQuery<Project[]>({
      queryKey: ["projects"],
      queryFn: () => apiClient.projects.getAll().then(extractData),
    });
  };

  const useCreateProject = () => {
    return useMutation<Project, Error, CreateProjectDto>({
      mutationFn: (data) => apiClient.projects.create(data).then(extractData),
      onSuccess: () => {
        queryClient.invalidateQueries({ queryKey: ["projects"] });
        toast({
          title: "Success",
          description: "Project created successfully",
        });
      },
      onError: (error) => {
        toast({
          title: "Error",
          description: error.message || "Failed to create project",
          variant: "destructive",
        });
      },
    });
  };

  const useDeleteProject = () => {
    return useMutation<void, Error, string>({
      mutationFn: (id) => apiClient.projects.delete(id).then(extractData),
      onSuccess: () => {
        queryClient.invalidateQueries({ queryKey: ["projects"] });
        toast({
          title: "Success",
          description: "Project deleted successfully",
        });
      },
      onError: (error) => {
        toast({
          title: "Error",
          description: error.message || "Failed to delete project",
          variant: "destructive",
        });
      },
    });
  };

  // Bugs
  const useBugs = (projectId?: string) => {
    return useQuery<Bug[]>({
      queryKey: ["bugs", projectId],
      queryFn: () => apiClient.bugs.getAll(projectId).then(extractData),
    });
  };

  const useBug = (id: string) => {
    return useQuery<Bug>({
      queryKey: ["bugs", id],
      queryFn: () => apiClient.bugs.getById(id).then(extractData),
    });
  };

  const useCreateBug = () => {
    return useMutation<Bug, Error, CreateBugDto>({
      mutationFn: (data) => apiClient.bugs.create(data).then(extractData),
      onSuccess: () => {
        queryClient.invalidateQueries({ queryKey: ["bugs"] });
        toast({ title: "Success", description: "Bug created successfully" });
      },
      onError: (error) => {
        toast({
          title: "Error",
          description: error.message || "Failed to create bug",
          variant: "destructive",
        });
      },
    });
  };

  const useUpdateBug = () => {
    return useMutation<Bug, Error, { id: string; data: UpdateBugDto }>({
      mutationFn: ({ id, data }) =>
        apiClient.bugs.update(id, data).then(extractData),
      onSuccess: (_, { id }) => {
        queryClient.invalidateQueries({ queryKey: ["bugs", id] });
        queryClient.invalidateQueries({ queryKey: ["bugs"] });
        toast({ title: "Success", description: "Bug updated successfully" });
      },
      onError: (error) => {
        toast({
          title: "Error",
          description: error.message || "Failed to update bug",
          variant: "destructive",
        });
      },
    });
  };

  const useDeleteBug = () => {
    return useMutation<void, Error, string>({
      mutationFn: (id) => apiClient.bugs.delete(id).then(extractData),
      onSuccess: () => {
        queryClient.invalidateQueries({ queryKey: ["bugs"] });
        toast({ title: "Success", description: "Bug deleted successfully" });
      },
      onError: (error) => {
        toast({
          title: "Error",
          description: error.message || "Failed to delete bug",
          variant: "destructive",
        });
      },
    });
  };

  // Bug Comments
  const useBugComments = (bugId: string) => {
    return useQuery({
      queryKey: ["bugs", bugId, "comments"],
      queryFn: () => apiClient.comments.getByBugId(bugId).then(extractData),
    });
  };

  const useAddComment = () => {
    return useMutation<Comment, Error, { bugId: string; content: string }>({
      mutationFn: async ({ bugId, content }) => {
        const response = await apiClient.comments.create({ bugId, content });
        return extractData(response);
      },
      onSuccess: (_, { bugId }) => {
        queryClient.invalidateQueries({
          queryKey: ["bugs", bugId, "comments"],
        });
        toast({ title: "Success", description: "Comment added successfully" });
      },
      onError: (error) => {
        toast({
          title: "Error",
          description: error.message || "Failed to add comment",
          variant: "destructive",
        });
      },
    });
  };

  // Bug Attachments
  const useBugAttachments = (bugId: string) => {
    return useQuery({
      queryKey: ["bugs", bugId, "attachments"],
      queryFn: () => apiClient.bugs.getAttachments(bugId).then(extractData),
    });
  };

  const useAddAttachment = () => {
    return useMutation<Attachment, Error, { bugId: string; file: File }>({
      mutationFn: ({ bugId, file }) =>
        apiClient.bugs.addAttachment(bugId, file).then(extractData),
      onSuccess: (_, { bugId }) => {
        queryClient.invalidateQueries({
          queryKey: ["bugs", bugId, "attachments"],
        });
        toast({
          title: "Success",
          description: "Attachment added successfully",
        });
      },
      onError: (error) => {
        toast({
          title: "Error",
          description: error.message || "Failed to add attachment",
          variant: "destructive",
        });
      },
    });
  };

  // Bug History
  const useBugHistory = (bugId: string) => {
    return useQuery({
      queryKey: ["bugs", bugId, "history"],
      queryFn: () => apiClient.bugs.getHistory(bugId).then(extractData),
    });
  };

  // Bug Time Tracking
  const useBugTimeTracking = (bugId: string) => {
    return useQuery({
      queryKey: ["bugs", bugId, "time-tracking"],
      queryFn: () => apiClient.bugs.getTimeTracking(bugId).then(extractData),
    });
  };

  const useTrackTime = () => {
    return useMutation<
      TimeEntry,
      Error,
      { bugId: string; data: { duration: string; description: string } }
    >({
      mutationFn: async ({ bugId, data }) => {
        const response = await apiClient.bugs.trackTime(bugId, data);
        return extractData(response);
      },
      onSuccess: (_, { bugId }) => {
        queryClient.invalidateQueries({
          queryKey: ["bugs", bugId, "time-tracking"],
        });
        toast({ title: "Success", description: "Time tracked successfully" });
      },
      onError: (error) => {
        toast({
          title: "Error",
          description: error.message || "Failed to track time",
          variant: "destructive",
        });
      },
    });
  };

  // Bug Custom Fields
  const useBugCustomFields = (bugId: string) => {
    return useQuery({
      queryKey: ["bugs", bugId, "custom-fields"],
      queryFn: () => apiClient.bugs.getCustomFields(bugId).then(extractData),
    });
  };

  const useAddCustomField = () => {
    return useMutation<
      CustomField,
      Error,
      { bugId: string; data: { name: string; value: string } }
    >({
      mutationFn: async ({ bugId, data }) => {
        const response = await apiClient.bugs.addCustomField(bugId, data);
        return extractData(response);
      },
      onSuccess: (_, { bugId }) => {
        queryClient.invalidateQueries({
          queryKey: ["bugs", bugId, "custom-fields"],
        });
        toast({
          title: "Success",
          description: "Custom field added successfully",
        });
      },
      onError: (error) => {
        toast({
          title: "Error",
          description: error.message || "Failed to add custom field",
          variant: "destructive",
        });
      },
    });
  };

  // Users
  const useUsers = () => {
    return useQuery<User[]>({
      queryKey: ["users"],
      queryFn: () => apiClient.users.getAll().then(extractData),
    });
  };

  const useUser = (id: string) => {
    return useQuery<User>({
      queryKey: ["users", id],
      queryFn: () => apiClient.users.getById(id).then(extractData),
    });
  };

  const useUpdateUser = () => {
    return useMutation<
      User,
      Error,
      { id: string; data: { firstName: string; lastName: string } }
    >({
      mutationFn: ({ id, data }) =>
        apiClient.users.update(id, data).then(extractData),
      onSuccess: (_, { id }) => {
        queryClient.invalidateQueries({ queryKey: ["users", id] });
        queryClient.invalidateQueries({ queryKey: ["users"] });
        toast({ title: "Success", description: "User updated successfully" });
      },
      onError: (error) => {
        toast({
          title: "Error",
          description: error.message || "Failed to update user",
          variant: "destructive",
        });
      },
    });
  };

  const useAssignRole = () => {
    return useMutation<void, Error, { id: string; role: string }>({
      mutationFn: ({ id, role }) =>
        apiClient.users.assignRole(id, role).then(extractData),
      onSuccess: (_, { id }) => {
        queryClient.invalidateQueries({ queryKey: ["users", id] });
        queryClient.invalidateQueries({ queryKey: ["users"] });
        toast({ title: "Success", description: "Role assigned successfully" });
      },
      onError: (error) => {
        toast({
          title: "Error",
          description: error.message || "Failed to assign role",
          variant: "destructive",
        });
      },
    });
  };

  // Repositories (these don't use ApiResponse wrapper based on the API structure)
  const useRepositories = (projectId?: string) => {
    return useQuery<Repository[]>({
      queryKey: ["repositories", projectId],
      queryFn: () => apiClient.repositories.getAll().then(extractData),
    });
  };

  const useRepositoryDetails = (url: string, path: string) => {
    return useQuery({
      queryKey: ["repositories", url, path],
      queryFn: () =>
        apiClient.repositories.getDetails(url, path).then(extractData),
    });
  };

  const useCreateRepository = () => {
    return useMutation<
      Repository,
      Error,
      { name: string; url: string; projectId: string; description?: string }
    >({
      mutationFn: (data) =>
        apiClient.repositories
          .create({ description: "", ...data })
          .then(extractData),
      onSuccess: () => {
        queryClient.invalidateQueries({
          queryKey: ["repositories"],
        });
        toast({
          title: "Success",
          description: "Repository created successfully",
        });
      },
      onError: (error) => {
        toast({
          title: "Error",
          description: error.message || "Failed to create repository",
          variant: "destructive",
        });
      },
    });
  };

  const useDeleteRepository = () => {
    return useMutation<void, Error, string>({
      mutationFn: (url) =>
        apiClient.repositories.delete(url).then(extractData),
      onSuccess: () => {
        queryClient.invalidateQueries({ queryKey: ["repositories"] });
        toast({
          title: "Success",
          description: "Repository deleted successfully",
        });
      },
      onError: (error) => {
        toast({
          title: "Error",
          description: error.message || "Failed to delete repository",
          variant: "destructive",
        });
      },
    });
  };

  return {
    // Projects
    useProjects,
    useCreateProject,
    useDeleteProject,

    // Bugs
    useBugs,
    useBug,
    useCreateBug,
    useUpdateBug,
    useDeleteBug,
    useBugComments,
    useAddComment,
    useBugAttachments,
    useAddAttachment,
    useBugHistory,
    useBugTimeTracking,
    useTrackTime,
    useBugCustomFields,
    useAddCustomField,

    // Users
    useUsers,
    useUser,
    useUpdateUser,
    useAssignRole,

    // Repositories
    useRepositories,
    useRepositoryDetails,
    useCreateRepository,
    useDeleteRepository,
  };
};
