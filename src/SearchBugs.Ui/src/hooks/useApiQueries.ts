import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { apiClient } from "@/lib/api";
import { useAuthStore } from "@/stores";
import type {
  Project,
  Bug,
  CreateProjectDto,
  CreateBugDto,
  ApiResponse,
} from "@/lib/api";

// Helper function to extract data from ApiResponse
const extractApiData = <T>(response: { data: ApiResponse<T> }): T => {
  return response.data.value;
};

// Auth queries that sync with Zustand
export function useLoginMutation() {
  const { setUser, setToken } = useAuthStore();

  return useMutation({
    mutationFn: async ({
      email,
      password,
    }: {
      email: string;
      password: string;
    }) => {
      const response = await apiClient.auth.login({ email, password });
      return response.data;
    },
    onSuccess: (data) => {
      // Assuming the response structure for auth is different
      if (data.token) {
        setToken(data.token);
        // setUser(user); // Will need to fetch user separately
      }
    },
  });
}

export function useRegisterMutation() {
  const { setUser, setToken } = useAuthStore();

  return useMutation({
    mutationFn: async (data: {
      firstName: string;
      lastName: string;
      email: string;
      password: string;
    }) => {
      const response = await apiClient.auth.register(data);
      return response.data;
    },
    onSuccess: (data) => {
      if (data.token) {
        setToken(data.token);
      }
    },
  });
}

// Projects queries
export function useProjectsQuery() {
  return useQuery({
    queryKey: ["projects"],
    queryFn: async () => {
      const response = await apiClient.projects.getAll();
      return extractApiData(response);
    },
    staleTime: 5 * 60 * 1000, // 5 minutes
  });
}

export function useProjectQuery(id: string) {
  return useQuery({
    queryKey: ["projects", id],
    queryFn: async () => {
      const response = await apiClient.projects.getById(id);
      return extractApiData(response);
    },
    enabled: !!id,
  });
}

export function useCreateProjectMutation() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (data: CreateProjectDto) => {
      const response = await apiClient.projects.create(data);
      return extractApiData(response);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["projects"] });
    },
  });
}

export function useDeleteProjectMutation() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (id: string) => {
      await apiClient.projects.delete(id);
      return id;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["projects"] });
    },
  });
}

// Bugs queries
export function useBugsQuery(projectId?: string) {
  return useQuery({
    queryKey: projectId ? ["bugs", "project", projectId] : ["bugs"],
    queryFn: async () => {
      const response = await apiClient.bugs.getAll(projectId);
      return extractApiData(response);
    },
    staleTime: 2 * 60 * 1000, // 2 minutes
  });
}

export function useBugQuery(id: string) {
  return useQuery({
    queryKey: ["bugs", id],
    queryFn: async () => {
      const response = await apiClient.bugs.getById(id);
      return extractApiData(response);
    },
    enabled: !!id,
  });
}

export function useCreateBugMutation() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (data: CreateBugDto) => {
      const response = await apiClient.bugs.create(data);
      return extractApiData(response);
    },
    onSuccess: (_, data) => {
      queryClient.invalidateQueries({ queryKey: ["bugs"] });
      queryClient.invalidateQueries({
        queryKey: ["bugs", "project", data.projectId],
      });
    },
  });
}

export function useUpdateBugMutation() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async ({ id, data }: { id: string; data: Partial<Bug> }) => {
      const response = await apiClient.bugs.update(id, data);
      return extractApiData(response);
    },
    onSuccess: (_, { id, data }) => {
      queryClient.invalidateQueries({ queryKey: ["bugs"] });
      queryClient.invalidateQueries({ queryKey: ["bugs", id] });
      if (data.projectId) {
        queryClient.invalidateQueries({
          queryKey: ["bugs", "project", data.projectId],
        });
      }
    },
  });
}

export function useDeleteBugMutation() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (id: string) => {
      await apiClient.bugs.delete(id);
      return id;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["bugs"] });
    },
  });
}

// Users queries
export function useUsersQuery() {
  return useQuery({
    queryKey: ["users"],
    queryFn: async () => {
      const response = await apiClient.users.getAll();
      return extractApiData(response);
    },
    staleTime: 10 * 60 * 1000, // 10 minutes
  });
}

export function useUserQuery(id: string) {
  return useQuery({
    queryKey: ["users", id],
    queryFn: async () => {
      const response = await apiClient.users.getById(id);
      return extractApiData(response);
    },
    enabled: !!id,
  });
}

export function useUpdateUserMutation() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async ({
      id,
      data,
    }: {
      id: string;
      data: { firstName: string; lastName: string };
    }) => {
      const response = await apiClient.users.update(id, data);
      return extractApiData(response);
    },
    onSuccess: (_, { id }) => {
      queryClient.invalidateQueries({ queryKey: ["users"] });
      queryClient.invalidateQueries({ queryKey: ["users", id] });
    },
  });
}

// Repositories queries - these don't use ApiResponse wrapper
export function useRepositoriesQuery(projectId?: string) {
  return useQuery({
    queryKey: projectId
      ? ["repositories", "project", projectId]
      : ["repositories"],
    queryFn: async () => {
      const response = await apiClient.repositories.getAll();
      return response.data; // Direct data, no ApiResponse wrapper
    },
    staleTime: 5 * 60 * 1000, // 5 minutes
  });
}

export function useRepositoryBranchesQuery(url: string) {
  return useQuery({
    queryKey: ["repositories", url, "branches"],
    queryFn: async () => {
      const response = await apiClient.repositories.getBranches(url);
      return extractApiData(response);
    },
    enabled: !!url,
    staleTime: 2 * 60 * 1000, // 2 minutes
  });
}

export function useRepositoryTreeQuery(url: string, branch?: string) {
  return useQuery({
    queryKey: ["repositories", url, "tree", branch || "main"],
    queryFn: async () => {
      const response = await apiClient.repositories.getTree(
        url,
        branch || "main"
      );
      return extractApiData(response);
    },
    enabled: !!url,
    staleTime: 1 * 60 * 1000, // 1 minute
  });
}

export function useRepositoryFileQuery(
  url: string,
  filePath: string,
  branch?: string
) {
  return useQuery({
    queryKey: ["repositories", url, "file", filePath, branch || "main"],
    queryFn: async () => {
      const response = await apiClient.repositories.getFileContent(
        url,
        branch || "main",
        filePath
      );
      return extractApiData(response);
    },
    enabled: !!url && !!filePath,
    staleTime: 5 * 60 * 1000, // 5 minutes
  });
}
