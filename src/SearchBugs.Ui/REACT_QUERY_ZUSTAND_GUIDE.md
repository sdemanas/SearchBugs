# React Query + Zustand Integration Guide

This guide shows how to use **React Query for data loading** and **Zustand for state management** together - a clean separation where React Query handles API calls, caching, and loading states, while Zustand manages application state.

## Architecture Overview

```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   React Query   │    │     Zustand     │    │   Components    │
│                 │    │                 │    │                 │
│ • Data fetching │    │ • App state     │    │ • UI rendering  │
│ • Loading states│ ◄──┤ • Filters       │ ◄──┤ • User actions  │
│ • Error handling│    │ • Selections    │    │ • Event handling│
│ • Cache mgmt    │    │ • UI state      │    │                 │
└─────────────────┘    └─────────────────┘    └─────────────────┘
```

## What Each Tool Handles

### React Query Responsibilities:

- ✅ API calls and data fetching
- ✅ Loading and error states
- ✅ Data caching and revalidation
- ✅ Background updates
- ✅ Mutations (CRUD operations)

### Zustand Store Responsibilities:

- ✅ Application state (filters, selections, UI state)
- ✅ Computed values and data transformations
- ✅ State persistence (localStorage)
- ✅ Cross-component state sharing

## Updated Store Structure

### Auth Store (State Only)

```typescript
// stores/global/authStore.ts
interface AuthState {
  // State only - no loading states
  user: User | null;
  isAuthenticated: boolean;
  token: string | null;

  // Actions for state management only
  setUser: (user: User | null) => void;
  setToken: (token: string | null) => void;
  logout: () => void;

  // Utils
  getTokenPayload: () => any;
  isTokenExpired: () => boolean;
}
```

### Projects Store (State + Computed)

```typescript
// stores/features/projectsStore.ts
interface ProjectsState {
  // State - no data/loading states
  selectedProject: Project | null;
  searchQuery: string;
  sortBy: "name" | "createdOnUtc";
  sortOrder: "asc" | "desc";

  // Actions for state management only
  setSelectedProject: (project: Project | null) => void;
  setSearchQuery: (query: string) => void;
  setSorting: (sortBy: string, order: "asc" | "desc") => void;

  // Computed values (works with React Query data)
  filterProjects: (projects: Project[]) => Project[];
}
```

## React Query Hooks

### Authentication with State Sync

```typescript
// hooks/useApiQueries.ts
export function useLoginMutation() {
  const { setToken } = useAuthStore();

  return useMutation({
    mutationFn: async ({ email, password }: LoginData) => {
      const response = await apiClient.auth.login({ email, password });
      if (response.data.isSuccess) {
        return response.data.value;
      }
      throw new Error(response.data.error?.message);
    },
    onSuccess: (data) => {
      setToken(data.token);
      // Zustand store is updated, components re-render
    },
  });
}

export function useProjectsQuery() {
  return useQuery({
    queryKey: ["projects"],
    queryFn: async () => {
      const response = await apiClient.projects.getAll();
      return response.data.isSuccess ? response.data.value : [];
    },
    staleTime: 5 * 60 * 1000, // 5 minutes
  });
}
```

## Component Usage Examples

### Login Component

```typescript
import { useLoginMutation } from "@/hooks/useApiQueries";
import { useAuthStore } from "@/stores";

function LoginForm() {
  const { isAuthenticated } = useAuthStore(); // State from Zustand
  const loginMutation = useLoginMutation(); // Loading from React Query

  const handleSubmit = async (email: string, password: string) => {
    try {
      await loginMutation.mutateAsync({ email, password });
      // Token automatically set in Zustand store
    } catch (error) {
      console.error("Login failed:", error);
    }
  };

  if (isAuthenticated) {
    return <Redirect to="/dashboard" />;
  }

  return (
    <form onSubmit={handleSubmit}>
      <input type="email" required />
      <input type="password" required />
      <button
        type="submit"
        disabled={loginMutation.isPending} // React Query loading
      >
        {loginMutation.isPending ? "Logging in..." : "Login"}
      </button>
      {loginMutation.error && (
        <div className="error">
          {loginMutation.error.message} {/* React Query error */}
        </div>
      )}
    </form>
  );
}
```

### Projects List with Filtering

```typescript
import { useProjectsQuery } from "@/hooks/useApiQueries";
import { useProjectsStore } from "@/stores";

function ProjectsList() {
  const {
    searchQuery,
    selectedProject,
    filterProjects,
    setSearchQuery,
    setSelectedProject,
  } = useProjectsStore(); // State and actions from Zustand

  const { data: projects = [], isLoading, error } = useProjectsQuery(); // Data and loading from React Query

  // Use Zustand computed function with React Query data
  const filteredProjects = filterProjects(projects);

  if (isLoading) return <div>Loading projects...</div>;
  if (error) return <div>Error: {error.message}</div>;

  return (
    <div>
      <input
        value={searchQuery}
        onChange={(e) => setSearchQuery(e.target.value)}
        placeholder="Search projects..."
      />

      <div className="projects-list">
        {filteredProjects.map((project) => (
          <div
            key={project.id}
            className={selectedProject?.id === project.id ? "selected" : ""}
            onClick={() => setSelectedProject(project)}
          >
            <h3>{project.name}</h3>
            <p>{project.description}</p>
          </div>
        ))}
      </div>

      <div>
        Found {filteredProjects.length} of {projects.length} projects
      </div>
    </div>
  );
}
```

### Bugs Management with Advanced Filtering

```typescript
import { useBugsQuery, useUpdateBugMutation } from "@/hooks/useApiQueries";
import { useBugsStore } from "@/stores";

function BugsList({ projectId }: { projectId?: string }) {
  const {
    searchQuery,
    statusFilter,
    priorityFilter,
    currentPage,
    pageSize,
    filterBugs,
    paginateBugs,
    setSearchQuery,
    setStatusFilter,
    setCurrentPage,
  } = useBugsStore(); // State from Zustand

  const { data: bugs = [], isLoading } = useBugsQuery(projectId); // Data from React Query

  const updateBugMutation = useUpdateBugMutation(); // Mutations from React Query

  // Use Zustand computed functions with React Query data
  const filteredBugs = filterBugs(bugs);
  const { bugs: paginatedBugs, totalPages } = paginateBugs(filteredBugs);

  const handleStatusChange = async (bugId: string, status: BugStatus) => {
    try {
      await updateBugMutation.mutateAsync({
        id: bugId,
        data: { status },
      });
      // React Query will refetch and update the UI
    } catch (error) {
      console.error("Failed to update bug:", error);
    }
  };

  if (isLoading) return <div>Loading bugs...</div>;

  return (
    <div>
      {/* Filters controlled by Zustand */}
      <div className="filters">
        <input
          value={searchQuery}
          onChange={(e) => setSearchQuery(e.target.value)}
          placeholder="Search bugs..."
        />
        <select
          value={statusFilter}
          onChange={(e) => setStatusFilter(e.target.value as BugStatus | "all")}
        >
          <option value="all">All Statuses</option>
          <option value="New">New</option>
          <option value="InProgress">In Progress</option>
          <option value="Resolved">Resolved</option>
        </select>
      </div>

      {/* Results from React Query + Zustand filtering */}
      <div className="bugs-list">
        {paginatedBugs.map((bug) => (
          <div key={bug.id} className="bug-item">
            <h4>{bug.title}</h4>
            <p>Status: {bug.status}</p>
            <select
              value={bug.status}
              onChange={(e) =>
                handleStatusChange(bug.id, e.target.value as BugStatus)
              }
              disabled={updateBugMutation.isPending}
            >
              <option value="New">New</option>
              <option value="InProgress">In Progress</option>
              <option value="Resolved">Resolved</option>
            </select>
          </div>
        ))}
      </div>

      {/* Pagination controlled by Zustand */}
      <div className="pagination">
        {Array.from({ length: totalPages }, (_, i) => (
          <button
            key={i + 1}
            onClick={() => setCurrentPage(i + 1)}
            disabled={currentPage === i + 1}
          >
            {i + 1}
          </button>
        ))}
      </div>

      <div>
        Showing {paginatedBugs.length} of {filteredBugs.length} bugs
      </div>
    </div>
  );
}
```

### Create Project Modal

```typescript
import { useCreateProjectMutation } from "@/hooks/useApiQueries";
import { useUIStore } from "@/stores";

function CreateProjectModal() {
  const { isModalOpen, closeModal } = useUIStore(); // UI state from Zustand
  const createProjectMutation = useCreateProjectMutation(); // Mutation from React Query

  const handleSubmit = async (data: CreateProjectDto) => {
    try {
      await createProjectMutation.mutateAsync(data);
      closeModal(); // Close modal via Zustand
      // React Query will automatically refetch projects list
    } catch (error) {
      console.error("Failed to create project:", error);
    }
  };

  if (!isModalOpen) return null;

  return (
    <Modal onClose={closeModal}>
      <form onSubmit={handleSubmit}>
        <input name="name" placeholder="Project name" required />
        <textarea name="description" placeholder="Description" />
        <div className="actions">
          <button type="button" onClick={closeModal}>
            Cancel
          </button>
          <button type="submit" disabled={createProjectMutation.isPending}>
            {createProjectMutation.isPending ? "Creating..." : "Create Project"}
          </button>
        </div>
        {createProjectMutation.error && (
          <div className="error">{createProjectMutation.error.message}</div>
        )}
      </form>
    </Modal>
  );
}
```

## Key Benefits

### 1. **Clear Separation of Concerns**

- React Query: Data fetching, caching, server state
- Zustand: Application state, UI state, client-side logic

### 2. **Optimal Performance**

- React Query handles caching and prevents unnecessary API calls
- Zustand provides granular subscriptions for minimal re-renders
- Computed values are only recalculated when dependencies change

### 3. **Excellent Developer Experience**

- React Query DevTools for debugging API calls
- Zustand DevTools for state inspection
- TypeScript support throughout

### 4. **Predictable State Flow**

```
User Action → Zustand State Update → Component Re-render
     ↓
React Query Mutation → API Call → Cache Update → Component Re-render
```

## Migration Tips

1. **Remove data/loading/error from Zustand stores**
2. **Move API calls to React Query hooks**
3. **Keep computed values and filters in Zustand**
4. **Use React Query mutations for CRUD operations**
5. **Sync important state changes between the two systems**

This architecture gives you the best of both worlds: powerful data fetching with React Query and flexible state management with Zustand!
