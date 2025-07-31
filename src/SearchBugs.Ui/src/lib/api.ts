import axios from "axios";
import { apiBaseUrl } from "./constants";
export const accessTokenKey = "access";
export const refreshTokenKey = "refresh";

const accessToken = localStorage.getItem(accessTokenKey);
const refreshToken = localStorage.getItem(refreshTokenKey);
export const api = axios.create({
  baseURL: apiBaseUrl,
  headers: {
    Authorization: `Bearer ${accessToken}` || "",
  },
});
api.defaults.headers.common["Content-Type"] = "application/json";

export const refreshAccessTokenFn = async () => {
  fetch(`${apiBaseUrl}token/refresh`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      Authorization: `Bearer ${refreshToken}` || "",
    },
  })
    .then((res) => res.json())
    .then((data) => {
      const { accessToken } = data;
      api.defaults.headers.common["Authorization"] = `Bearer ${accessToken}`;
      localStorage.setItem(accessTokenKey, accessToken);
    });
};

api.interceptors.response.use(
  (response) => {
    return response;
  },
  async (error) => {
    const originalRequest = error.config;
    if (error.response.status === 401 && !originalRequest._retry) {
      originalRequest._retry = true;
      await refreshAccessTokenFn();
      return api(originalRequest);
    }
    return Promise.reject(error);
  }
);

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
  username: string;
  email: string;
  role: UserRole;
  createdOnUtc: string;
  updatedOnUtc: string;
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

// Enums
export enum BugStatus {
  New = "New",
  InProgress = "InProgress",
  Resolved = "Resolved",
  Closed = "Closed",
  Reopened = "Reopened"
}

export enum BugPriority {
  Low = "Low",
  Medium = "Medium",
  High = "High",
  Critical = "Critical"
}

export enum BugSeverity {
  Low = "Low",
  Medium = "Medium",
  High = "High",
  Critical = "Critical"
}

export enum CustomFieldType {
  Text = "Text",
  Number = "Number",
  Date = "Date",
  Boolean = "Boolean",
  Select = "Select"
}

export enum UserRole {
  User = "User",
  Admin = "Admin"
}

export enum RepositoryType {
  Git = "Git",
  Svn = "Svn",
  Mercurial = "Mercurial"
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
  status: BugStatus;
  priority: BugPriority;
  severity: BugSeverity;
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
  // Projects
  projects: {
    getAll: () => api.get<Project[]>('/projects'),
    create: (data: CreateProjectDto) => 
      api.post<Project>('/projects', data),
    delete: (id: string) => 
      api.delete(`/projects/${id}`),
  },

  // Bugs
  bugs: {
    getAll: (projectId?: string) => 
      api.get<Bug[]>(projectId ? `/projects/${projectId}/bugs` : "/bugs"),
    getById: (id: string) => 
      api.get<Bug>(`/bugs/${id}`),
    create: (data: CreateBugDto) => api.post<Bug>('/bugs', data),
    update: (id: string, data: UpdateBugDto) => api.put<Bug>(`/bugs/${id}`, data),
    delete: (id: string) => 
      api.delete(`/bugs/${id}`),
    
    // Comments
    getComments: (bugId: string) => 
      api.get<Comment[]>(`/bugs/${bugId}/comments`),
    addComment: (bugId: string, content: string) => 
      api.post<Comment>(`/bugs/${bugId}/comments`, { content }),
    
    // Attachments
    getAttachments: (bugId: string) => 
      api.get<Attachment[]>(`/bugs/${bugId}/attachments`),
    addAttachment: (bugId: string, file: File) => {
      const formData = new FormData();
      formData.append('file', file);
      return api.post<Attachment>(`/bugs/${bugId}/attachments`, formData, {
        headers: { 'Content-Type': 'multipart/form-data' },
      });
    },
    
    // History
    getHistory: (bugId: string) => 
      api.get<HistoryEntry[]>(`/bugs/${bugId}/history`),
    
    // Time tracking
    getTimeTracking: (bugId: string) => 
      api.get<TimeEntry[]>(`/bugs/${bugId}/time-tracking`),
    trackTime: (bugId: string, data: { duration: string; description: string }) => 
      api.post<TimeEntry>(`/bugs/${bugId}/time-tracking`, data),
    
    // Custom fields
    getCustomFields: (bugId: string) => 
      api.get<CustomField[]>(`/bugs/${bugId}/custom-fields`),
    addCustomField: (bugId: string, data: { name: string; value: string }) => 
      api.post<CustomField>(`/bugs/${bugId}/custom-fields`, data),
  },

  // Users
  users: {
    getAll: () => 
      api.get<User[]>('/users'),
    getById: (id: string) => 
      api.get<User>(`/users/${id}`),
    update: (id: string, data: Partial<User>) => 
      api.put<User>(`/users/${id}`, data),
    assignRole: (id: string, role: string) => 
      api.post(`/users/${id}/assign-role`, { userId: id, role }),
  },

  // Repositories
  repositories: {
    getAll: (projectId: string) => api.get<Repository[]>(`/projects/${projectId}/repositories`),
    getDetails: (url: string, path: string) => 
      api.get(`/repo/${url}/${path}`),
    create: (data: CreateRepositoryDto) => 
      api.post<Repository>('/repo', data),
    delete: (url: string) => 
      api.delete(`/repo/${url}`),
    getCommitDiff: (url: string, commitSha: string) => 
      api.get(`/repo/${url}/commit/${commitSha}`),
    commitChanges: (url: string, commitSha: string, data: {
      author: string;
      email: string;
      message: string;
      content: string;
    }) => api.post(`/repo/${url}/commit/${commitSha}`, data),
    getTree: (url: string, commitSha: string) => 
      api.get(`/repo/${url}/tree/${commitSha}`),
  },
};