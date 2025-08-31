# Impersonation Feature Test Guide

## Overview

The impersonation feature now requires Admin role permissions. Only users with the "Admin" role can impersonate other users.

## What was implemented:

### 1. Role-based Authorization

- **Location**: `ImpersonateCommandHandler.cs`
- **Logic**: Check if current user has Admin role before allowing impersonation
- **Error**: Returns `UserErrors.InsufficientPermissions` if user is not admin

### 2. JWT Token Enhancement

- **Location**: `JwtProvider.cs`
- **Enhancement**: JWT tokens now include role claims for proper authorization
- **Claims**: Both regular and impersonation tokens include role information

### 3. Role Loading in Repository

- **Location**: `UserRepository.cs`
- **Enhancement**: All user queries now automatically include roles
- **Methods Updated**: `GetUserByEmailAsync`, `GetByEmailAsync`, `GetByIdAsync`

## API Endpoints:

### Impersonate User (Admin Only)

```http
POST /api/auth/impersonate
Authorization: Bearer <admin_jwt_token>
Content-Type: application/json

{
  "userIdToImpersonate": "12345678-1234-1234-1234-123456789012"
}
```

**Success Response (200):**

```json
{
  "token": "new_jwt_token_with_impersonated_user",
  "email": "impersonated.user@example.com"
}
```

**Error Response (403 - Non-Admin User):**

```json
{
  "error": "User.InsufficientPermissions",
  "description": "User does not have sufficient permissions to perform this action."
}
```

### Stop Impersonation

```http
POST /api/auth/stop-impersonate
Authorization: Bearer <impersonation_jwt_token>
```

## Testing Steps:

1. **Login as Admin User**: Use credentials of a user with Admin role
2. **Get JWT Token**: Save the JWT token from login response
3. **Attempt Impersonation**: Call impersonate endpoint with admin token
4. **Verify Success**: Should return new token with impersonated user
5. **Test Non-Admin**: Login as non-admin user and attempt impersonation
6. **Verify Failure**: Should return insufficient permissions error

## Security Features:

- ✅ Role-based access control
- ✅ JWT tokens include role claims
- ✅ Automatic role loading from database
- ✅ Proper error handling
- ✅ Authorization middleware on endpoints

## Database Requirements:

Ensure users have proper role assignments in the database:

- Admin users should have the "Admin" role assigned
- Non-admin users should have other roles (Developer, Reporter, etc.)

## JWT Token Structure:

The JWT tokens now include:

```json
{
  "sub": "user_id",
  "email": "user@example.com",
  "role": "Admin",
  "impersonated_user_id": "target_user_id", // only in impersonation tokens
  "impersonated_email": "target@example.com" // only in impersonation tokens
}
```
