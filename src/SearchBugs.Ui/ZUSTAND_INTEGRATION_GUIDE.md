# Zustand Store Integration Guide

This guide shows how to migrate from React Context API to Zustand stores and how to use the new global state management system.

## Overview

The application now has a comprehensive Zustand-based state management system with:

### Global Stores

- **Auth Store** (`useAuthStore`): Authentication state, user management
- **UI Store** (`useUIStore`): Theme, sidebar, notifications, modals, global search

### Feature Stores

- **Projects Store** (`useProjectsStore`): Project management with CRUD operations
- **Bugs Store** (`useBugsStore`): Bug tracking with advanced filtering and pagination
- **Repositories Store** (`useRepositoriesStore`): Repository management with Git operations
- **Users Store** (`useUsersStore`): User management with role assignments

## Migration from React Context

### Before (React Context)

```tsx
// Old auth context usage
import { useAuth } from "@/contexts/AuthContext";

function MyComponent() {
  const { user, login, logout, isAuthenticated } = useAuth();
  // ...
}
```

### After (Zustand)

```tsx
// New Zustand store usage
import { useAuthStore } from "@/stores";

function MyComponent() {
  const { user, login, logout, isAuthenticated } = useAuthStore();
  // ...
}
```

## Basic Usage Examples

### Authentication

```tsx
import { useAuthStore } from "@/stores";

function LoginForm() {
  const { login, isLoading, error } = useAuthStore();

  const handleSubmit = async (email: string, password: string) => {
    try {
      await login(email, password);
      // User is automatically logged in and state updated
    } catch (error) {
      // Error is automatically set in store
      console.error("Login failed:", error);
    }
  };

  return (
    <form onSubmit={handleSubmit}>
      {error && <div className="error">{error}</div>}
      {/* Form fields */}
      <button type="submit" disabled={isLoading}>
        {isLoading ? "Logging in..." : "Login"}
      </button>
    </form>
  );
}
```

### UI State Management

```tsx
import { useUIStore } from "@/stores";

function Header() {
  const { theme, toggleTheme, isSidebarOpen, toggleSidebar, addNotification } =
    useUIStore();

  const handleSuccess = () => {
    addNotification({
      type: "success",
      title: "Success!",
      message: "Operation completed successfully",
    });
  };

  return (
    <header className={theme === "dark" ? "dark" : "light"}>
      <button onClick={toggleSidebar}>
        {isSidebarOpen ? "Close" : "Open"} Sidebar
      </button>
      <button onClick={toggleTheme}>
        Switch to {theme === "dark" ? "light" : "dark"} theme
      </button>
      <button onClick={handleSuccess}>Test Notification</button>
    </header>
  );
}
```

### Project Management

```tsx
import { useProjectsStore } from "@/stores";
import { useEffect } from "react";

function ProjectsList() {
  const {
    projects,
    filteredProjects,
    isLoading,
    fetchProjects,
    setSearchQuery,
    setSorting,
  } = useProjectsStore();

  useEffect(() => {
    fetchProjects();
  }, [fetchProjects]);

  const handleSearch = (query: string) => {
    setSearchQuery(query);
  };

  return (
    <div>
      <input
        type="text"
        placeholder="Search projects..."
        onChange={(e) => handleSearch(e.target.value)}
      />

      <select
        onChange={(e) => setSorting("name", e.target.value as "asc" | "desc")}
      >
        <option value="asc">Name A-Z</option>
        <option value="desc">Name Z-A</option>
      </select>

      {isLoading ? (
        <div>Loading...</div>
      ) : (
        <div>
          {filteredProjects().map((project) => (
            <div key={project.id}>
              <h3>{project.name}</h3>
              <p>{project.description}</p>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
```

### Bug Tracking

```tsx
import { useBugsStore } from "@/stores";

function BugsList() {
  const {
    bugs,
    filteredBugs,
    statusFilter,
    priorityFilter,
    setStatusFilter,
    setPriorityFilter,
    fetchBugs,
    updateBugStatus,
  } = useBugsStore();

  useEffect(() => {
    fetchBugs();
  }, [fetchBugs]);

  const handleStatusChange = async (bugId: string, status: BugStatus) => {
    try {
      await updateBugStatus(bugId, status);
      // Bug status updated automatically in store
    } catch (error) {
      console.error("Failed to update bug status:", error);
    }
  };

  return (
    <div>
      <div className="filters">
        <select
          value={statusFilter}
          onChange={(e) => setStatusFilter(e.target.value)}
        >
          <option value="all">All Statuses</option>
          <option value="New">New</option>
          <option value="InProgress">In Progress</option>
          <option value="Resolved">Resolved</option>
          <option value="Closed">Closed</option>
        </select>

        <select
          value={priorityFilter}
          onChange={(e) => setPriorityFilter(e.target.value)}
        >
          <option value="all">All Priorities</option>
          <option value="Low">Low</option>
          <option value="Medium">Medium</option>
          <option value="High">High</option>
          <option value="Critical">Critical</option>
        </select>
      </div>

      <div className="bugs-list">
        {filteredBugs().map((bug) => (
          <div key={bug.id} className="bug-item">
            <h4>{bug.title}</h4>
            <p>Status: {bug.status}</p>
            <p>Priority: {bug.priority}</p>
            <select
              value={bug.status}
              onChange={(e) =>
                handleStatusChange(bug.id, e.target.value as BugStatus)
              }
            >
              <option value="New">New</option>
              <option value="InProgress">In Progress</option>
              <option value="Resolved">Resolved</option>
              <option value="Closed">Closed</option>
            </select>
          </div>
        ))}
      </div>
    </div>
  );
}
```

## Advanced Patterns

### Selective Subscriptions

Only subscribe to specific parts of the store to optimize re-renders:

```tsx
import { useAuthStore } from "@/stores";

function UserProfile() {
  // Only re-render when user changes
  const user = useAuthStore((state) => state.user);

  return (
    <div>
      {user ? (
        <div>
          <h2>
            {user.firstName} {user.lastName}
          </h2>
          <p>{user.email}</p>
        </div>
      ) : (
        <div>Not logged in</div>
      )}
    </div>
  );
}
```

### Multiple Store Actions

```tsx
import { useAuthStore, useUIStore } from "@/stores";

function LogoutButton() {
  const logout = useAuthStore((state) => state.logout);
  const addNotification = useUIStore((state) => state.addNotification);

  const handleLogout = async () => {
    try {
      await logout();
      addNotification({
        type: "info",
        title: "Logged out",
        message: "You have been logged out successfully",
      });
    } catch (error) {
      addNotification({
        type: "error",
        title: "Logout failed",
        message: "Failed to logout. Please try again.",
      });
    }
  };

  return <button onClick={handleLogout}>Logout</button>;
}
```

### Store Composition

```tsx
import { useProjectsStore, useBugsStore } from "@/stores";

function ProjectDashboard({ projectId }: { projectId: string }) {
  const project = useProjectsStore((state) => state.getProjectById(projectId));

  const projectBugs = useBugsStore((state) =>
    state.getBugsByProject(projectId)
  );

  const openBugsCount = useBugsStore(
    (state) =>
      state
        .getBugsByProject(projectId)
        .filter((bug) => bug.status === "New" || bug.status === "InProgress")
        .length
  );

  return (
    <div>
      <h2>{project?.name}</h2>
      <p>Open bugs: {openBugsCount}</p>
      <p>Total bugs: {projectBugs.length}</p>
    </div>
  );
}
```

## Performance Tips

1. **Selective Subscriptions**: Only subscribe to the parts of state you need
2. **Computed Values**: Use computed functions in stores for derived state
3. **Error Boundaries**: Wrap components using stores in error boundaries
4. **Loading States**: Use loading states from stores to show appropriate UI

## Store Structure

Each store follows a consistent pattern:

```tsx
interface StoreState {
  // Data state
  items: Item[];
  selectedItem: Item | null;

  // UI state
  isLoading: boolean;
  error: string | null;

  // Filter/search state
  searchQuery: string;
  sortBy: string;
  sortOrder: "asc" | "desc";

  // Actions
  fetchItems: () => Promise<void>;
  createItem: (data: CreateItemDto) => Promise<void>;
  updateItem: (id: string, data: UpdateItemDto) => Promise<void>;
  deleteItem: (id: string) => Promise<void>;

  // UI actions
  setSearchQuery: (query: string) => void;
  setSorting: (sortBy: string, order: "asc" | "desc") => void;
  clearError: () => void;

  // Computed
  filteredItems: () => Item[];
}
```

## Migration Checklist

- [ ] Replace `useAuth()` with `useAuthStore()`
- [ ] Replace theme context with `useUIStore()` theme management
- [ ] Replace manual API calls with store actions
- [ ] Update components to use computed values from stores
- [ ] Remove old React Context providers
- [ ] Update error handling to use store error states
- [ ] Test all CRUD operations with new stores

## Best Practices

1. **Keep stores focused**: Each store should handle a specific domain
2. **Use computed values**: Leverage store computed functions for derived state
3. **Handle errors gracefully**: Store errors in state and display appropriately
4. **Persist important state**: Use localStorage for auth and UI preferences
5. **Clear errors**: Provide ways to clear error states
6. **Loading states**: Show loading indicators during async operations
