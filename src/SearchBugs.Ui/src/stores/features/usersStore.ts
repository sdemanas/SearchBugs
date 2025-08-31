import { create } from "zustand";
import { apiClient, User, UserRole } from "@/lib/api";

interface UsersState {
  // State
  users: User[];
  selectedUser: User | null;
  currentUserId: string | null;
  isLoading: boolean;
  error: string | null;
  searchQuery: string;
  roleFilter: string | "all";
  sortBy: "firstName" | "lastName" | "email" | "createdOnUtc" | "modifiedOnUtc";
  sortOrder: "asc" | "desc";

  // Actions
  fetchUsers: () => Promise<void>;
  fetchUserById: (id: string) => Promise<User>;
  updateUser: (
    id: string,
    data: { firstName: string; lastName: string }
  ) => Promise<void>;
  deleteUser: (id: string) => Promise<void>;
  selectUser: (user: User | null) => void;
  setCurrentUserId: (userId: string | null) => void;
  setSearchQuery: (query: string) => void;
  setRoleFilter: (role: string | "all") => void;
  setSorting: (sortBy: UsersState["sortBy"], order: "asc" | "desc") => void;
  clearError: () => void;
  reset: () => void;

  // User-specific actions
  assignRole: (userId: string, role: string) => Promise<void>;
  removeRole: (userId: string, role: string) => Promise<void>;
  changePassword: (
    userId: string,
    currentPassword: string,
    newPassword: string
  ) => Promise<void>;

  // Computed
  filteredUsers: () => User[];
  getUserById: (id: string) => User | undefined;
  getUsersByRole: (role: string) => User[];
  getAdmins: () => User[];
}

export const useUsersStore = create<UsersState>((set, get) => ({
  // Initial state
  users: [],
  selectedUser: null,
  currentUserId: null,
  isLoading: false,
  error: null,
  searchQuery: "",
  roleFilter: "all",
  sortBy: "firstName",
  sortOrder: "asc",

  // Actions
  fetchUsers: async () => {
    set({ isLoading: true, error: null });
    try {
      const response = await apiClient.users.getAll();
      if (response.data.isSuccess) {
        set({ users: response.data.value, isLoading: false });
      } else {
        set({ error: response.data.error.message, isLoading: false });
      }
    } catch (error) {
      set({
        error: error instanceof Error ? error.message : "Failed to fetch users",
        isLoading: false,
      });
    }
  },

  fetchUserById: async (id: string) => {
    set({ isLoading: true, error: null });
    try {
      const response = await apiClient.users.getById(id);
      if (response.data.isSuccess) {
        const user = response.data.value;
        set((state) => ({
          users: state.users.map((u) => (u.id === id ? user : u)),
          selectedUser:
            state.selectedUser?.id === id ? user : state.selectedUser,
          isLoading: false,
        }));
        return user;
      } else {
        set({ error: response.data.error.message, isLoading: false });
        throw new Error(response.data.error.message);
      }
    } catch (error) {
      const errorMessage =
        error instanceof Error ? error.message : "Failed to fetch user";
      set({ error: errorMessage, isLoading: false });
      throw new Error(errorMessage);
    }
  },

  updateUser: async (
    id: string,
    data: { firstName: string; lastName: string }
  ) => {
    set({ isLoading: true, error: null });
    try {
      const response = await apiClient.users.update(id, data);
      if (response.data.isSuccess) {
        const updatedUser = response.data.value;
        set((state) => ({
          users: state.users.map((u) => (u.id === id ? updatedUser : u)),
          selectedUser:
            state.selectedUser?.id === id ? updatedUser : state.selectedUser,
          isLoading: false,
        }));
      } else {
        set({ error: response.data.error.message, isLoading: false });
      }
    } catch (error) {
      set({
        error: error instanceof Error ? error.message : "Failed to update user",
        isLoading: false,
      });
      throw error;
    }
  },

  deleteUser: async (id: string) => {
    set({ isLoading: true, error: null });
    try {
      await apiClient.users.delete(id);
      set((state) => ({
        users: state.users.filter((u) => u.id !== id),
        selectedUser: state.selectedUser?.id === id ? null : state.selectedUser,
        currentUserId: state.currentUserId === id ? null : state.currentUserId,
        isLoading: false,
      }));
    } catch (error) {
      set({
        error: error instanceof Error ? error.message : "Failed to delete user",
        isLoading: false,
      });
      throw error;
    }
  },

  selectUser: (user: User | null) => {
    set({ selectedUser: user });
  },

  setCurrentUserId: (userId: string | null) => {
    set({ currentUserId: userId });
  },

  setSearchQuery: (query: string) => {
    set({ searchQuery: query });
  },

  setRoleFilter: (role: string | "all") => {
    set({ roleFilter: role });
  },

  setSorting: (sortBy, order) => {
    set({ sortBy, sortOrder: order });
  },

  clearError: () => {
    set({ error: null });
  },

  reset: () => {
    set({
      users: [],
      selectedUser: null,
      currentUserId: null,
      isLoading: false,
      error: null,
      searchQuery: "",
      roleFilter: "all",
      sortBy: "firstName",
      sortOrder: "asc",
    });
  },

  // User-specific actions
  assignRole: async (userId: string, role: string) => {
    set({ isLoading: true, error: null });
    try {
      await apiClient.users.assignRole(userId, role);
      // Refresh user data
      await get().fetchUserById(userId);
    } catch (error) {
      set({
        error: error instanceof Error ? error.message : "Failed to assign role",
        isLoading: false,
      });
      throw error;
    }
  },

  removeRole: async (userId: string, role: string) => {
    set({ isLoading: true, error: null });
    try {
      await apiClient.users.removeRole(userId, role);
      // Refresh user data
      await get().fetchUserById(userId);
    } catch (error) {
      set({
        error: error instanceof Error ? error.message : "Failed to remove role",
        isLoading: false,
      });
      throw error;
    }
  },

  changePassword: async (
    userId: string,
    currentPassword: string,
    newPassword: string
  ) => {
    set({ isLoading: true, error: null });
    try {
      await apiClient.users.changePassword(
        userId,
        currentPassword,
        newPassword
      );
      set({ isLoading: false });
    } catch (error) {
      set({
        error:
          error instanceof Error ? error.message : "Failed to change password",
        isLoading: false,
      });
      throw error;
    }
  },

  // Computed
  filteredUsers: () => {
    const { users, searchQuery, roleFilter, sortBy, sortOrder } = get();
    let filtered = users;

    // Filter by search query
    if (searchQuery) {
      filtered = filtered.filter(
        (user) =>
          user.firstName.toLowerCase().includes(searchQuery.toLowerCase()) ||
          user.lastName.toLowerCase().includes(searchQuery.toLowerCase()) ||
          user.email.toLowerCase().includes(searchQuery.toLowerCase())
      );
    }

    // Filter by role
    if (roleFilter !== "all") {
      filtered = filtered.filter((user) =>
        user.roles?.includes(roleFilter as UserRole)
      );
    }

    // Sort
    filtered.sort((a, b) => {
      let aValue: any;
      let bValue: any;

      if (sortBy === "createdOnUtc" || sortBy === "modifiedOnUtc") {
        aValue = new Date(a[sortBy] || "");
        bValue = new Date(b[sortBy] || "");
      } else {
        aValue = a[sortBy];
        bValue = b[sortBy];
      }

      const comparison = aValue < bValue ? -1 : aValue > bValue ? 1 : 0;
      return sortOrder === "asc" ? comparison : -comparison;
    });

    return filtered;
  },

  getUserById: (id: string) => {
    return get().users.find((user) => user.id === id);
  },

  getUsersByRole: (role: string) => {
    return get().users.filter((user) => user.roles?.includes(role as UserRole));
  },

  getAdmins: () => {
    return get().getUsersByRole("Admin");
  },
}));
