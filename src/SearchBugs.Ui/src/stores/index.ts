// Global stores
export { useAuthStore } from "./global/authStore";
export { useUIStore } from "./global/uiStore";

// Feature stores
export { useProjectsStore } from "./features/projectsStore";
export { useBugsStore } from "./features/bugsStore";
export { useRepositoriesStore } from "./features/repositoriesStore";
export { useUsersStore } from "./features/usersStore";

// Type helpers for store subscriptions
export type StoreState<T> = T extends (...args: any) => any
  ? ReturnType<T>
  : never;
export type StoreActions<T extends Record<string, any>> = {
  [K in keyof T]: T[K] extends (...args: any[]) => any ? T[K] : never;
};
