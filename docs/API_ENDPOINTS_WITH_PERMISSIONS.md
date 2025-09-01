# SearchBugs API Endpoints with Permission-Based Authorization

This document provides a comprehensive overview of all API endpoints in the SearchBugs system, organized by category and showing the required permissions for each endpoint.

## Authentication Endpoints

- `POST /api/auth/login` - Login user
- `POST /api/auth/register` - Register new user
- `POST /api/auth/impersonate` - Start impersonation
- `POST /api/auth/stop-impersonate` - Stop impersonation

## User Management Endpoints

All user endpoints require specific permissions as shown:

### Basic User Operations

- `GET /api/users` - List all users (**ListAllUsers**)
- `GET /api/users/{id}` - Get user details (**ViewUserDetails**)
- `GET /api/users/{id}/roles` - Get user roles (**ViewUserDetails**)
- `POST /api/users` - Create new user (**CreateUser**)
- `PUT /api/users/{id}` - Update user (**UpdateUser**)
- `DELETE /api/users/{id}` - Delete user (**DeleteUser**)

### Role Assignment

- `POST /api/users/{id}/roles` - Assign role to user (**AssignRoleToUser**)
- `DELETE /api/users/{id}/roles/{roleId}` - Remove role from user (**RemoveRoleFromUser**)
- `PUT /api/users/{id}/change-password` - Change user password (**ChangeUserPassword**)

## Role & Permission Management Endpoints

### Roles

- `GET /api/roles` - Get all roles (**ViewRoles**)
- `GET /api/roles/{roleId}` - Get role with permissions (**ViewRolePermissions**)

### Permissions

- `GET /api/roles/permissions` - Get all permissions (**ViewPermissions**)
- `POST /api/roles/{roleId}/permissions/{permissionId}` - Assign permission to role (**AssignPermissionToRole**)
- `DELETE /api/roles/{roleId}/permissions/{permissionId}` - Remove permission from role (**RemovePermissionFromRole**)

## Project Management Endpoints

### Basic Project Operations

- `GET /api/projects` - List all projects (**ListAllProjects**)
- `GET /api/projects/{id}` - Get project details (**ViewProjectDetails**)
- `POST /api/projects` - Create new project (**CreateProject**)
- `PUT /api/projects/{id}` - Update project (**UpdateProject**)
- `DELETE /api/projects/{id}` - Delete project (**DeleteProject**)

### Project Analytics & Members

- `GET /api/projects/{id}/statistics` - Get project statistics (**ViewProjectDetails**)
- `GET /api/projects/{id}/bugs` - Get project bugs (**ListAllBugs**)
- `GET /api/projects/{id}/members` - Get project members (**ViewProjectDetails**)
- `POST /api/projects/{id}/members` - Add project member (**UpdateProject**)
- `DELETE /api/projects/{id}/members/{userId}` - Remove project member (**UpdateProject**)

## Bug Management Endpoints

### Basic Bug Operations

- `GET /api/bugs` - List all bugs (**ListAllBugs**)
- `GET /api/bugs/{bugId}` - Get bug details (**ViewBugDetails**)
- `POST /api/bugs` - Create new bug (**CreateBug**)
- `PUT /api/bugs/{bugId}` - Update bug (**UpdateBug**)
- `DELETE /api/bugs/{bugId}` - Delete bug (**DeleteBug**)

### Bug Comments

- `POST /api/bugs/{bugId}/comments` - Add comment to bug (**AddCommentToBug**)
- `GET /api/bugs/{bugId}/comments` - Get bug comments (**ViewBugComments**)

### Bug Attachments

- `POST /api/bugs/{bugId}/attachments` - Add attachment to bug (**AddAttachmentToBug**)
- `GET /api/bugs/{bugId}/attachments` - Get bug attachments (**ViewBugAttachments**)

### Bug History & Time Tracking

- `GET /api/bugs/{bugId}/history` - Get bug history (**ViewBugHistory**)
- `POST /api/bugs/{bugId}/time-tracking` - Track time on bug (**TrackTimeSpentOnBug**)
- `GET /api/bugs/{bugId}/time-tracking` - Get time tracking info (**ViewTimeSpentOnBug**)

### Bug Custom Fields

- `POST /api/bugs/{bugId}/custom-fields` - Add custom field (**AddCustomFieldToBug**)
- `GET /api/bugs/{bugId}/custom-fields` - Get custom fields (**ViewCustomFieldOnBug**)

## Repository Management Endpoints

### Basic Repository Operations

- `GET /api/repositories` - List all repositories (**ListAllRepositories**)
- `GET /api/repositories/{id}` - Get repository details (**ViewRepositoryDetails**)
- `POST /api/repositories` - Create new repository (**CreateRepository**)
- `PUT /api/repositories/{id}` - Update repository (**UpdateRepository**)
- `DELETE /api/repositories/{id}` - Delete repository (**DeleteRepository**)

### Repository-Bug Linking

- `POST /api/repositories/{id}/bugs` - Link bug to repository (**LinkBugToRepository**)
- `GET /api/repositories/{id}/bugs` - Get repository bugs (**ViewBugRepository**)
- `DELETE /api/repositories/{id}/bugs/{bugId}` - Unlink bug from repository (**LinkBugToRepository**)

### Repository Analytics & Operations

- `GET /api/repositories/{id}/statistics` - Get repository statistics (**ViewRepositoryDetails**)
- `GET /api/repositories/{id}/commits` - Get repository commits (**ViewRepositoryDetails**)
- `GET /api/repositories/{id}/branches` - Get repository branches (**ViewRepositoryDetails**)
- `POST /api/repositories/{id}/sync` - Sync repository (**UpdateRepository**)
- `GET /api/repositories/{id}/sync-status` - Get sync status (**ViewRepositoryDetails**)

## Notification Management Endpoints

### Basic Notification Operations

- `GET /api/notifications` - Get user notifications (**ViewNotification**)
- `GET /api/notifications/unread` - Get unread notifications (**ViewNotification**)
- `GET /api/notifications/unread-count` - Get unread count (**ViewNotification**)
- `PUT /api/notifications/{id}/read` - Mark as read (**MarkNotificationAsRead**)
- `PUT /api/notifications/mark-all-read` - Mark all as read (**MarkNotificationAsRead**)
- `DELETE /api/notifications/{id}` - Delete notification (**DeleteNotification**)
- `DELETE /api/notifications/clear-all` - Clear all notifications (**DeleteNotification**)

### Admin Notification Operations

- `POST /api/notifications/send` - Send notification (**ViewNotification**)
- `POST /api/notifications/send-to-user` - Send to specific user (**ViewNotification**)
- `POST /api/notifications/broadcast` - Broadcast notification (**ViewNotification**)
- `POST /api/notifications/bug-notification` - Send bug notification (**ViewNotification**)
- `GET /api/notifications/admin/all` - Get all notifications (admin) (**ViewNotification**)
- `GET /api/notifications/admin/statistics` - Get notification statistics (**ViewNotification**)
- `POST /api/notifications/admin/bulk-delete` - Bulk delete notifications (**DeleteNotification**)
- `POST /api/notifications/admin/bulk-mark-read` - Bulk mark as read (**MarkNotificationAsRead**)

## Analytics & Reporting Endpoints

### Dashboard Analytics

- `GET /api/analytics/dashboard` - Get dashboard analytics (**ViewBugDetails**)
- `POST /api/analytics/dashboard` - Get filtered dashboard (**ViewBugDetails**)

### Bug Analytics

- `GET /api/analytics/bugs/summary` - Bug summary (**ListAllBugs**)
- `GET /api/analytics/bugs/trends` - Bug trends (**ListAllBugs**)
- `GET /api/analytics/bugs/by-status` - Bugs by status (**ListAllBugs**)
- `GET /api/analytics/bugs/by-priority` - Bugs by priority (**ListAllBugs**)
- `GET /api/analytics/bugs/by-assignee` - Bugs by assignee (**ListAllBugs**)
- `GET /api/analytics/bugs/resolution-time` - Bug resolution time (**ListAllBugs**)

### Project Analytics

- `GET /api/analytics/projects/summary` - Project summary (**ListAllProjects**)
- `GET /api/analytics/projects/{projectId}/health` - Project health (**ViewProjectDetails**)
- `GET /api/analytics/projects/{projectId}/velocity` - Project velocity (**ViewProjectDetails**)

### User Analytics

- `GET /api/analytics/users/productivity` - User productivity (**ListAllUsers**)
- `GET /api/analytics/users/{userId}/activity` - User activity (**ViewUserDetails**)
- `GET /api/analytics/users/workload` - User workload (**ListAllUsers**)

### Time Tracking Analytics

- `GET /api/analytics/time-tracking/summary` - Time tracking summary (**ViewTimeSpentOnBug**)
- `GET /api/analytics/time-tracking/by-project` - Time by project (**ViewTimeSpentOnBug**)
- `GET /api/analytics/time-tracking/by-user` - Time by user (**ViewTimeSpentOnBug**)

### Report Generation

- `POST /api/analytics/reports/generate` - Generate report (**ViewBugDetails**)
- `GET /api/analytics/reports` - Get available reports (**ViewBugDetails**)
- `GET /api/analytics/reports/{reportId}` - Get report status (**ViewBugDetails**)
- `GET /api/analytics/reports/{reportId}/download` - Download report (**ViewBugDetails**)

### System Analytics

- `GET /api/analytics/system/health` - System health (**ListAllUsers**)
- `GET /api/analytics/system/performance` - System performance (**ListAllUsers**)

## System Administration Endpoints

### System Configuration

- `GET /api/admin/config` - Get system configuration (**ListAllUsers**)
- `PUT /api/admin/config/{key}` - Update configuration (**UpdateUser**)
- `POST /api/admin/config` - Create configuration (**CreateUser**)
- `DELETE /api/admin/config/{key}` - Delete configuration (**DeleteUser**)

### Bulk User Operations

- `POST /api/admin/users/bulk-action` - Bulk user action (**UpdateUser**)
- `POST /api/admin/users/bulk-activate` - Bulk activate users (**UpdateUser**)
- `POST /api/admin/users/bulk-deactivate` - Bulk deactivate users (**UpdateUser**)
- `POST /api/admin/users/bulk-delete` - Bulk delete users (**DeleteUser**)
- `GET /api/admin/users/inactive` - Get inactive users (**ListAllUsers**)
- `GET /api/admin/users/login-statistics` - Login statistics (**ListAllUsers**)

### Security & Audit Management

- `GET /api/admin/audit-logs` - Get audit logs (**ListAllUsers**)
- `GET /api/admin/audit-logs/export` - Export audit logs (**ListAllUsers**)
- `GET /api/admin/security/failed-logins` - Failed login attempts (**ListAllUsers**)
- `POST /api/admin/security/lock-account` - Lock user account (**UpdateUser**)
- `POST /api/admin/security/unlock-account` - Unlock user account (**UpdateUser**)

### System Maintenance

- `GET /api/admin/maintenance/status` - Maintenance status (**ListAllUsers**)
- `POST /api/admin/maintenance/schedule` - Schedule maintenance (**UpdateUser**)
- `POST /api/admin/maintenance/start` - Start maintenance (**UpdateUser**)
- `POST /api/admin/maintenance/end` - End maintenance (**UpdateUser**)

### Database Management

- `GET /api/admin/database/health` - Database health (**ListAllUsers**)
- `GET /api/admin/database/statistics` - Database statistics (**ListAllUsers**)
- `POST /api/admin/database/backup` - Create backup (**UpdateUser**)
- `GET /api/admin/database/backups` - Get backups (**ListAllUsers**)
- `POST /api/admin/database/restore` - Restore backup (**UpdateUser**)
- `POST /api/admin/database/cleanup` - Cleanup database (**DeleteUser**)

### System Monitoring

- `GET /api/admin/monitoring/performance` - Performance metrics (**ListAllUsers**)
- `GET /api/admin/monitoring/errors` - System errors (**ListAllUsers**)
- `GET /api/admin/monitoring/active-sessions` - Active sessions (**ListAllUsers**)
- `POST /api/admin/monitoring/clear-cache` - Clear cache (**UpdateUser**)

### License & Feature Management

- `GET /api/admin/license` - License information (**ListAllUsers**)
- `GET /api/admin/features` - Feature flags (**ListAllUsers**)
- `PUT /api/admin/features/{feature}` - Toggle feature (**UpdateUser**)

### Integration Management

- `GET /api/admin/integrations` - Get integrations (**ListAllUsers**)
- `POST /api/admin/integrations/test` - Test integration (**UpdateUser**)
- `PUT /api/admin/integrations/{integration}/enable` - Enable integration (**UpdateUser**)
- `PUT /api/admin/integrations/{integration}/disable` - Disable integration (**UpdateUser**)

## Permission Summary

The system uses the following permissions to control access:

### User Management

- `CreateUser` - Can create new users
- `ViewUserDetails` - Can view user details
- `UpdateUser` - Can update user information
- `DeleteUser` - Can delete users
- `AuthenticateUser` - Can authenticate users
- `ChangeUserPassword` - Can change user passwords
- `ListAllUsers` - Can list all users

### Project Management

- `CreateProject` - Can create new projects
- `ViewProjectDetails` - Can view project details
- `UpdateProject` - Can update project information
- `DeleteProject` - Can delete projects
- `ListAllProjects` - Can list all projects

### Bug Management

- `CreateBug` - Can create new bugs
- `ViewBugDetails` - Can view bug details
- `UpdateBug` - Can update bug information
- `DeleteBug` - Can delete bugs
- `ListAllBugs` - Can list all bugs
- `AddCommentToBug` - Can add comments to bugs
- `ViewBugComments` - Can view bug comments
- `AddAttachmentToBug` - Can add attachments to bugs
- `ViewBugAttachments` - Can view bug attachments
- `ViewBugHistory` - Can view bug history
- `TrackTimeSpentOnBug` - Can track time spent on bugs
- `ViewTimeSpentOnBug` - Can view time spent on bugs
- `AddCustomFieldToBug` - Can add custom fields to bugs
- `ViewCustomFieldOnBug` - Can view custom fields on bugs

### Notification Management

- `ViewNotification` - Can view notifications
- `DeleteNotification` - Can delete notifications
- `MarkNotificationAsRead` - Can mark notifications as read

### Repository Management

- `CreateRepository` - Can create repositories
- `ViewRepositoryDetails` - Can view repository details
- `UpdateRepository` - Can update repositories
- `DeleteRepository` - Can delete repositories
- `ListAllRepositories` - Can list all repositories
- `LinkBugToRepository` - Can link bugs to repositories
- `ViewBugRepository` - Can view bug repository links

### Role Management

- `ViewRoles` - Can view roles
- `ViewPermissions` - Can view permissions
- `AssignPermissionToRole` - Can assign permissions to roles
- `RemovePermissionFromRole` - Can remove permissions from roles
- `ViewRolePermissions` - Can view role permissions
- `AssignRoleToUser` - Can assign roles to users
- `RemoveRoleFromUser` - Can remove roles from users

## Notes

1. **Admin Role**: The Admin role should have ALL permissions assigned for full system access.

2. **Permission Hierarchy**: Some endpoints use higher-level permissions (like ListAllUsers) for admin operations, indicating they require administrative privileges.

3. **Future Implementation**: Many endpoints are currently placeholder implementations marked with "Note: This would require implementing [CommandName] in the Application layer" - these need to be implemented with proper MediatR commands/queries.

4. **Security**: All endpoints require authentication, and most have specific permission requirements enforced through the `RequireAuthorization()` method.

5. **RBAC Integration**: The permission-based authorization system integrates with the existing JWT authentication and role-based access control system.
