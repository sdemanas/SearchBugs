# Zustand Stores Architecture

This directory contains the Zustand-based global state management system for the SearchBugs application.

## Directory Structure

```
stores/
├── index.ts                    # Main exports and type helpers
├── global/                     # Global application state
│   ├── authStore.ts           # Authentication & user session
│   └── uiStore.ts             # UI state (theme, notifications, etc.)
└── features/                  # Feature-specific state management
    ├── projectsStore.ts       # Project management
    ├── bugsStore.ts           # Bug tracking
    ├── repositoriesStore.ts   # Repository & Git operations
    └── usersStore.ts          # User management
```

## Global Stores

### `authStore.ts` - Authentication State

- **Purpose**: Manages user authentication, login/logout, token management
- **Key Features**:
  - JWT token handling with automatic expiration checking
  - User profile management
  - Persistent login state with localStorage
  - Auto-refresh token mechanism
- **API Integration**: `/auth` endpoints (login, register, refresh)

### `uiStore.ts` - UI State Management

- **Purpose**: Global UI state for consistent user experience
- **Key Features**:
  - Dark/light theme switching with persistence
  - Sidebar open/closed state
  - Global notification system with auto-dismissal
  - Modal state management
  - Global search functionality
- **Storage**: Theme and sidebar preferences persisted to localStorage

## Feature Stores

### `projectsStore.ts` - Project Management

- **Purpose**: CRUD operations and state management for projects
- **Key Features**:
  - Project listing with search and sorting
  - Create, read, update, delete operations
  - Real-time filtering and computed values
- **API Integration**: `/projects` endpoints

### `bugsStore.ts` - Bug Tracking

- **Purpose**: Comprehensive bug management with advanced filtering
- **Key Features**:
  - Multi-criteria filtering (status, priority, assignee, project)
  - Pagination support for large datasets
  - Bug lifecycle management (status transitions)
  - Assignment and comment management
- **API Integration**: `/bugs` endpoints with related operations

### `repositoriesStore.ts` - Repository Management

- **Purpose**: Git repository operations and file management
- **Key Features**:
  - Repository CRUD operations
  - Git-specific operations (clone, branches, file tree)
  - File content retrieval and diff viewing
  - Branch management and Git status
- **API Integration**: `/repositories` and Git-related endpoints

### `usersStore.ts` - User Management

- **Purpose**: User administration and role management
- **Key Features**:
  - User CRUD operations with role-based filtering
  - Role assignment and removal
  - Password management
  - Admin-specific functionality
- **API Integration**: `/users` endpoints with role management

## Design Patterns

### State Structure

Each store follows a consistent structure:

```typescript
interface StoreState {
  // Core data
  items: Item[];
  selectedItem: Item | null;

  // Loading & error states
  isLoading: boolean;
  error: string | null;

  // Filter/sort/search
  searchQuery: string;
  filters: FilterState;
  sortBy: string;
  sortOrder: "asc" | "desc";

  // CRUD actions
  fetch: () => Promise<void>;
  create: (data: CreateDto) => Promise<void>;
  update: (id: string, data: UpdateDto) => Promise<void>;
  delete: (id: string) => Promise<void>;

  // UI actions
  setSearch: (query: string) => void;
  setFilter: (key: string, value: any) => void;
  clearError: () => void;
  reset: () => void;

  // Computed values
  filteredItems: () => Item[];
  getItemById: (id: string) => Item | undefined;
}
```

### Error Handling

- All async operations include try/catch blocks
- Errors are stored in state and can be displayed in UI
- `clearError()` action available for dismissing errors
- Failed operations throw errors for component-level handling

### Loading States

- `isLoading` boolean for all async operations
- Loading state management prevents multiple simultaneous requests
- UI can show loading indicators during operations

### Computed Values

- Stores provide computed functions for derived state
- Filtering, sorting, and searching handled in store
- Reduces component complexity and ensures consistency

## API Integration

All stores integrate with the centralized API client (`@/lib/api`):

- Consistent response handling with `ApiResponse<T>` wrapper
- Automatic error extraction and state updates
- Type-safe API calls with TypeScript interfaces
- Standardized CRUD operation patterns

## Usage Examples

### Basic Store Usage

```typescript
import { useProjectsStore } from "@/stores";

function ProjectsList() {
  const {
    projects,
    filteredProjects,
    isLoading,
    fetchProjects,
    setSearchQuery,
  } = useProjectsStore();

  useEffect(() => {
    fetchProjects();
  }, [fetchProjects]);

  return (
    <div>
      <input onChange={(e) => setSearchQuery(e.target.value)} />
      {isLoading ? (
        <Spinner />
      ) : (
        <div>
          {filteredProjects().map((project) => (
            <ProjectCard key={project.id} project={project} />
          ))}
        </div>
      )}
    </div>
  );
}
```

### Selective Subscriptions

```typescript
// Only re-render when user changes
const user = useAuthStore((state) => state.user);

// Only re-render when theme changes
const theme = useUIStore((state) => state.theme);
```

### Cross-Store Actions

```typescript
const logout = useAuthStore((state) => state.logout);
const addNotification = useUIStore((state) => state.addNotification);

const handleLogout = async () => {
  await logout();
  addNotification({ type: "info", message: "Logged out successfully" });
};
```

## Performance Considerations

1. **Selective Subscriptions**: Use state selectors to only re-render when needed
2. **Computed Values**: Leverage store computations rather than component calculations
3. **Batch Updates**: Zustand automatically batches state updates
4. **Memory Management**: Stores automatically clean up when no longer used

## Migration Notes

This Zustand implementation is designed to replace the existing React Context API:

- **Auth Context** → `useAuthStore`
- **Theme Context** → `useUIStore` (theme management)
- **Direct API calls** → Store actions with state management

See `ZUSTAND_INTEGRATION_GUIDE.md` for detailed migration instructions.

## Dependencies

- `zustand`: ^5.0.8 - Core state management library
- `@/lib/api`: Application API client with TypeScript types
- `localStorage`: Browser storage for persistence (auth tokens, UI preferences)

## Best Practices

1. **Keep stores focused**: Each store manages a specific domain
2. **Use TypeScript**: Leverage full type safety for state and actions
3. **Handle errors gracefully**: Store and display errors appropriately
4. **Provide loading states**: Always indicate when operations are in progress
5. **Clear error states**: Provide ways to dismiss errors
6. **Persist important data**: Use localStorage for critical user preferences
7. **Test store operations**: Verify CRUD operations work correctly with API
