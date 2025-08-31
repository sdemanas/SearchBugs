import { z } from "zod";

// Common validation schemas
export const emailSchema = z
  .string()
  .email("Please enter a valid email address");
export const passwordSchema = z
  .string()
  .min(6, "Password must be at least 6 characters long");

// Auth schemas
export const loginSchema = z.object({
  email: emailSchema,
  password: z.string().min(1, "Password is required"),
});

export const registerSchema = z.object({
  firstName: z.string().min(1, "First name is required"),
  lastName: z.string().min(1, "Last name is required"),
  email: emailSchema,
  password: passwordSchema,
});

// Project schemas
export const createProjectSchema = z.object({
  name: z.string().min(1, "Project name is required"),
  description: z.string().optional(),
});

// Bug schemas
export const createBugSchema = z.object({
  title: z.string().min(1, "Title is required"),
  description: z.string().min(1, "Description is required"),
  status: z.string().min(1, "Status is required"),
  priority: z.string().min(1, "Priority is required"),
  severity: z.string().min(1, "Severity is required"),
  projectId: z.string().min(1, "Project is required"),
  assigneeId: z.string().optional(),
});

// User schemas
export const createUserSchema = z.object({
  firstName: z.string().min(1, "First name is required"),
  lastName: z.string().min(1, "Last name is required"),
  email: emailSchema,
  password: passwordSchema,
  roles: z.array(z.string()).optional(),
});

export const updateUserSchema = z.object({
  firstName: z.string().min(1, "First name is required"),
  lastName: z.string().min(1, "Last name is required"),
  email: emailSchema,
});

// Profile schemas
export const profileSettingsSchema = z.object({
  firstName: z.string().min(1, "First name is required"),
  lastName: z.string().min(1, "Last name is required"),
  email: emailSchema,
  bio: z.string().optional(),
  location: z.string().optional(),
  website: z
    .string()
    .url("Please enter a valid URL")
    .optional()
    .or(z.literal("")),
  company: z.string().optional(),
  jobTitle: z.string().optional(),
  twitterHandle: z.string().optional(),
  linkedInUrl: z
    .string()
    .url("Please enter a valid LinkedIn URL")
    .optional()
    .or(z.literal("")),
  gitHubUsername: z.string().optional(),
  phoneNumber: z.string().optional(),
  timeZone: z.string().optional(),
  avatarUrl: z
    .string()
    .url("Please enter a valid avatar URL")
    .optional()
    .or(z.literal("")),
  dateOfBirth: z.string().optional(), // ISO date string
});

// Comment schemas
export const commentSchema = z.object({
  content: z
    .string()
    .min(1, "Comment cannot be empty")
    .max(2000, "Comment must be less than 2000 characters"),
});

// Custom field schemas
export const customFieldSchema = z.object({
  name: z.string().min(1, "Field name is required"),
  value: z.string().min(1, "Field value is required"),
});

// Repository schemas
export const createRepositorySchema = z.object({
  name: z.string().min(1, "Repository name is required"),
  description: z.string().optional(),
  projectId: z.string().min(1, "Project is required"),
  url: z.string().url("Please enter a valid repository URL"),
  type: z.string().min(1, "Repository type is required"),
});

// Search schemas
export const searchSchema = z.object({
  query: z.string().min(1, "Search query is required"),
});

// Type exports for TypeScript
export type LoginFormData = z.infer<typeof loginSchema>;
export type RegisterFormData = z.infer<typeof registerSchema>;
export type CreateProjectFormData = z.infer<typeof createProjectSchema>;
export type CreateBugFormData = z.infer<typeof createBugSchema>;
export type CreateUserFormData = z.infer<typeof createUserSchema>;
export type UpdateUserFormData = z.infer<typeof updateUserSchema>;
export type ProfileSettingsFormData = z.infer<typeof profileSettingsSchema>;
export type CommentFormData = z.infer<typeof commentSchema>;
export type CustomFieldFormData = z.infer<typeof customFieldSchema>;
export type CreateRepositoryFormData = z.infer<typeof createRepositorySchema>;
export type SearchFormData = z.infer<typeof searchSchema>;
