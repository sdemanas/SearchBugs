import { create } from "zustand";
import { apiClient, Repository, RepositoryType } from "@/lib/api";

interface RepositoryState {
  // State
  repositories: Repository[];
  selectedRepository: Repository | null;
  currentRepositoryId: string | null;
  isLoading: boolean;
  error: string | null;
  searchQuery: string;
  filterByType: RepositoryType | "all";
  filterByProject: string | "all";
  sortBy: "name" | "url" | "type" | "createdOnUtc" | "updatedOnUtc";
  sortOrder: "asc" | "desc";

  // Actions
  fetchRepositories: () => Promise<void>;
  fetchRepositoryById: (id: string) => Promise<Repository>;
  createRepository: (data: {
    name: string;
    description: string;
    url: string;
    projectId: string;
    type?: RepositoryType;
  }) => Promise<Repository>;
  updateRepository: (id: string, data: Partial<Repository>) => Promise<void>;
  deleteRepository: (id: string) => Promise<void>;
  selectRepository: (repository: Repository | null) => void;
  setCurrentRepositoryId: (repositoryId: string | null) => void;
  setSearchQuery: (query: string) => void;
  setTypeFilter: (type: RepositoryType | "all") => void;
  setProjectFilter: (projectId: string | "all") => void;
  setSorting: (
    sortBy: RepositoryState["sortBy"],
    order: "asc" | "desc"
  ) => void;
  clearError: () => void;
  reset: () => void;

  // Repository-specific actions
  cloneRepository: (url: string, targetPath: string) => Promise<void>;
  getBranches: (url: string) => Promise<string[]>;
  getTree: (url: string, commitSha: string) => Promise<any[]>;
  getFileContent: (
    url: string,
    commitSha: string,
    filePath: string
  ) => Promise<string>;

  // Computed
  filteredRepositories: () => Repository[];
  getRepositoryById: (id: string) => Repository | undefined;
  getRepositoriesByProject: (projectId: string) => Repository[];
  getRepositoriesByType: (type: RepositoryType) => Repository[];
}

export const useRepositoriesStore = create<RepositoryState>((set, get) => ({
  // Initial state
  repositories: [],
  selectedRepository: null,
  currentRepositoryId: null,
  isLoading: false,
  error: null,
  searchQuery: "",
  filterByType: "all",
  filterByProject: "all",
  sortBy: "name",
  sortOrder: "asc",

  // Actions
  fetchRepositories: async () => {
    set({ isLoading: true, error: null });
    try {
      const response = await apiClient.repositories.getAll();
      // Handle direct array response
      const repositories: Repository[] = Array.isArray(response.data)
        ? response.data
        : [];
      set({ repositories, isLoading: false });
    } catch (error) {
      set({
        error:
          error instanceof Error
            ? error.message
            : "Failed to fetch repositories",
        isLoading: false,
      });
    }
  },

  fetchRepositoryById: async (id: string) => {
    set({ isLoading: true, error: null });
    try {
      // Find repository in current state
      const repository = get().repositories.find((r) => r.id === id);
      if (repository) {
        set({ selectedRepository: repository, isLoading: false });
        return repository;
      } else {
        throw new Error("Repository not found");
      }
    } catch (error) {
      const errorMessage =
        error instanceof Error ? error.message : "Failed to fetch repository";
      set({ error: errorMessage, isLoading: false });
      throw new Error(errorMessage);
    }
  },

  createRepository: async (data) => {
    set({ isLoading: true, error: null });
    try {
      const response = await apiClient.repositories.create(data);
      const newRepository = response.data as Repository;
      set((state) => ({
        repositories: [...state.repositories, newRepository],
        isLoading: false,
      }));
      return newRepository;
    } catch (error) {
      const errorMessage =
        error instanceof Error ? error.message : "Failed to create repository";
      set({ error: errorMessage, isLoading: false });
      throw new Error(errorMessage);
    }
  },

  updateRepository: async (id: string, data: Partial<Repository>) => {
    set({ isLoading: true, error: null });
    try {
      // For now, just update locally until you have an update endpoint
      set((state) => ({
        repositories: state.repositories.map((r) =>
          r.id === id ? { ...r, ...data } : r
        ),
        selectedRepository:
          state.selectedRepository?.id === id
            ? { ...state.selectedRepository, ...data }
            : state.selectedRepository,
        isLoading: false,
      }));
    } catch (error) {
      set({
        error:
          error instanceof Error
            ? error.message
            : "Failed to update repository",
        isLoading: false,
      });
      throw error;
    }
  },

  deleteRepository: async (id: string) => {
    set({ isLoading: true, error: null });
    try {
      const repository = get().repositories.find((r) => r.id === id);
      if (repository) {
        await apiClient.repositories.delete(repository.url);
        set((state) => ({
          repositories: state.repositories.filter((r) => r.id !== id),
          selectedRepository:
            state.selectedRepository?.id === id
              ? null
              : state.selectedRepository,
          currentRepositoryId:
            state.currentRepositoryId === id ? null : state.currentRepositoryId,
          isLoading: false,
        }));
      } else {
        throw new Error("Repository not found");
      }
    } catch (error) {
      set({
        error:
          error instanceof Error
            ? error.message
            : "Failed to delete repository",
        isLoading: false,
      });
      throw error;
    }
  },

  selectRepository: (repository: Repository | null) => {
    set({ selectedRepository: repository });
  },

  setCurrentRepositoryId: (repositoryId: string | null) => {
    set({ currentRepositoryId: repositoryId });
  },

  setSearchQuery: (query: string) => {
    set({ searchQuery: query });
  },

  setTypeFilter: (type: RepositoryType | "all") => {
    set({ filterByType: type });
  },

  setProjectFilter: (projectId: string | "all") => {
    set({ filterByProject: projectId });
  },

  setSorting: (sortBy, order) => {
    set({ sortBy, sortOrder: order });
  },

  clearError: () => {
    set({ error: null });
  },

  reset: () => {
    set({
      repositories: [],
      selectedRepository: null,
      currentRepositoryId: null,
      isLoading: false,
      error: null,
      searchQuery: "",
      filterByType: "all",
      filterByProject: "all",
      sortBy: "name",
      sortOrder: "asc",
    });
  },

  // Repository-specific actions
  cloneRepository: async (url: string, targetPath: string) => {
    set({ isLoading: true, error: null });
    try {
      await apiClient.repositories.clone(url, targetPath);
      set({ isLoading: false });
    } catch (error) {
      set({
        error:
          error instanceof Error ? error.message : "Failed to clone repository",
        isLoading: false,
      });
      throw error;
    }
  },

  getBranches: async (url: string) => {
    try {
      const response = await apiClient.repositories.getBranches(url);
      if (response.data?.isSuccess && Array.isArray(response.data.value)) {
        return response.data.value;
      }
      return [];
    } catch (error) {
      console.error("Failed to get branches:", error);
      return [];
    }
  },

  getTree: async (url: string, commitSha: string) => {
    try {
      const response = await apiClient.repositories.getTree(url, commitSha);
      if (response.data?.isSuccess && Array.isArray(response.data.value)) {
        return response.data.value;
      }
      return [];
    } catch (error) {
      console.error("Failed to get tree:", error);
      return [];
    }
  },

  getFileContent: async (url: string, commitSha: string, filePath: string) => {
    try {
      const response = await apiClient.repositories.getFileContent(
        url,
        commitSha,
        filePath
      );
      if (response.data?.isSuccess && typeof response.data.value === "string") {
        return response.data.value;
      }
      return "";
    } catch (error) {
      console.error("Failed to get file content:", error);
      return "";
    }
  },

  // Computed
  filteredRepositories: () => {
    const {
      repositories,
      searchQuery,
      filterByType,
      filterByProject,
      sortBy,
      sortOrder,
    } = get();
    let filtered = repositories;

    // Filter by search query
    if (searchQuery) {
      filtered = filtered.filter(
        (repo) =>
          repo.name.toLowerCase().includes(searchQuery.toLowerCase()) ||
          repo.url.toLowerCase().includes(searchQuery.toLowerCase())
      );
    }

    // Filter by type
    if (filterByType !== "all") {
      filtered = filtered.filter((repo) => repo.type === filterByType);
    }

    // Filter by project
    if (filterByProject !== "all") {
      filtered = filtered.filter((repo) => repo.projectId === filterByProject);
    }

    // Sort
    filtered.sort((a, b) => {
      const aValue = a[sortBy];
      const bValue = b[sortBy];
      const comparison = aValue < bValue ? -1 : aValue > bValue ? 1 : 0;
      return sortOrder === "asc" ? comparison : -comparison;
    });

    return filtered;
  },

  getRepositoryById: (id: string) => {
    return get().repositories.find((repo) => repo.id === id);
  },

  getRepositoriesByProject: (projectId: string) => {
    return get().repositories.filter((repo) => repo.projectId === projectId);
  },

  getRepositoriesByType: (type: RepositoryType) => {
    return get().repositories.filter((repo) => repo.type === type);
  },
}));
