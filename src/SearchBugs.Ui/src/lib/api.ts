import axios from "axios";
import { apiBaseUrl } from "./constants";
export const accessTokenKey = "access";

// Create api instance
export const api = axios.create({
  baseURL: apiBaseUrl,
});

// Set default content type
api.defaults.headers.common["Content-Type"] = "application/json";

// Add request interceptor to include token in each request
api.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem(accessTokenKey);
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => Promise.reject(error)
);

// Response interceptor for handling 401 errors
api.interceptors.response.use(
  (response) => response,
  async (error) => {
    if (error.response?.status === 401) {
      // Token is invalid or expired, remove it and redirect to login
      localStorage.removeItem(accessTokenKey);
      // You might want to redirect to login page here
      // window.location.href = '/login';
    }
    return Promise.reject(error);
  }
);

// Response wrapper type for API responses
export interface ApiResponse<T> {
  value: T;
  isSuccess: boolean;
  isFailure: boolean;
  error: {
    code: string;
    message: string;
  };
}

// Authentication response types
export interface LoginResponse {
  token: string;
}

export interface RegisterResponse {
  token: string;
}

// Types
export interface Project {
  id: string;
  name: string;
  description: string;
  createdOnUtc: string;
  updatedOnUtc: string;
}

export interface Bug {
  id: string;
  projectId: string;
  title: string;
  description: string;
  status: BugStatus;
  priority: BugPriority;
  severity: BugSeverity;
  reporterId: string;
  assigneeId?: string;
  createdOnUtc: string;
  updatedOnUtc: string;
}

export interface Comment {
  id: string;
  bugId: string;
  userId: string;
  content: string;
  createdOnUtc: string;
  updatedOnUtc: string;
}

export interface Attachment {
  id: string;
  bugId: string;
  fileName: string;
  fileSize: number;
  contentType: string;
  uploadedBy: string;
  uploadedOnUtc: string;
}

export interface TimeEntry {
  id: string;
  bugId: string;
  userId: string;
  hours: number;
  description: string;
  date: string;
  createdOnUtc: string;
  updatedOnUtc: string;
}

export interface HistoryEntry {
  id: string;
  bugId: string;
  userId: string;
  field: string;
  oldValue: string;
  newValue: string;
  changedOnUtc: string;
}

export interface CustomField {
  id: string;
  projectId: string;
  name: string;
  type: CustomFieldType;
  isRequired: boolean;
  defaultValue?: string;
  createdOnUtc: string;
  updatedOnUtc: string;
}

export interface User {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  roles?: UserRole[];
  createdOnUtc: string;
  modifiedOnUtc?: string;
}

export interface Role {
  id: number;
  name: string;
  permissions: string[];
}

export interface Permission {
  id: number;
  name: string;
  description: string;
}

export interface Repository {
  id: string;
  projectId: string;
  name: string;
  url: string;
  type: RepositoryType;
  createdOnUtc: string;
  updatedOnUtc: string;
}

export interface GitTreeItem {
  path: string;
  name: string;
  type: string; // "Tree" or "Blob"
}

export interface FileDiff {
  filePath: string;
  oldPath: string;
  status: string;
  patch: string;
}

// Enums
export enum BugStatus {
  New = "New",
  InProgress = "InProgress",
  Resolved = "Resolved",
  Closed = "Closed",
  Reopened = "Reopened",
}

export enum BugPriority {
  Low = "Low",
  Medium = "Medium",
  High = "High",
  Critical = "Critical",
}

export enum BugSeverity {
  Low = "Low",
  Medium = "Medium",
  High = "High",
  Critical = "Critical",
}

export enum CustomFieldType {
  Text = "Text",
  Number = "Number",
  Date = "Date",
  Boolean = "Boolean",
  Select = "Select",
}

export enum UserRole {
  User = "User",
  Admin = "Admin",
}

export enum RepositoryType {
  Git = "Git",
  Svn = "Svn",
  Mercurial = "Mercurial",
}

// DTOs
export interface CreateProjectDto {
  name: string;
  description: string;
}

export interface CreateBugDto {
  projectId: string;
  title: string;
  description: string;
  status: string;
  priority: string;
  severity: string;
  reporterId: string;
  assigneeId?: string;
}

export interface UpdateBugDto {
  title?: string;
  description?: string;
  status?: BugStatus;
  priority?: BugPriority;
  severity?: BugSeverity;
  assigneeId?: string;
}

export interface CreateCommentDto {
  bugId: string;
  userId: string;
  content: string;
}

export interface CreateTimeEntryDto {
  bugId: string;
  userId: string;
  hours: number;
  description: string;
  date: string;
}

export interface CreateCustomFieldDto {
  projectId: string;
  name: string;
  type: CustomFieldType;
  isRequired: boolean;
  defaultValue?: string;
}

export interface UpdateCustomFieldDto {
  name?: string;
  type?: CustomFieldType;
  isRequired?: boolean;
  defaultValue?: string;
}

export interface CreateRepositoryDto {
  projectId: string;
  name: string;
  url: string;
  type: RepositoryType;
}

export interface UpdateRepositoryDto {
  name?: string;
  url?: string;
  type?: RepositoryType;
}

// API functions
export const apiClient = {
  // Authentication
  auth: {
    login: (data: { email: string; password: string }) =>
      api.post<ApiResponse<LoginResponse>>("/auth/login", data),
    register: (data: {
      email: string;
      password: string;
      firstName: string;
      lastName: string;
    }) => api.post<ApiResponse<RegisterResponse>>("/auth/register", data),
  },

  // Projects
  projects: {
    getAll: () => api.get<ApiResponse<Project[]>>("/projects"),
    getById: (id: string) => api.get<ApiResponse<Project>>(`/projects/${id}`),
    create: (data: CreateProjectDto) =>
      api.post<ApiResponse<Project>>("/projects", data),
    delete: (id: string) => api.delete<ApiResponse<void>>(`/projects/${id}`),
  },

  // Bugs
  bugs: {
    getAll: (projectId?: string) =>
      api.get<ApiResponse<Bug[]>>(
        projectId ? `/bugs?projectId=${projectId}` : "/bugs"
      ),
    getById: (id: string) => api.get<ApiResponse<Bug>>(`/bugs/${id}`),
    create: (data: CreateBugDto) => api.post<ApiResponse<Bug>>("/bugs", data),
    update: (id: string, data: UpdateBugDto) =>
      api.put<ApiResponse<Bug>>(`/bugs/${id}`, data),
    delete: (id: string) => api.delete<ApiResponse<void>>(`/bugs/${id}`),

    // Comments
    getComments: (bugId: string) =>
      api.get<ApiResponse<Comment[]>>(`/bugs/${bugId}/comments`),
    addComment: (bugId: string, content: string) =>
      api.post<ApiResponse<Comment>>(`/bugs/${bugId}/comments`, { content }),

    // Attachments
    getAttachments: (bugId: string) =>
      api.get<ApiResponse<Attachment[]>>(`/bugs/${bugId}/attachments`),
    addAttachment: (bugId: string, file: File) => {
      const formData = new FormData();
      formData.append("file", file);
      return api.post<ApiResponse<Attachment>>(
        `/bugs/${bugId}/attachments`,
        formData,
        {
          headers: { "Content-Type": "multipart/form-data" },
        }
      );
    },

    // History
    getHistory: (bugId: string) =>
      api.get<ApiResponse<HistoryEntry[]>>(`/bugs/${bugId}/history`),

    // Time tracking
    getTimeTracking: (bugId: string) =>
      api.get<ApiResponse<TimeEntry[]>>(`/bugs/${bugId}/time-tracking`),
    trackTime: (
      bugId: string,
      data: { duration: string; description: string }
    ) => api.post<ApiResponse<TimeEntry>>(`/bugs/${bugId}/time-tracking`, data),

    // Custom fields
    getCustomFields: (bugId: string) =>
      api.get<ApiResponse<CustomField[]>>(`/bugs/${bugId}/custom-fields`),
    addCustomField: (bugId: string, data: { name: string; value: string }) =>
      api.post<ApiResponse<CustomField>>(`/bugs/${bugId}/custom-fields`, data),
  },

  // Users
  users: {
    getAll: () => api.get<ApiResponse<User[]>>("/users"),
    getById: (id: string) => api.get<ApiResponse<User>>(`/users/${id}`),
    create: (data: {
      firstName: string;
      lastName: string;
      email: string;
      password: string;
      roles?: string[];
    }) => api.post<ApiResponse<User>>("/users", data),
    update: (id: string, data: { firstName: string; lastName: string }) =>
      api.put<ApiResponse<User>>(`/users/${id}`, data),
    delete: (id: string) => api.delete<ApiResponse<void>>(`/users/${id}`),
    assignRole: (userId: string, role: string) =>
      api.post<ApiResponse<void>>(`/users/${userId}/assign-role`, {
        userId,
        role,
      }),
    removeRole: (userId: string, role: string) =>
      api.post<ApiResponse<void>>(`/users/${userId}/remove-role`, {
        userId,
        role,
      }),
    changePassword: (
      userId: string,
      currentPassword: string,
      newPassword: string
    ) =>
      api.post<ApiResponse<void>>(`/users/${userId}/change-password`, {
        currentPassword,
        newPassword,
      }),
  },

  // Roles
  roles: {
    getAll: () => api.get<ApiResponse<Role[]>>("/roles"),
    getPermissions: () =>
      api.get<ApiResponse<Permission[]>>("/roles/permissions"),
  },

  // Repositories
  repositories: {
    getAll: () => api.get<Repository[]>("/repo"),
    getDetails: (url: string, path: string) =>
      api.get(`/repo/${encodeURIComponent(url)}/${path}`),
    create: (data: {
      name: string;
      description: string;
      url: string;
      projectId: string;
    }) => api.post<Repository>("/repo", data),
    delete: (url: string) => api.delete(`/repo/${encodeURIComponent(url)}`),
    getCommitDiff: (url: string, commitSha: string) =>
      api.get(`/repo/${encodeURIComponent(url)}/commit/${commitSha}`),
    commitChanges: (
      url: string,
      commitSha: string,
      data: {
        author: string;
        email: string;
        message: string;
        content: string;
      }
    ) => api.post(`/repo/${encodeURIComponent(url)}/commit/${commitSha}`, data),
    getTree: (url: string, commitSha: string) =>
      api.get<ApiResponse<GitTreeItem[]>>(
        `/repo/${encodeURIComponent(url)}/tree/${commitSha}`
      ),
    getFileContent: (url: string, commitSha: string, filePath: string) =>
      api.get<ApiResponse<string>>(
        `/repo/${encodeURIComponent(url)}/file/${commitSha}/${filePath}`
      ),
    getBranches: (url: string) =>
      api.get<ApiResponse<string[]>>(
        `/repo/${encodeURIComponent(url)}/branches`
      ),
    clone: (url: string, targetPath: string) =>
      api.post(`/repo/${encodeURIComponent(url)}/clone`, { targetPath }),
  },

  // Test Notifications
  testNotifications: {
    sendTestNotification: (data: { userId: string; message?: string }) =>
      api.post<{ message: string }>(
        "/test-notifications/send-test-notification",
        data
      ),
  },
};
